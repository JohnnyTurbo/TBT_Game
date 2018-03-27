using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState { PlayerSelectTile, PlayerSelectAction, PlayerMoveUnit, PlayerAttackUnit, EnemyTurn, GameOver };

public class GameController : MonoBehaviour {

    public static GameController instance;

    public int gridSizeX, gridSizeY;
    public GameObject whiteTilePrefab, blackTilePrefab;
    public GameObject basicLandUnitPrefab;
    public GameObject damageAmtTextPrefab;
    public GameObject WorldSpaceCanvas;
    public GameObject[] unitTypes;
    public int numPlayerUnits, numEnemyUnits;
    public int tileSize;
    public const int numTeams = 2;
    public int playerTeamID;
    public int nextPlayerID;
    public TileController[,] mapGrid;
    public UnitController curUnit = null;
    public TileController curHoveredTile = null;
    public TileController curSelectedTile = null;
    public GameObject uiObject;
    public GameState curState;
    public List<UnitController>[] unitsInGame;
    public GameObject cursor;
    public Material[] playerColors;
    public Vector3[] camPositions;
    public Vector3[] camRotations;

    public static readonly Vector3 farAway = new Vector3(1000f, 1000f, 1000f);

    int indexOfLastCommand = 0;
    int unitIndex = 0;
    int camPositionIndex = 1;
    bool isCameraMoving = false;
    string thisTurnCMDs = "";
    GameObject actionCanvas, statCanvas, messageCanvas, generalActionCanvas, debugCanvas;
    Text numUnitsText, attackText, defenseText, movementText, rangeText, unitTypeText, messageText, debugStateText, debugGameID;
    Text p1Label, p2Label;
    Button moveConfirmButton, moveActionButton, attackActionButton, endTurnButton;
    TileController prevHoveredTile = null;
    List<TileController> availableTiles;
    Coroutine messageCoro;

