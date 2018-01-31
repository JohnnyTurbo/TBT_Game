﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public static GameController instance;

    public int gridSizeX, gridSizeY;
    public GameObject whiteTilePrefab, blackTilePrefab;
    public GameObject basicLandUnitPrefab;
    public int numPlayerUnits, numEnemyUnits;
    public int tileSize;
    public const int numTeams = 2;
    public int playerTeamID;
    public TileController[,] mapGrid;
    public UnitController curUnit = null;
    public TileController curHoveredTile = null;
    public TileController curSelectedTile = null;
    public GameObject uiObject;
    public GameState curState;
    public List<UnitController>[] unitsInGame;

    GameObject actionCanvas, statCanvas, moveUnitCanvas, attackUnitCanvas, debugCanvas;
    Text numUnitsText, attackText, defenseText, movementText, rangeText, debugStateText;
    Button moveConfirmButton, moveActionButton, attackActionButton;
    TileController tempTile = null;
    List<TileController> availableTiles;

    void Awake()
    {

        unitsInGame = new List<UnitController>[2];

        for(int i = 0; i < numTeams; i++)
        {
            unitsInGame[i] = new List<UnitController>();
        }
        instance = this;
    }

    void Start()
    {
        //Find the canvases on the UI GameObject
        actionCanvas = uiObject.transform.Find("ActionCanvas").gameObject;
        statCanvas = uiObject.transform.Find("StatCanvas").gameObject;
        moveUnitCanvas = uiObject.transform.Find("MoveUnitCanvas").gameObject;
        attackUnitCanvas = uiObject.transform.Find("AttackUnitCanvas").gameObject;
        debugCanvas = uiObject.transform.Find("DebugCanvas").gameObject;

        //Find the UI components on the actionCanvas
        moveActionButton = actionCanvas.transform.Find("MoveButton").GetComponent<Button>();
        attackActionButton = actionCanvas.transform.Find("AttackButton").GetComponent<Button>();

        //Find the UI components on the statCanvas
        numUnitsText = statCanvas.transform.Find("NumUnitsText").GetComponent<Text>();
        attackText = statCanvas.transform.Find("AttackText").GetComponent<Text>();
        defenseText = statCanvas.transform.Find("DefenseText").GetComponent<Text>();
        movementText = statCanvas.transform.Find("MovementText").GetComponent<Text>();
        rangeText = statCanvas.transform.Find("RangeText").GetComponent<Text>();

        //Find the UI components on the moveUnitCanvas
        moveConfirmButton = moveUnitCanvas.transform.Find("ConfirmButton").GetComponent<Button>();

        //Find the UI componenets on the debugStateCanvas
        debugStateText = debugCanvas.transform.Find("DebugStateText").GetComponent<Text>();

        actionCanvas.SetActive(false);
        statCanvas.SetActive(false);
        moveUnitCanvas.SetActive(false);
        attackUnitCanvas.SetActive(false);
        InitializeMap();
        SpawnUnits();
        curState = GameState.PlayerSelectTile;
    }

    void Update()
    {
        switch (curState)
        {
            case GameState.PlayerSelectTile:
                debugStateText.text = "GameState: PlayerSelectTile";
                if (Input.GetButtonDown("Fire1"))
                {
                    //Debug.Log("Fire1 Pressed");
                    if (curUnit != null)
                    {
                        curUnit.OnUnitDeselect();
                    }
                    if (curHoveredTile != null)
                    {
                        curSelectedTile = curHoveredTile;
                        curHoveredTile.OnTileSelect();
                    }
                }
                break;

            case GameState.PlayerSelectAction:
                debugStateText.text = "GameState: PlayerSelectAction";
                if(Input.GetButtonDown("Fire1") && curHoveredTile != null && curSelectedTile != curHoveredTile)
                {
                    //Debug.Log("curselected is not curhovered");
                    curUnit.OnUnitDeselect();
                    if(curHoveredTile.unitOnTile != null)
                    {
                        //curSelectedTile = curHoveredTile;
                        curHoveredTile.OnTileSelect();
                    }
                }
                break;

            case GameState.PlayerMoveUnit:
                debugStateText.text = "GameState: PlayerMoveUnit";
                if (Input.GetButtonDown("Fire1"))
                {

                    if(curHoveredTile == null)
                    {
                        Debug.Log("curState is PlayerMoveUnit, Fire1 down, curHoveredTile is null");
                        //curUnit.OnUnitDeselect();
                    }
                    else if(curHoveredTile.unitOnTile != null)
                    {
                        Debug.Log("curState is PlayerMoveUnit, Fire1 down, curHoveredTile.unitOntile is not null");

                        //curUnit.OnUnitDeselect();
                        //curHoveredTile.unitOnTile.GetComponent<UnitController>().OnUnitSelect();
                        curHoveredTile.OnTileSelect();
                    }
                    else if (curHoveredTile.AttemptUnitMove(curUnit))
                    {
                        Debug.Log("curState is PlayerMoveUnit, Fire1 down, curHoveredTile is attemptUnitMove(curUnit) is true");

                        curSelectedTile.unitOnTile = null;
                        Debug.Log("curSelected Tile is at: " + curSelectedTile.curCoords.ToString());
                        curHoveredTile.unitOnTile = curUnit.gameObject;
                        curUnit.curCoords = curHoveredTile.curCoords;
                        curUnit.isMoving = false;
                        curUnit.canMove = false;
                        ShowActionsMenu();
                        if (curUnit.canAttack)
                        {
                            //Make Unit Attack
                            curUnit.AttackUnit();
                        }
                        else
                        {
                            //Unit out of actions
                        }
                    }
                }
                break;

            case GameState.PlayerAttackUnit:
                debugStateText.text = "GameState: PlayerAttackUnit";
                if(Input.GetButtonDown("Fire1") && curHoveredTile != null)
                {
                    if (curHoveredTile.AttemptUnitAttack(curUnit))
                    {
                        //Attack Succeeded
                        Debug.Log("Attack Succeeded");
                        curUnit.canAttack = false;
                        curUnit.isAttacking = false;
                        HideAttackMenu();
                        ShowActionsMenu();
                        curState = GameState.PlayerSelectAction;
                    }
                    else
                    {
                        //Attack Failed
                        Debug.Log("Attack Failed");
                    }
                }
                break;

            case GameState.EnemyTurn:
                debugStateText.text = "GameState: EnemyTurn";
                break;

            default:
                debugStateText.text = "GameState: ERROR";
                return;
        }
    }

    void InitializeMap()
    {
        //tileList = new List<GameObject>();
        mapGrid = new TileController[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                GameObject tileToBeSpawned = (x + y) % 2 == 0 ? whiteTilePrefab : blackTilePrefab;
                Vector3 newTileLoc = new Vector3(x * tileSize, 0f, y * tileSize);
                GameObject newTile = Instantiate(tileToBeSpawned, newTileLoc, Quaternion.identity);
                newTile.GetComponent<TileController>().curCoords = new IntVector2(x, y);
                mapGrid[x, y] = newTile.GetComponent<TileController>();
                //tileList.Add(newTile);
            }
        }
        //Debug.Log(tileList.Count + " tiles on map");

    }

    void SpawnUnits()
    {
        for (int i = 0; i < numPlayerUnits; i++)
        {
            Vector3 newUnitLoc = new Vector3(i * tileSize, 0, 0);
            GameObject newUnit = Instantiate(basicLandUnitPrefab, newUnitLoc, Quaternion.identity);

            UnitController newUnitController = newUnit.GetComponent<UnitController>();
            newUnitController.unitTeamID = 0;
            newUnitController.curCoords = new IntVector2(i, 0);
            mapGrid[i, 0].isOccupied = true;
            mapGrid[i, 0].unitOnTile = newUnit;
            unitsInGame[0].Add(newUnitController);
        }

        for (int i = 0; i < numEnemyUnits; i++)
        {
            Vector3 newUnitLoc = new Vector3(i * tileSize, 0, (gridSizeY - 1) * 2);
            GameObject newUnit = Instantiate(basicLandUnitPrefab, newUnitLoc, Quaternion.identity);
            UnitController newUnitController = newUnit.GetComponent<UnitController>();
            newUnitController.unitTeamID = 1;
            newUnitController.curCoords = new IntVector2(i, gridSizeY - 1);
            mapGrid[i, gridSizeY - 1].isOccupied = true;
            mapGrid[i, gridSizeY - 1].unitOnTile = newUnit;
            unitsInGame[1].Add(newUnitController);
        }
    }

    public List<TileController> FindAvailableTiles(IntVector2 startLoc, int maxDist)
    {
        availableTiles = new List<TileController>();
        FindNextTile(startLoc, startLoc, maxDist);
        return availableTiles;
    }

    void FindNextTile(IntVector2 startLoc, IntVector2 curLoc, int maxDist)
    {
        //Debug.Log("FindNextTile(startLoc: " + startLoc.ToString() + " curLoc: " + curLoc.ToString() + " maxDist: " + maxDist + ")");
        if (IntVector2.Distance(startLoc, curLoc) > maxDist || !IntVector2.OnGrid(curLoc, gridSizeX, gridSizeY))
        {
            return;
        }

        else if(!availableTiles.Contains(mapGrid[curLoc.x, curLoc.y]))
        {

            availableTiles.Add(mapGrid[curLoc.x, curLoc.y]);
            FindNextTile(startLoc, curLoc + IntVector2.coordUp, maxDist);
            FindNextTile(startLoc, curLoc + IntVector2.coordRight, maxDist);
            FindNextTile(startLoc, curLoc + IntVector2.coordDown, maxDist);
            FindNextTile(startLoc, curLoc + IntVector2.coordLeft, maxDist);
        }
    }

    public void MoveButtonSelect()
    {
        curUnit.MoveUnit();
    }

    public void AttackButtonSelect()
    {
        curUnit.AttackUnit();
    }

    public void CancelButtonSelect()
    {
        curUnit.OnUnitDeselect();
        HideActionsMenu();
        HideUnitStats();
    }

    public void ConfirmMoveButtonSelect()
    {
        curUnit.canMove = false;
        curSelectedTile.unitOnTile = curUnit.gameObject;
        tempTile.unitOnTile = curUnit.gameObject;
        curUnit.curCoords = tempTile.curCoords;
        tempTile = null;
        curSelectedTile.unitOnTile = null;
        HideMoveMenu();
        ShowActionsMenu();
        curState = GameState.PlayerSelectAction;
        curUnit.isMoving = false;
    }

    public void CancelMoveButtonSelect()
    {
        CancelMove();
        ShowActionsMenu();
        curState = GameState.PlayerSelectAction;
    }

    public void CancelMove() {
        HideMoveMenu();
        curUnit.isMoving = false;
    }

    public void EndTurnButtonSelect()
    {
        if(curUnit != null)
        {
            curUnit.OnUnitDeselect();
        }

        foreach(UnitController p1unit in unitsInGame[0])
        {
            p1unit.canMove = true;
            p1unit.canAttack = true;
        }
    }

    public void CancelAttackButtonSelect()
    {
        CancelAttack();
        ShowActionsMenu();
        curState = GameState.PlayerSelectAction;
    }

    public void CancelAttack()
    {
        HideAttackMenu();
        curUnit.isAttacking = false;
    }

    public void ShowUnitStats(UnitController selectedUnit)
    {
        statCanvas.SetActive(true);
        numUnitsText.text = "Units: " + selectedUnit.numUnits;
        attackText.text = "Attack: " + selectedUnit.attackStat;
        defenseText.text = "Defense: " + selectedUnit.defenseStat;
        movementText.text = "Movement: " + selectedUnit.movementRange;
        rangeText.text = "Range: " + selectedUnit.AttackRange;
    }

    public void HideUnitStats()
    {
        statCanvas.SetActive(false);
    }

    public void ShowActionsMenu()
    {
        actionCanvas.SetActive(true);
        moveActionButton.interactable = curUnit.canMove;
        attackActionButton.interactable = curUnit.canAttack;
    }

    public void HideActionsMenu()
    {
        actionCanvas.SetActive(false);
    }

    public void ShowMoveMenu()
    {
        moveUnitCanvas.SetActive(true);
    }

    public void HideMoveMenu()
    {
        moveUnitCanvas.SetActive(false);
    }

    public void ShowAttackMenu()
    {
        attackUnitCanvas.SetActive(true);
    }

    public void HideAttackMenu()
    {
        attackUnitCanvas.SetActive(false);
    }
}

