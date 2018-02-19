/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerNetworkObjectController : NetworkBehaviour {

    public int playerTeamID;

    public GameObject uiObject;
    public GameState curState;
    public GameObject cursor;

    public GameObject basicLandUnitPrefab;

    public UnitController curUnit = null;
    public TileController curHoveredTile = null;
    public TileController curSelectedTile = null;

    public static readonly Vector3 farAway = new Vector3(1000f, 1000f, 1000f);

    GameObject actionCanvas, statCanvas, messageCanvas, debugCanvas;
    Text numUnitsText, attackText, defenseText, movementText, rangeText, messageText, debugStateText;
    Button moveConfirmButton, moveActionButton, attackActionButton;
    InputField debugPlayerID;
    TileController prevHoveredTile = null;
    List<TileController> availableTiles;
    Coroutine messageCoro;

    void Start()
    {
        uiObject = GameObject.Find("UI");

        //Find the canvases on the UI GameObject
        actionCanvas = uiObject.transform.Find("ActionCanvas").gameObject;
        statCanvas = uiObject.transform.Find("StatCanvas").gameObject;
        messageCanvas = uiObject.transform.Find("MessageCanvas").gameObject;
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

        //Find the UI components on the messageCanvas
        messageText = messageCanvas.transform.Find("MessageText").GetComponent<Text>();

        //Find the UI componenets on the debugStateCanvas
        debugStateText = debugCanvas.transform.Find("DebugStateText").GetComponent<Text>();
        debugPlayerID = debugCanvas.transform.Find("DebugPlayerID").GetComponent<InputField>();

        actionCanvas.SetActive(false);
        statCanvas.SetActive(false);
        messageCanvas.SetActive(false);

        cursor = Instantiate(cursor, farAway, Quaternion.identity);
        curState = GameState.PlayerSelectTile;

        playerTeamID = Convert.ToInt32(debugPlayerID.text);
        ShowMessage("PlayerTeamID = " + playerTeamID, 3);
    }

    void Update()
    {
        //FOR DEBUGGING, DELETE LATER!!!
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Vector3 newUnitLoc = new Vector3(6, 0, 10 * playerTeamID);
            GameObject newUnit = Instantiate(basicLandUnitPrefab, newUnitLoc, Quaternion.identity);
            NetworkServer.Spawn(newUnit);
        }

        switch (curState)
        {
            case GameState.PlayerSelectTile:
                debugStateText.text = "GameState: PlayerSelectTile";
                if (Input.GetButtonDown("Fire1"))
                {
                    //Debug.Log("Fire1 Pressed");
                    if (curUnit != null)
                    {
                        curUnit.OnUnitDeselect(this);
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
                if (Input.GetButtonDown("Fire1") && curHoveredTile != null && curSelectedTile != curHoveredTile)
                {
                    //Debug.Log("curselected is not curhovered");
                    curUnit.OnUnitDeselect(this);
                    if (curHoveredTile.unitOnTile != null)
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

                    if (curHoveredTile == null)
                    {
                        //Debug.Log("curState is PlayerMoveUnit, Fire1 down, curHoveredTile is null");
                        //curUnit.OnUnitDeselect();
                    }
                    else if (curHoveredTile.unitOnTile != null)
                    {
                        //Debug.Log("curState is PlayerMoveUnit, Fire1 down, curHoveredTile.unitOntile is not null");

                        //curUnit.OnUnitDeselect();
                        //curHoveredTile.unitOnTile.GetComponent<UnitController>().OnUnitSelect();
                        curHoveredTile.OnTileSelect();
                    }
                    else if (curHoveredTile.AttemptUnitMove(curUnit))
                    {
                        //Debug.Log("curState is PlayerMoveUnit, Fire1 down, curHoveredTile is attemptUnitMove(curUnit) is true");

                        curSelectedTile.unitOnTile = null;
                        //Debug.Log("curSelected Tile is at: " + curSelectedTile.curCoords.ToString());
                        curHoveredTile.unitOnTile = curUnit.gameObject;
                        curUnit.curCoords = curHoveredTile.curCoords;
                        curUnit.isMoving = false;
                        curUnit.canMove = false;
                        ShowActionsMenu();
                        if (curUnit.canAttack)
                        {
                            //Make Unit Attack
                            curUnit.AttackUnit(this);
                        }
                        else
                        {
                            //Unit out of actions
                            curUnit.UnhighlightTiles();
                            curState = GameState.PlayerSelectAction;
                            ShowMessage("Unit out of Moves!", 1f);
                        }
                    }
                }
                break;

            case GameState.PlayerAttackUnit:
                debugStateText.text = "GameState: PlayerAttackUnit";

                if (Input.GetButtonDown("Fire1"))
                {
                    if (curHoveredTile == null)
                    {
                        Debug.Log("curState is PlayerAttackUnit, Fire1 down, curHoveredTile is null");
                    }
                    else if (curHoveredTile.unitOnTile != null)
                    {
                        if (curHoveredTile.unitOnTile.GetComponent<UnitController>().unitTeamID == playerTeamID)
                        {
                            //Clicking on a unit on the same team
                            curHoveredTile.OnTileSelect();
                        }
                        else if (curHoveredTile.unitOnTile.GetComponent<UnitController>().unitTeamID != playerTeamID)
                        {
                            //Clicking on a unit on another team
                            if (curHoveredTile.AttemptUnitAttack(curUnit))
                            {
                                //Attack Success
                                curUnit.canAttack = false;
                                curUnit.isAttacking = false;
                                ShowActionsMenu();
                                if (curUnit.canMove)
                                {
                                    //Select Move Action
                                    curUnit.MoveUnit(this);
                                }
                                else
                                {
                                    //Unit out of actions
                                    curUnit.UnhighlightTiles();
                                    curState = GameState.PlayerSelectAction;
                                    ShowMessage("Unit out of Moves!", 1f);
                                }
                            }
                            else
                            {
                                //Attack Failed
                            }
                        }
                        else
                        {
                            //Clicking on a unit not on a team
                        }
                    }
                }
                break;

            case GameState.EnemyTurn:
                debugStateText.text = "GameState: EnemyTurn";
                break;

            default:
                Debug.LogError("Game is in an invalid state!");
                debugStateText.text = "GameState: ERROR";
                return;
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
        Debug.Log(GameController.instance.GetInstanceID());
        if (IntVector2.Distance(startLoc, curLoc) > maxDist || !IntVector2.OnGrid(curLoc, GameController.instance.gridSizeX, GameController.instance.gridSizeY))
        {
            return;
        }

        
        else if (!availableTiles.Contains(GameController.instance.mapGrid[curLoc.x, curLoc.y]))
        {

            availableTiles.Add(GameController.instance.mapGrid[curLoc.x, curLoc.y]);
            FindNextTile(startLoc, curLoc + IntVector2.coordUp, maxDist);
            FindNextTile(startLoc, curLoc + IntVector2.coordRight, maxDist);
            FindNextTile(startLoc, curLoc + IntVector2.coordDown, maxDist);
            FindNextTile(startLoc, curLoc + IntVector2.coordLeft, maxDist);
        }
    }

    /// <summary>
    /// Called by the OnClick() Event attached to the Move Button on the Action Canvas
    /// </summary>
    public void MoveButtonSelect()
    {
        curUnit.MoveUnit(this);
    }

    /// <summary>
    /// Called by the OnClick() event attached to the Attack Button on the Action Canvas
    /// </summary>
    public void AttackButtonSelect()
    {
        curUnit.AttackUnit(this);
    }

    /// <summary>
    /// Called by the OnClick() event attached to the Cancel Button on the Action Canvas
    /// </summary>
    public void CancelButtonSelect()
    {
        curUnit.OnUnitDeselect(this);
        HideActionsMenu();
        HideUnitStats();
    }

    /// <summary>
    /// Called by the OnClick() event attached to the End Turn Button on the General Action Canvas
    /// </summary>
    public void EndTurnButtonSelect()
    {
        if (curUnit != null)
        {
            curUnit.OnUnitDeselect(this);
        }

        foreach (UnitController p1unit in GameController.instance.unitsInGame[0])
        {
            p1unit.canMove = true;
            p1unit.canAttack = true;
        }
    }

    /// <summary>
    /// Displays the statistics for the selected unit
    /// </summary>
    /// <param name="selectedUnit">Unit that you want to show stats for</param>
    public void ShowUnitStats(UnitController selectedUnit)
    {
        statCanvas.SetActive(true);
        numUnitsText.text = "Units: " + selectedUnit.numUnits;
        attackText.text = "Attack: " + selectedUnit.attackStat;
        defenseText.text = "Defense: " + selectedUnit.defenseStat;
        movementText.text = "Movement: " + selectedUnit.movementRange;
        rangeText.text = "Range: " + selectedUnit.AttackRange;
    }

    /// <summary>
    /// Hides the statistics UI elements
    /// </summary>
    public void HideUnitStats()
    {
        statCanvas.SetActive(false);
    }

    /// <summary>
    /// Displays the Actions menu
    /// </summary>
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

    public void PointerEnterButton()
    {
        cursor.transform.position = farAway;
        prevHoveredTile = curHoveredTile;
        curHoveredTile = null;
    }

    public void PointerExitButton()
    {
        curHoveredTile = prevHoveredTile;
        cursor.transform.position = curHoveredTile.transform.position + new Vector3(0f, 0.00001f, 0f);
    }

    public void ShowMessage(string newMessage, float messageTime)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        messageCoro = StartCoroutine(ShowMessageCoro(newMessage, messageTime));
    }

    IEnumerator ShowMessageCoro(string newMessage, float messageDuration)
    {
        //Debug.Log("ShowMessage()");

        float startTime;
        float endTime;
        float fadeInDuration = 0.1f;
        float fadeOutDuration = 0.25f;

        messageCanvas.SetActive(true);
        messageText.text = newMessage;

        startTime = Time.time;
        endTime = startTime + fadeInDuration;

        while (Time.time <= endTime)
        {
            float colorAlpha = (Time.time - startTime) / fadeOutDuration;
            ChangeMessageTextAlpha(colorAlpha);
            yield return null;
        }
        ChangeMessageTextAlpha(1);

        yield return new WaitForSeconds(messageDuration);

        //Debug.Log("Begin Fade");
        startTime = Time.time;
        endTime = startTime + fadeOutDuration;
        while (Time.time <= endTime)
        {
            float colorAlpha = (endTime - Time.time) / fadeOutDuration;
            ChangeMessageTextAlpha(colorAlpha);
            yield return null;
        }
        HideMessage();
    }

    void ChangeMessageTextAlpha(float newAlphaValue)
    {
        messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, newAlphaValue);
    }

    public void HideMessage()
    {
        Debug.Log("HideMessage()");
        if (messageCoro != null)
        {
            StopCoroutine(messageCoro);
        }
        //messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1);
        messageCanvas.SetActive(false);
    }

    /*
    public override void OnStartClient()
    {
        base.OnStartClient();
        //Debug.Log(debugPlayerID.text);
        playerTeamID = 0;// Convert.ToInt32(debugPlayerID.text);
    }
    */
//}
