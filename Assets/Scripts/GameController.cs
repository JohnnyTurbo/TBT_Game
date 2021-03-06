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
                        curSelectedTile = curHoveredTile;
                        curHoveredTile.OnTileSelect();
                    }
                }
                break;

            case GameState.PlayerMoveUnit:
                debugStateText.text = "GameState: PlayerMoveUnit";
                if (Input.GetButtonDown("Fire1") && curHoveredTile != null)
                {
                    if (curHoveredTile.AttemptUnitMove(curUnit))
                    {
                        //Debug.Log("Move Succeeded");
                        if (curHoveredTile == curSelectedTile)
                        {
                            curSelectedTile.unitOnTile = curUnit.gameObject;
                            moveConfirmButton.interactable = false;
                        }
                        else
                        {
                            curSelectedTile.unitOnTile = null;
                            moveConfirmButton.interactable = true;
                        }
                        tempTile = curHoveredTile;
                    }
                    else
                    {
                        //Debug.Log("Move Failed");
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
                newTile.GetComponent<TileController>().xPos = x;
                newTile.GetComponent<TileController>().yPos = y;
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
            newUnitController.curXPos = i;
            newUnitController.curYPos = 0;
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
            newUnitController.curXPos = i;
            newUnitController.curYPos = gridSizeY - 1;
            mapGrid[i, gridSizeY - 1].isOccupied = true;
            mapGrid[i, gridSizeY - 1].unitOnTile = newUnit;
            unitsInGame[1].Add(newUnitController);
        }
    }

    public void MoveButtonSelect()
    {
        curState = GameState.PlayerMoveUnit;
        HideActionsMenu();
        ShowMoveMenu();
    }

    public void AttackButtonSelect()
    {
        //HideActionsMenu();
        curState = GameState.PlayerAttackUnit;
        HideActionsMenu();
        ShowAttackMenu();
    }

    public void CancelButtonSelect()
    {
        //curState = GameState.PlayerSelectTile;
        curUnit.OnUnitDeselect();
        HideActionsMenu();
        HideUnitStats();
    }

    public void ConfirmMoveButtonSelect()
    {
        curUnit.canMove = false;
        curSelectedTile.unitOnTile = curUnit.gameObject;
        tempTile.unitOnTile = curUnit.gameObject;
        curUnit.curXPos = tempTile.xPos;
        curUnit.curYPos = tempTile.yPos;
        tempTile = null;
        curSelectedTile.unitOnTile = null;
        HideMoveMenu();
        ShowActionsMenu();
        curState = GameState.PlayerSelectAction;
    }

    public void CancelMoveButtonSelect()
    {
        HideMoveMenu();
        ShowActionsMenu();
        curUnit.transform.position = curSelectedTile.transform.position;
        curSelectedTile.unitOnTile = curUnit.gameObject;
        curState = GameState.PlayerSelectAction;
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
        HideAttackMenu();
        ShowActionsMenu();
        curState = GameState.PlayerSelectAction;
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
}