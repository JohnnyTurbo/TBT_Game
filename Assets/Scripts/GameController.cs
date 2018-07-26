using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState { PlayerSelectTile, PlayerSelectAction, PlayerMoveUnit, PlayerAttackUnit, EnemyTurn, GameOver };

public class GameController : MonoBehaviour {

    public static GameController instance;

    public int gridSizeX, gridSizeY;
    public GameObject whiteTilePrefab, blackTilePrefab;
    public GameObject basicLandUnitPrefab;
    public GameObject damageAmtTextPrefab;
    public GameObject WorldSpaceCanvas;
	public GameObject loadingScreen;
    public GameObject[] unitTypes;
    public int numPlayerUnits, numEnemyUnits;
    public int tileSize;
    public const int numTeams = 2;
    public int playerTeamID;
	public int nextPlayerID;
    public int[] numPlayersOnBench;
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
    public bool isOnMobile = false;

    public static readonly Vector3 farAway = new Vector3(1000f, 1000f, 1000f);

    int indexOfLastCommand;
    int unitIndex = 0;
    int camPositionIndex = 1;
    bool isCameraMoving = false;
    bool bothTeamsSpawned = false;
    bool waitingForTurn = false;
    string teamStr, gameID;

    string thisTurnCMDs = "";
    GameObject actionCanvas, statCanvas, messageCanvas, generalActionCanvas, debugCanvas;
    Text numUnitsText, attackText, defenseText, movementText, rangeText, unitTypeText, messageText, debugStateText, debugGameID;
    Text p1Label, p2Label;
    Button moveActionButton, attackActionButton, endTurnButton, backButton;
    TileController prevHoveredTile = null;
    List<TileController> availableTiles;
    Coroutine messageCoro;

    void Awake()
    {

        #if UNITY_ANDROID
            isOnMobile = true;
            //Debug.Log("Is On Android");
        #endif
        
        #if UNITY_IOS
            isOnMobile = true;
            //Debug.Log("Is On IOs");
        #endif
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
		backButton = generalActionCanvas.transform.Find ("BackButton").GetComponent<Button> ();
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

		backButton.onClick.AddListener (LoadMainMenu);

        //InitializeMap(0 == GlobalData.instance.playerID);
        //SpawnUnits();

        numPlayersOnBench = new int[numTeams];

        if (!isOnMobile)
        {
            MoveCameraLeftButtonSelect(3.5f);
        }
        else
        {
            Firebase.Messaging.FirebaseMessaging.MessageReceived += FCMReceived;
            Camera.main.transform.position = new Vector3(7.25f, 18, 7);
            Camera.main.transform.rotation = Quaternion.Euler(90, -90, 0);
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 8.5f;
            generalActionCanvas.transform.Find("CamMoveIcon").gameObject.SetActive(false);
        }
        StartCoroutine(SetupGame());
		loadingScreen.SetActive (false);
    }