public enum GameState {PlayerSelectTile, PlayerSelectAction, PlayerMoveUnit, PlayerAttackUnit, EnemyTurn};

[System.Serializable]
public struct IntVector2
{
    public int x;
    public int y;
    /// <summary>
    /// new IntVector2(0, 0);
    /// </summary>
    public static IntVector2 zero = new IntVector2(0, 0);
    /// <summary>
    /// new IntVector2(0, 1);
    /// </summary>
    public static IntVector2 coordUp = new IntVector2(0, 1);
    /// <summary>
    /// new IntVector2(0, -1);
    /// </summary>
    public static IntVector2 coordDown = new IntVector2(0, -1);
    /// <summary>
    /// new IntVector2(1, 0);
    /// </summary>
    public static IntVector2 coordRight = new IntVector2(1, 0);
    /// <summary>
    /// new IntVector2(-1, 0);
    /// </summary>
    public static IntVector2 coordLeft = new IntVector2(-1, 0);
    /// <summary>
    /// new IntVector2(-1, 1);
    /// </summary>
    public static IntVector2 coordUpLeft = new IntVector2(-1, 1);
    /// <summary>
    /// new IntVector2(1, 1);
    /// </summary>
    public static IntVector2 coordUpRight = new IntVector2(1, 1);
    /// <summary>
    /// new IntVector2(1, -1);
    /// </summary>
    public static IntVector2 coordDownRight = new IntVector2(1, -1);
    /// <summary>
    /// new IntVector2(-1, -1);
    /// </summary>
    public static IntVector2 coordDownLeft = new IntVector2(-1, -1);