    void Awake()
    {

        unitsInGame = new List<UnitController>[numTeams + 1];

        for(int i = 0; i < numTeams + 1; i++)
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
        messageCanvas = uiObject.transform.Find("MessageCanvas").gameObject;
        generalActionCanvas = uiObject.transform.Find("GeneralActionCanvas").gameObject;
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
        unitTypeText = statCanvas.transform.Find("UnitTypeText").GetComponent<Text>();

        //Find the UI components on the messageCanvas
        messageText = messageCanvas.transform.Find("MessageText").GetComponent<Text>();

        //Find the UI components on the generalActionCanvas
        endTurnButton = generalActionCanvas.transform.Find("EndTurnButton").GetComponent<Button>();
        p1Label = generalActionCanvas.transform.Find("Player1Label").GetComponent<Text>();
        p2Label = generalActionCanvas.transform.Find("Player2Label").GetComponent<Text>();

        //Find the UI componenets on the debugStateCanvas
        debugStateText = debugCanvas.transform.Find("DebugStateText").GetComponent<Text>();
        debugGameID = debugCanvas.transform.Find("DebugGameID").GetComponent<Text>();

        actionCanvas.SetActive(false);
        statCanvas.SetActive(false);
        messageCanvas.SetActive(false);
        p1Label.gameObject.SetActive(false);
        p2Label.gameObject.SetActive(false);

        //InitializeMap(0 == GlobalData.instance.playerID);
        //SpawnUnits();

        if(GlobalData.instance.playerID == 0)
        {
            p1Label.gameObject.SetActive(true);
            MoveCameraLeftButtonSelect(3.5f);
            playerTeamID = 0;
            nextPlayerID = 1;
            indexOfLastCommand+=2;
            InitializeMap(true);
            curState = GameState.EnemyTurn;
            StartCoroutine(WaitForMyTurn());
        }
        else
        {
            p2Label.gameObject.SetActive(true);
            MoveCameraRightButtonSelect(3.5f);
            playerTeamID = 1;
            nextPlayerID = 0;
            StartCoroutine(GetCommands());
            curState = GameState.PlayerSelectTile;
        }

        debugGameID.text = "GameID: " + GlobalData.instance.gameID;

        cursor = Instantiate(cursor, farAway, Quaternion.identity);
        
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
                        //Debug.Log("curState is PlayerMoveUnit, Fire1 down, curHoveredTile is null");
                        //curUnit.OnUnitDeselect();
                    }
                    else if(curHoveredTile.unitOnTile != null)
                    {
                        //Debug.Log("curState is PlayerMoveUnit, Fire1 down, curHoveredTile.unitOntile is not null");

                        //curUnit.OnUnitDeselect();
                        //curHoveredTile.unitOnTile.GetComponent<UnitController>().OnUnitSelect();
                        curHoveredTile.OnTileSelect();
                    }
                    else if (curHoveredTile.AttemptUnitMove(curUnit))
                    {
                        //Debug.Log("curState is PlayerMoveUnit, Fire1 down, curHoveredTile is attemptUnitMove(curUnit) is true");

                        //curSelectedTile.unitOnTile = null;
                        //Debug.Log("curSelected Tile is at: " + curSelectedTile.curCoords.ToString());
                        //curHoveredTile.unitOnTile = curUnit.gameObject;
                        //curUnit.curCoords = curHoveredTile.curCoords;
                        curUnit.isMoving = false;
                        curUnit.canMove = false;

                        //Send Data to server
                        //NetworkController.instance.SendStringToDB("&mvt|" + curUnit.unitIndex + "|" + curUnit.curCoords.ToLightString());
                        
                        thisTurnCMDs += ("&mvt|" + curUnit.unitIndex + "|" + curUnit.curCoords.ToLightString());

                        indexOfLastCommand++;

                        ShowActionsMenu();
                        if (curUnit.canAttack)
                        {
                            //Make Unit Attack
                            curUnit.AttackUnit();
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
                    if(curHoveredTile == null)
                    {
                        //Debug.Log("curState is PlayerAttackUnit, Fire1 down, curHoveredTile is null");
                    }
                    else if(curHoveredTile.unitOnTile != null)
                    {
                        if(curHoveredTile.unitOnTile.GetComponent<UnitController>().unitTeamID == playerTeamID)
                        {
                            //Clicking on a unit on the same team
                            curHoveredTile.OnTileSelect();
                        }
                        else if(curHoveredTile.unitOnTile.GetComponent<UnitController>().unitTeamID != playerTeamID)
                        {
                            //Clicking on a unit on another team
                            int newHealthOfOther;
                            int indexOfOther = curHoveredTile.unitOnTile.GetComponent<UnitController>().unitIndex;
                            if (curHoveredTile.AttemptUnitAttack(curUnit, out newHealthOfOther))
                            {
                                //Attack Success
                                //NetworkController.instance.SendStringToDB("&atk|" + indexOfOther + "|" + newHealthOfOther);

                                HideUnitStats();

                                thisTurnCMDs += ("&atk|" + indexOfOther + "|" + newHealthOfOther);

                                indexOfLastCommand++;

                                curUnit.canAttack = false;
                                curUnit.isAttacking = false;

                                //Debug.Log("Num of all units: " + unitsInGame[numTeams].Count);

                                if (newHealthOfOther <= 0 && unitsInGame[nextPlayerID].Count <= 0)
                                {
                                    //Debug.Log(unitsInGame[nextPlayerID].Count);
                                    curUnit.OnUnitDeselect();
                                    ShowMessage("YOU WON!", 500);
                                    thisTurnCMDs += ("&end");
                                    indexOfLastCommand++;
                                    Debug.Log("thisTurnCMDs: " + thisTurnCMDs);
                                    curState = GameState.GameOver;
                                    //NetworkController.instance.SendData(thisTurnCMDs, nextPlayerID);
                                    StartCoroutine(WaitForMyTurn());
                                    break;
                                }

                                ShowActionsMenu();
                                if (curUnit.canMove)
                                {
                                    //Select Move Action
                                    curUnit.MoveUnit();
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
                if (endTurnButton.IsInteractable())
                {
                    endTurnButton.interactable = false;
                }
                break;

            case GameState.GameOver:
                debugStateText.text = "GameState: GameOver";
                if (endTurnButton.IsInteractable())
                {
                    endTurnButton.interactable = false;
                }
                break;

            default:
                Debug.LogError("Game is in an invalid state!");
                debugStateText.text = "GameState: ERROR";
                return;
        }
    }

    IEnumerator JoinGame()
    {
        CoroutineWithData cd = new CoroutineWithData(this, NetworkController.instance.ReceiveData());
        yield return cd.coroutine;
        string strToParse;
        string[] subStrings;

        strToParse = (string)cd.result;

        Debug.Log(strToParse);

        subStrings = strToParse.Split('&');
        strToParse = subStrings[1];

        Debug.Log(strToParse);

        subStrings = strToParse.Split(',');

        gridSizeX = int.Parse(subStrings[0]);

        gridSizeY = int.Parse(subStrings[1]);

        InitializeMap(false);
    }

    IEnumerator GetCommands()
    {
        CoroutineWithData cd = new CoroutineWithData(this, NetworkController.instance.ReceiveData());
        yield return cd.coroutine;
        string strToParse;
        string[] subStrings;

        strToParse = (string)cd.result;

        Debug.Log(strToParse);

        subStrings = strToParse.Split('&');

        while(indexOfLastCommand < subStrings.Length)
        {
            string[] cmdToProc = subStrings[indexOfLastCommand].Split('|');
            string[] cmdParts;
            int unitID;

            switch (cmdToProc[0])
            {
                //Message
                case "msg":
                    //Debug.Log("Game Start");
                    break;

                //Grid Size
                case "grd":
                    //Debug.Log("Setting grid size");
                    cmdParts = cmdToProc[1].Split(',');
                    gridSizeX = int.Parse(cmdParts[0]);
                    gridSizeY = int.Parse(cmdParts[1]);
                    InitializeMap(false);
                    break;

                case "spn":
                    int teamID = int.Parse(cmdToProc[1]);
                    cmdParts = cmdToProc[2].Split(',');
                    foreach (string unitTypeStr in cmdParts)
                    {
                        int unitType = int.Parse(unitTypeStr);
                        SpawnUnit(unitType, teamID);
                    }
                    break;

                //Movement
                case "mvt":
                    //cmdParts = cmdToProc[1]
                    unitID = int.Parse(cmdToProc[1]);
                    cmdParts = cmdToProc[2].Split(',');
                    int newXLoc = int.Parse(cmdParts[0]);
                    int newYLoc = int.Parse(cmdParts[1]);
                    UnitController unitToMove = unitsInGame[numTeams][unitID];
                    mapGrid[newXLoc, newYLoc].AttemptUnitMove(unitToMove);
                    break;

                //Attack
                case "atk":
                    unitID = int.Parse(cmdToProc[1]);
                    int newHealth = int.Parse(cmdToProc[2]);
                    UnitController affectedUnit = unitsInGame[numTeams][unitID];

                    Debug.Log("atk, numTeams: " + numTeams + ", unitID: " + unitID);

                    if(newHealth <= 0)
                    {
                        //Debug.Log("New Health is less than or equal to zero!");
                        Destroy(affectedUnit.gameObject);
                    }
                    else
                    {
                        affectedUnit.unitHealth = newHealth;
                    }
                    break;

                //Game Over
                case "end":
                    ShowMessage("YOU LOST!", 500);
                    break;

                default:
                    Debug.LogError("Error: Unrecognized Command: " + cmdToProc[0]);
                    break;
            }

            indexOfLastCommand++;
        }
    }

    void InitializeMap(bool isCreator)
    {
        //tileList = new List<GameObject>();
        IntVector2 gridSize = new IntVector2();

        if (isCreator)
        {
            //Debug.Log("isCreator == true");
            gridSize.x = gridSizeX;
            gridSize.y = gridSizeY;

            NetworkController.instance.SendStringToDB("&grd|" + gridSize.x + "," + gridSize.y, 0);
        }
        else
        {
            //Debug.Log("isCreator == false");
            //receive grid size from server
            gridSize.x = gridSizeX;
            gridSize.y = gridSizeY;
        }

        mapGrid = new TileController[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
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

        //SpawnUnits();
        StartCoroutine(SendUnitsToServer());
    }

    IEnumerator SendUnitsToServer()
    {
        //Debug.Log("Sending Units to Server");
        string strToSend = "&spn|" + GlobalData.instance.playerID + "|" + GlobalData.instance.teamStr;
        Coroutine sendCMD = StartCoroutine(NetworkController.instance.SendData(strToSend, 1));
        yield return sendCMD;
        //Debug.Log("units sent to server: " + strToSend);
        Coroutine getCmd = StartCoroutine(GetCommands());
        yield return getCmd;
    }

    /*
    void SpawnUnits()
    {
        //Debug.Log("Spawning Units");
        int unitIndex = 0;

        for (int i = 0; i < numPlayerUnits; i++)
        {
            Vector3 newUnitLoc = new Vector3(i * tileSize, 0, 0);
            GameObject newUnit = Instantiate(basicLandUnitPrefab, newUnitLoc, Quaternion.identity);

            UnitController newUnitController = newUnit.GetComponent<UnitController>();
            newUnitController.unitTeamID = 0;
            newUnitController.unitIndex = unitIndex;
            newUnitController.curCoords = new IntVector2(i, 0);
            mapGrid[i, 0].isOccupied = true;
            mapGrid[i, 0].unitOnTile = newUnit;
            unitsInGame[0].Add(newUnitController);
            unitsInGame[numTeams].Add(newUnitController);
            unitIndex++;
        }

        for (int i = 0; i < numEnemyUnits; i++)
        {
            Vector3 newUnitLoc = new Vector3(i * tileSize, 0, (gridSizeY - 1) * 2);
            GameObject newUnit = Instantiate(basicLandUnitPrefab, newUnitLoc, Quaternion.identity);
            UnitController newUnitController = newUnit.GetComponent<UnitController>();
            newUnitController.unitTeamID = 1;
            newUnitController.unitIndex = unitIndex;
            newUnitController.curCoords = new IntVector2(i, gridSizeY - 1);
            mapGrid[i, gridSizeY - 1].isOccupied = true;
            mapGrid[i, gridSizeY - 1].unitOnTile = newUnit;
            unitsInGame[1].Add(newUnitController);
            unitsInGame[numTeams].Add(newUnitController);
            unitIndex++;
        }
    }
    */

    void SpawnUnit(int unitType, int teamID /*, IntVector2 location*/)
    {
        int xLoc = (unitIndex % 3) + 2;
        int yLoc = (gridSizeY - 1) * teamID;

        Vector3 newUnitLoc = new Vector3((xLoc * tileSize), 0, (yLoc * tileSize));

        GameObject newUnit = Instantiate(unitTypes[unitType], newUnitLoc, Quaternion.identity);
        string unitSpriteToLoad = "";
        switch (unitType)
        {
            //Point Guard
            case 0:
                unitSpriteToLoad = "PointGuard";
                break;
            
            //Lineman
            case 1:
                unitSpriteToLoad = "Lineman";
                break;
            
            //Pitcher
            case 2:
                unitSpriteToLoad = "Pitcher";
                break;
            
            //MMAMiddleweight
            case 3:
                unitSpriteToLoad = "MMAMiddleweight";
                break;
            
            //Unknown
            default:
                Debug.LogError("Unknown unitType");
                break;
        }

        unitSpriteToLoad += (teamID == 0) ? "Blue" : "Yellow";

        //Debug.Log("unitSpriteToLoad: " + unitSpriteToLoad);

        SpriteRenderer unitSprite = newUnit.transform.Find("Sprite").GetComponent<SpriteRenderer>();
        unitSprite.sprite = Resources.Load("PlayerSprites/" + unitSpriteToLoad, typeof (Sprite)) as Sprite;
        UnitController newUnitController = newUnit.GetComponent<UnitController>();
        newUnitController.unitTeamID = teamID;
        newUnitController.unitIndex = unitIndex;
        newUnitController.RotateUnitSprite();
        newUnitController.curCoords = new IntVector2(xLoc, yLoc);
        mapGrid[xLoc, yLoc].isOccupied = true;
        mapGrid[xLoc, yLoc].unitOnTile = newUnit;
        //newUnit.GetComponent<MeshRenderer>().material = playerColors[teamID];
        unitsInGame[teamID].Add(newUnitController);
        unitsInGame[numTeams].Add(newUnitController);
        unitIndex++;
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

    /// <summary>
    /// Called by the OnClick() Event attached to the Move Button on the Action Canvas
    /// </summary>
    public void MoveButtonSelect()
    {
        curUnit.MoveUnit();
    }

    /// <summary>
    /// Called by the OnClick() event attached to the Attack Button on the Action Canvas
    /// </summary>
    public void AttackButtonSelect()
    {
        curUnit.AttackUnit();
    }

    /// <summary>
    /// Called by the OnClick() event attached to the Cancel Button on the Action Canvas
    /// </summary>
    public void CancelButtonSelect()
    {
        curUnit.OnUnitDeselect();
        HideActionsMenu();
        HideUnitStats();
    }

    /// <summary>
    /// Called by the OnClick() event attached to the End Turn Button on the General Action Canvas
    /// </summary>
    public void EndTurnButtonSelect()
    {
        if(curUnit != null)
        {
            curUnit.OnUnitDeselect();
        }

        foreach(UnitController playerUnit in unitsInGame[playerTeamID])
        {
            playerUnit.canMove = true;
            playerUnit.canAttack = true;
        }
        curState = GameState.EnemyTurn;
        
        StartCoroutine(WaitForMyTurn());
    }

    public void MoveCameraLeftButtonSelect(float duration)
    {
        if(camPositionIndex <= 0 || isCameraMoving)
        {
            return;
        }

        camPositionIndex--;
        isCameraMoving = true;
        StartCoroutine(MoveCamera(duration));

    }

    public void MoveCameraRightButtonSelect(float duration)
    {
        if (camPositionIndex >= 2 || isCameraMoving)
        {
            return;
        }

        camPositionIndex++;
        isCameraMoving = true;
        StartCoroutine(MoveCamera(duration));
    }

    IEnumerator MoveCamera(float duration)
    {

        Vector3 camStartPos, camEndPos, newPos;
        Quaternion camStartRot, camEndRot, newRot;
        float startTime, endTime, percentComplete;

        startTime = Time.time;
        endTime = startTime + duration;
        percentComplete = (Time.time - startTime) / duration;

        camStartPos = Camera.main.transform.position;
        camStartRot = Camera.main.transform.rotation;
        camEndPos = camPositions[camPositionIndex];
        camEndRot = Quaternion.Euler(camRotations[camPositionIndex]);

        while(percentComplete < 1f) {

            percentComplete = (Time.time - startTime) / duration;

            float t = percentComplete * percentComplete * (3f - 2f * percentComplete);

            newPos = Vector3.Lerp(camStartPos, camEndPos, t);
            newRot = Quaternion.Slerp(camStartRot, camEndRot, t);

            Camera.main.transform.position = newPos;
            Camera.main.transform.rotation = newRot;
            RotateAllUnits();
            percentComplete = (Time.time - startTime) / duration;

            yield return null;
        }
        isCameraMoving = false;
    }

    void RotateAllUnits()
    {
        foreach(UnitController curUC in unitsInGame[numTeams])
        {
            if (curUC != null)
            {
                curUC.RotateUnitSprite();
            }
        }
    }

    IEnumerator WaitForMyTurn()
    {
        int nextID = nextPlayerID;

        //Debug.Log("MyID: " + playerTeamID + ", nextPlayerID: " + nextPlayerID);

        //NetworkController.instance.SendStringToDB(thisTurnCMDs, nextPlayerID);
        Coroutine sendCMD = StartCoroutine(NetworkController.instance.SendData(thisTurnCMDs, nextPlayerID));
        thisTurnCMDs = "";

        yield return sendCMD;

        while (nextID != playerTeamID) {

            CoroutineWithData cd = new CoroutineWithData(this, NetworkController.instance.RecieveTurn());
            yield return cd.coroutine;

            //string helperStr = (string)cd.result;
            nextID = int.Parse((string)cd.result);
            yield return new WaitForSecondsRealtime(0.5f);
        }

        Debug.Log("Before GetCommands() lastCMDindex is: " + indexOfLastCommand);

        Coroutine coro = StartCoroutine(GetCommands());

        yield return coro;

        //Debug.Log("Your turn!");
        
        curState = GameState.PlayerSelectTile;
        endTurnButton.interactable = true;
    }

    /// <summary>
    /// Displays the statistics for the selected unit
    /// </summary>
    /// <param name="selectedUnit">Unit that you want to show stats for</param>
    public void ShowUnitStats(UnitController selectedUnit)
    {
        statCanvas.SetActive(true);
        numUnitsText.text = "Health: " + selectedUnit.unitHealth;
        attackText.text = "Attack: " + selectedUnit.attackStat;
        defenseText.text = "Defense: " + selectedUnit.defenseStat;
        movementText.text = "Movement: " + selectedUnit.movementRange;
        rangeText.text = "Range: " + selectedUnit.AttackRange;
        unitTypeText.text = "Type: " + selectedUnit.unitType;
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
        while(Time.time <= endTime)
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
        //Debug.Log("HideMessage()");
        if (messageCoro != null)
        {
            StopCoroutine(messageCoro);
        }
        //messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1);
        messageCanvas.SetActive(false);
    }
}

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

    public string ToLightString()
    {
        return (x + "," + y);
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