    IEnumerator SetupGame()
    {
        string[] gameData;

        if (GlobalData.instance.GetCurGameData(out gameData))
        {
            Debug.Log("Game has data: " + GlobalData.instance.CurGameDataToString());
            playerTeamID = (gameData[0] == "0") ? 0 : 1;
            nextPlayerID = (gameData[0] == "0") ? 1 : 0;
            if(playerTeamID == 0)
            {
                p1Label.gameObject.SetActive(true);
            }
            else if(playerTeamID == 1)
            {
                p2Label.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Error: Unknown Team ID");
            }
            gameID = gameData[1];
            int.TryParse(gameData[2], out indexOfLastCommand);
            //Debug.Log("indexOfLastCommand = " + indexOfLastCommand);
            teamStr = gameData[3];
            Debug.Log("GameUnitsInfo: " + teamStr);
            string gameBoardSize = "grd|" + gameData[4];
            //Debug.Log("gameBoardSize: " + gameBoardSize);

            //ProcessCommand(gameBoardSize.Split('|'));
            StartCoroutine(ProcessCommand(gameBoardSize.Split('|')));

            string[] teamUnits = teamStr.Split('&');
            int index = 0;
            foreach (string teamString in teamUnits)
            {
                //ProcessCommand(teamString.Split('!'));
                StartCoroutine(ProcessCommand(teamString.Split('!')));
                ++index;
            }

            if (index == 2)
            {
                bothTeamsSpawned = true;
                Debug.Log("Both Teams Have Spawned");
            }
            else if (index == 1)
            {
                Debug.Log("Only one team spawned");
            }
            else
            {
                Debug.LogError("Error: Tried to spawn " + index + " teams.");
            }

            int whoseTurn = int.Parse(gameData[5]);

            if(whoseTurn == playerTeamID)
            {
                //This Player's Turn
                Debug.Log("It's your turn");
                StartCoroutine(GetCommands());
                curState = GameState.PlayerSelectTile;
            }
            else
            {
                //Opposing player's turn
                Debug.Log("It's your opponent's turn");
                curState = GameState.EnemyTurn;
                StartCoroutine(WaitForMyTurn());
            }

            GlobalData.instance.ClearLoadGameDataHelper();
        }
        else
        {
            Debug.LogError("Game has no data!");
        }
        yield return null;
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

                                //thisTurnCMDs += ("&atk|" + indexOfOther + "|" + newHealthOfOther);
                                thisTurnCMDs += ("&atk|" + indexOfOther + "|" + curUnit.unitIndex);

                                indexOfLastCommand++;

                                curUnit.canAttack = false;
                                curUnit.isAttacking = false;

                                //Debug.Log("Num of all units: " + unitsInGame[numTeams].Count);

                                //if (newHealthOfOther <= 0 && unitsInGame[nextPlayerID].Count <= 0)
                                if(numPlayersOnBench[nextPlayerID] >= 3)
                                {
                                    //Debug.Log(unitsInGame[nextPlayerID].Count);
                                    curUnit.OnUnitDeselect();
                                    ShowMessage("YOU WON!", 500);
                                    thisTurnCMDs += ("&end");
                                    indexOfLastCommand++;
                                    Debug.Log("thisTurnCMDs: " + thisTurnCMDs);
                                    curState = GameState.GameOver;
                                    //NetworkController.instance.SendData(thisTurnCMDs, nextPlayerID);
                                    StartCoroutine(NetworkController.instance.SendData(thisTurnCMDs, nextPlayerID, gameID, UpdateUnitLocations(), indexOfLastCommand.ToString()));
                                    StartCoroutine(NetworkController.instance.EndGame(gameID));
                                    //StartCoroutine(WaitForMyTurn());
                                    endTurnButton.gameObject.SetActive(false);
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadMainMenu();
        }
    }

    void LoadMainMenu()
    {
        GlobalData.instance.returningToLoginScreen = true;
        SceneManager.LoadScene("PublicAlphaMainMenu");
    }

    IEnumerator GetCommands()
    {
        Debug.Log("GetCommands()");
        CoroutineWithData cd = new CoroutineWithData(this, NetworkController.instance.ReceiveData(gameID));
        yield return cd.coroutine;
        string strToParse;
        string[] subStrings;

        strToParse = (string)cd.result;

        Debug.Log(strToParse);

        subStrings = strToParse.Split('&');

        Debug.Log("index of last CMD: " + indexOfLastCommand + " sub length: " + subStrings.Length);

        while(indexOfLastCommand < subStrings.Length)
        {
            
            string[] cmdToProc = subStrings[indexOfLastCommand].Split('|');

            //ProcessCommand(cmdToProc);
            StartCoroutine(ProcessCommand(cmdToProc));
            while (!bothTeamsSpawned)
            {
                yield return null;
                Debug.Log("both ev spnd");
            }
            indexOfLastCommand++;
        }
    }