    public IntVector2(int newX, int newY)
    {
        x = newX;
        y = newY;
    }

    /// <summary>
    /// Prints the IntV2 in format: (x, y)
    /// </summary>
    public void Print()
    {
        Debug.Log("(" + x + ", " + y + ")");
    }

    /// <summary>
    /// Returns the IntV2 in string format (x, y)
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return ("(" + x + ", " + y + ")");
    }

    public static bool operator ==(IntVector2 v1, IntVector2 v2)
    {
        if (v1.x == v2.x && v1.y == v2.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool operator !=(IntVector2 v1, IntVector2 v2)
    {
        if (v1.x == v2.x && v1.y == v2.y)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static IntVector2 operator +(IntVector2 v1, IntVector2 v2)
    {
        IntVector2 sum = new IntVector2(v1.x + v2.x, v1.y + v2.y);
        return sum;
    }

    public static IntVector2 operator -(IntVector2 v1, IntVector2 v2)
    {
        IntVector2 diff = new IntVector2(v1.x - v2.x, v1.y - v2.y);
        return diff;
    }

    /// <summary>
    /// Returns the distance between two points. Distance is the number of IntVector2s that would need to be visited
    /// when traveling from v1 to v2
    /// </summary>
    /// <param name="v1">Starting Point</param>
    /// <param name="v2">Ending Point</param>
    /// <returns></returns>
    public static int Distance(IntVector2 v1, IntVector2 v2)
    {
        IntVector2 diff = v1 - v2;
        int dist = (Mathf.Abs(diff.x) + Mathf.Abs(diff.y));
        return dist;
    }

    /// <summary>
    /// Returns true if the IntVector2 is on the grid
    /// </summary>
    /// <param name="curLoc">Current IntVector2 to test on</param>
    /// <param name="gridSizeX">Horizontal Grid Size</param>
    /// <param name="gridSizeY">Vertical Grid Size</param>
    /// <returns></returns>
    public static bool OnGrid(IntVector2 curLoc, int gridSizeX, int gridSizeY)
    {
        if(curLoc.x < 0 || curLoc.y < 0 || curLoc.x >= gridSizeX || curLoc.y >= gridSizeY)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}