    //void ProcessCommand(string[] cmdToProc)
    IEnumerator ProcessCommand(string[] cmdToProc)
    {
        string[] cmdParts;
        int unitID;

        switch (cmdToProc[0])
        {
            //Message
            case "msg":
                string messageFromGame = cmdToProc[1];
                Debug.Log("Processing Message: " + messageFromGame);
                if (indexOfLastCommand == 0 && playerTeamID == 0 && !bothTeamsSpawned)
                {
                    Coroutine spnOther = StartCoroutine(NetworkController.instance.SpawnOtherTeam(gameID));
                    yield return spnOther;
                    Coroutine spnCmd = StartCoroutine(ProcessCommand(GlobalData.instance.otherTeam.Split('!')));
                    yield return spnCmd;
                    bothTeamsSpawned = true;
                    Debug.Log("Other Team Spawned");
                }
                break;

            //Grid Size
            case "grd":
                //Debug.Log("Setting grid size");
                cmdParts = cmdToProc[1].Split('X');
                gridSizeX = int.Parse(cmdParts[0]);
                gridSizeY = int.Parse(cmdParts[1]);
                InitializeMap();
                break;

            case "spn":
                int teamID = int.Parse(cmdToProc[1]);
                cmdParts = cmdToProc[2].Split('^');
                foreach (string unitTypeStr in cmdParts)
                {
                    //Debug.Log(unitTypeStr);
                    string[] unitSubStr = unitTypeStr.Split('*');
                    int unitType = int.Parse(unitSubStr[0]);
                    int unitHealth = int.Parse(unitSubStr[3]);
                    IntVector2 spawnLoc = new IntVector2(unitSubStr[1], unitSubStr[2]);
                    SpawnUnit(unitType, teamID, spawnLoc, unitHealth);
                }
                //StartCoroutine(UpdateUnitLocations());
                break;

            //Movement
            case "mvt":
                //cmdParts = cmdToProc[1]
                unitID = int.Parse(cmdToProc[1]);
                cmdParts = cmdToProc[2].Split(',');
                int newXLoc = int.Parse(cmdParts[0]);
                int newYLoc = int.Parse(cmdParts[1]);
                Debug.Log("mvt, numTeams: " + numTeams + ", unitID: " + unitID);
                UnitController unitToMove = unitsInGame[numTeams][unitID];
                mapGrid[newXLoc, newYLoc].AttemptUnitMove(unitToMove);
                break;

            //Attack
            case "atk":
                unitID = int.Parse(cmdToProc[1]);
                //int newHealth = int.Parse(cmdToProc[2]);
                int newHealth;
                int attackingUnitID = int.Parse(cmdToProc[2]);
                UnitController affectedUnit = unitsInGame[numTeams][unitID];
                UnitController attackingUnit = unitsInGame[numTeams][attackingUnitID];
                TileController affectedUnitTile = mapGrid[affectedUnit.curCoords.x, affectedUnit.curCoords.y];
                affectedUnitTile.AttemptUnitAttack(attackingUnit, out newHealth);
                Handheld.Vibrate();
                
                break;

            //Game Over
            case "end":
                ShowMessage("YOU LOST!", 500);
                endTurnButton.gameObject.SetActive(false);
                break;

            default:
                Debug.LogError("Error: Unrecognized Command: " + cmdToProc[0]);
                break;
        }
        yield return null;
    }

    void InitializeMap()
    {
        IntVector2 gridSize = new IntVector2();

        gridSize.x = gridSizeX;
        gridSize.y = gridSizeY;

        int numTiles = 0;

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
                numTiles++;
            }
        }

        //Debug.Log(numTiles + " tiles on map.");
    }

    void SpawnUnit(int unitType, int teamID, IntVector2 location, int health)
    {

        GameObject newUnit = Instantiate(unitTypes[unitType], Vector3.zero, Quaternion.identity);
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
        newUnitController.numUnitType = unitType;
        if (health == 0)
        {
            newUnitController.GoToBench();
            newUnitController.unitHealth = 0;
        }
        else
        {
            int xLoc, yLoc;

            if (location == IntVector2.coordDownLeft)
            {
                xLoc = (unitIndex % 3) + 2;
                yLoc = (gridSizeY - 1) * teamID;
            }
            else
            {
                xLoc = location.x;
                yLoc = location.y;
            }
            Vector3 newUnitLoc = new Vector3((xLoc * tileSize), newUnitController.yOffset, (yLoc * tileSize));
            newUnit.transform.position = newUnitLoc;
            newUnitController.curCoords = new IntVector2(xLoc, yLoc);
            mapGrid[xLoc, yLoc].isOccupied = true;
            mapGrid[xLoc, yLoc].unitOnTile = newUnit;
            if (health != -1)
            {
                newUnitController.unitHealth = health;
            }
        }
        if (!isOnMobile)
        {
            newUnitController.RotateUnitSprite();
        }
        else
        {
            newUnit.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
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

        //UpdateUnitLocations();

        StartCoroutine(EndTurn());
    }

    IEnumerator EndTurn()
    {
        Coroutine sendCMD = StartCoroutine(NetworkController.instance.SendData(thisTurnCMDs, nextPlayerID, gameID,
                                           UpdateUnitLocations(), indexOfLastCommand.ToString()));
        thisTurnCMDs = "";

        yield return sendCMD;

        StartCoroutine(WaitForMyTurn());
    }

    string UpdateUnitLocations()
    {
        string teamInfo = "";

        for (int i = 0; i < numTeams; i++)
        {
            teamInfo += "&spn!" + i + "!";

            foreach (UnitController unit in unitsInGame[i])
            {
                teamInfo += unit.numUnitType + "*" + unit.curCoords.ToStarString() + "*" + unit.unitHealth + "^";
            }
            teamInfo = teamInfo.Remove(teamInfo.Length - 1);    //Removes the ^ at the end of the string
        }

        teamInfo = teamInfo.Remove(0, 1);    //Removes the & at the beginning of the string

        Debug.Log("teamInfo: " + teamInfo);

        return teamInfo;
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
        if (isOnMobile)
        {
            return;
        }
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
        Debug.Log("WaitForMyTurn()");

        debugGameID.text = "W8";

        waitingForTurn = true;

        int nextID = nextPlayerID;

        //Debug.Log("MyID: " + playerTeamID + ", nextPlayerID: " + nextPlayerID);

        //NetworkController.instance.SendStringToDB(thisTurnCMDs, nextPlayerID);

        while (nextID != playerTeamID) {
            if (Application.isEditor)
            {
                CoroutineWithData cd = new CoroutineWithData(this, NetworkController.instance.RecieveTurn(gameID));
                yield return cd.coroutine;

                //Debug.Log(cd.result);

                //string helperStr = (string)cd.result;
                nextID = int.Parse((string)cd.result);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else
            {
                while (waitingForTurn)
                {
                    yield return null;
                }
                CoroutineWithData cd = new CoroutineWithData(this, NetworkController.instance.RecieveTurn(gameID));
                yield return cd.coroutine;

                //Debug.Log(cd.result);

                //string helperStr = (string)cd.result;
                nextID = int.Parse((string)cd.result);
            }
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

    public void FCMReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        debugGameID.text = "NEW MESG!";
        string msgGameID;
        if(e.Message.Data.TryGetValue("gameID", out msgGameID))
        {
            debugGameID.text = "no w8 - " + msgGameID;
            waitingForTurn = false;
        }
    }

    public void OnButtonBack()
    {

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

    public IntVector2(string newX, string newY)
    {
        x = int.Parse(newX);
        y = int.Parse(newY);
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

    public string ToStarString()
    {
        return (x + "*" + y);
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