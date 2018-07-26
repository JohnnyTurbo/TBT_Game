using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuController : MonoBehaviour {

	//OSX edit
    //W10 edit

    private static MainMenuController _instance;

    public static MainMenuController instance;
    
    public GameObject mainMenuCanvas, loginCanvas, createUserCanvas, userHomepageCanvas, createGameCanvas, draftTeamCanvas,
                      loadingScreenCanvas, mainOptionsCanvas, requestedGamesCanvas, errorCanvas, feedbackCanvas;
    public GameObject activeGameButtonPrefab, pendingGameButtonPrefab, pastGameButtonPrefab;
    public Sprite[] athletes;
    public Image[] team;
    public Button confirmButton, backButton;

    readonly string serverAddress = "http://homecookedgames.com/sbphp/scripts/";
    readonly string dbUsername = "johnnytu_testusr", dbPassword = "OAnF8TqR12PJ";
    readonly int maxNumPlayersOnTeam = 3;

    int numPlayersOnTeam = 0;
    string teamStr = "", gameIdToJoin = "", otherUsername = "", wasGameRequested = "";
    Sprite SelectionCircleStartSpr;
    InputField usernameIF, pinIF, newUsernameIF, newPinIF, newPinConfIF, newEmailIF, gameIDIF, otherUsernameIF, feedbackIF;
    Text errorText, serverErrorText;
    GameObject loadingScreen, usersGamesContainer, requestedGamesContainer;
    MainMenuScreen currentScreen;
    MainMenuScreen mainOptionsScreen, loginScreen, createUserScreen, userHomepageScreen, requestedGamesScreen, 
                   createGameScreen, draftTeamScreen, feedbackScreen;
    Button tryAgainButton, playButton, tutorialButton;
    Toggle staySignedInToggle;

    VideoPlayer introVP;

    delegate void tryAgainFunction();
    tryAgainFunction tryAgainButtonHandler;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            _instance = this;
        }

        instance = _instance;

        Screen.fullScreen = false;
    }

    void Start()
    {

        //Android Messing around

        AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject curActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject curIntent = curActivity.Call<AndroidJavaObject>("getIntent");
        string intString = curIntent.Call<string>("getDataString");

        Debug.Log("curActivity<" + curActivity.Call<string>("getLocalClassName") + ">");
        Debug.Log("curIntent<" + intString + ">");
        
        //End android messing around



        //PlayerPrefs.DeleteAll();

        //Find needed objects

        errorText = errorCanvas.transform.Find("ErrorText").GetComponent<Text>();

        playButton = mainOptionsCanvas.transform.Find("PlayButton").GetComponent<Button>();
        tutorialButton = mainOptionsCanvas.transform.Find("TutorialButton").GetComponent<Button>();

        usernameIF = loginCanvas.transform.Find("UsernameIF").GetComponent<InputField>();
        pinIF = loginCanvas.transform.Find("PinIF").GetComponent<InputField>();
        staySignedInToggle = loginCanvas.transform.Find("StaySignedInToggle").GetComponent<Toggle>();

        newUsernameIF = createUserCanvas.transform.Find("NewUsernameIF").GetComponent<InputField>();
        newPinIF = createUserCanvas.transform.Find("NewPinIF").GetComponent<InputField>();
        newPinConfIF = createUserCanvas.transform.Find("NewPinConfIF").GetComponent<InputField>();
        newEmailIF = createUserCanvas.transform.Find("NewEmailIF").GetComponent<InputField>();

        gameIDIF = userHomepageCanvas.transform.Find("GameIDIF").GetComponent<InputField>();
        usersGamesContainer = userHomepageCanvas.transform.Find("ScrollView").Find("UsersGamesContainer").gameObject;

        otherUsernameIF = createGameCanvas.transform.Find("OtherUsernameIF").GetComponent<InputField>();

        loadingScreen = loadingScreenCanvas.transform.Find("LoadingScreen").gameObject;
        serverErrorText = loadingScreen.transform.Find("ServerErrorText").GetComponent<Text>();
        tryAgainButton = loadingScreen.transform.Find("TryAgainButton").GetComponent<Button>();

        requestedGamesContainer = requestedGamesCanvas.transform.Find("ScrollView").Find("RequestedGamesContainer").gameObject;

        feedbackIF = feedbackCanvas.transform.Find("FeedbackIF").GetComponent<InputField>();

        introVP = this.GetComponent<VideoPlayer>();

        //Set defaults on objects

        errorText.text = "";
        serverErrorText.text = "";



        errorText.text = "curIntent<" + intString + ">";




        //TODO Check if they have played the tutorial before
        playButton.interactable = GlobalData.instance.hasPlayedTutorial;

        //TODO Check if they have watched the intro video before
        tutorialButton.interactable = GlobalData.instance.hasSeenIntroVid;

        confirmButton.interactable = false;
        SelectionCircleStartSpr = team[0].sprite;

		backButton.onClick.AddListener (GoBack);

        mainOptionsScreen = new MainMenuScreen(mainOptionsCanvas, null);
        loginScreen = new MainMenuScreen(loginCanvas, mainOptionsScreen);
        createUserScreen = new MainMenuScreen(createUserCanvas, loginScreen);
        userHomepageScreen = new MainMenuScreen(userHomepageCanvas, mainOptionsScreen);
        requestedGamesScreen = new MainMenuScreen(requestedGamesCanvas, userHomepageScreen);
        createGameScreen = new MainMenuScreen(createGameCanvas, userHomepageScreen);
        draftTeamScreen = new MainMenuScreen(draftTeamCanvas, userHomepageScreen);
        feedbackScreen = new MainMenuScreen(feedbackCanvas, userHomepageScreen);

        //deactivate unneeded componenets.

        loginCanvas.SetActive(false);
        createUserCanvas.SetActive(false);
        userHomepageCanvas.SetActive(false);
        createGameCanvas.SetActive(false);
        draftTeamCanvas.SetActive(false);
        requestedGamesCanvas.SetActive(false);
        feedbackCanvas.SetActive(false);
        tryAgainButton.gameObject.SetActive(false);
        loadingScreen.SetActive(false);

        currentScreen = mainOptionsScreen;

        if(PlayerPrefs.GetInt("stayLoggedIn") == 1 || GlobalData.instance.returningToLoginScreen) {
            Debug.Log("AttemptingLogin" + PlayerPrefs.GetInt("stayLoggedIn"));
            GlobalData.instance.returningToLoginScreen = false;
            mainOptionsCanvas.SetActive(false);
            Login(PlayerPrefs.GetString("lastLoggedInUser"), PlayerPrefs.GetString("lastUserPIN"));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoBack();
        }
    }

	void GoBack(){
		currentScreen = currentScreen.GoBack ();
	}

    public void OnButtonPlay()
    {
        Debug.Log("OnButtonPlay()");
        mainOptionsCanvas.SetActive(false);
        if (PlayerPrefs.GetInt("stayLoggedIn") == 1)
        {
            string curUsername = PlayerPrefs.GetString("lastLoggedInUser");
            string curPIN = PlayerPrefs.GetString("lastUserPIN");
            Login(curUsername, curPIN);
        }
        else
        {
            loginCanvas.SetActive(true);
            currentScreen = loginScreen;
        }
    }

    void Login(string acctUsername, string acctPassword)
    {
        Debug.Log("Login\nU:" + acctUsername + "\nP:" + acctPassword);
        loadingScreen.SetActive(true);
        //Call php login script
        StartCoroutine(NetworkController.AccountLogin(acctUsername, acctPassword));
    }

    public void OnButtonTutorial()
    {
        Debug.Log("OnButtonTutorial()");
        GlobalData.instance.hasPlayedTutorial = true;
        playButton.interactable = true;
        loadingScreen.SetActive(true);
        SceneManager.LoadScene("PublicAlphaTutorial");
    }

    public void OnButtonIntroVideo()
    {
        Debug.Log("OnButtonIntroVideo");
        //Handheld.PlayFullScreenMovie("Videos/JT_SoupsOn2017.mov", Color.black, FullScreenMovieControlMode.Hidden, 
        //                             FullScreenMovieScalingMode.AspectFit);
        loadingScreen.SetActive(true);
        //initialOptions.SetActive(false);
        introVP.Play();
        introVP.loopPointReached += LoopPointReached;
    }

    private void LoopPointReached(VideoPlayer source)
    {
        //throw new System.NotImplementedException();
        Debug.Log("LoopPointReached()");
        introVP.Stop();
        loadingScreen.SetActive(false);
        GlobalData.instance.hasSeenIntroVid = true;
        tutorialButton.interactable = true;
    }

    public void OnButtonLogin()
    {
        //Debug.Log("OnButtonLogin()");

        errorText.text = "";

        if (usernameIF.text == "" && pinIF.text == "")
        {
            errorText.text = "Please enter your Username and PIN.";
        }
        else if (usernameIF.text == "")
        {
            errorText.text = "Please enter your Username.";
        }
        else if(pinIF.text == "")
        {
            errorText.text = "Please enter your PIN";
        }
        else
        {
            Debug.Log("Username and password are both entered");
            errorText.text = "";

            Login(usernameIF.text, pinIF.text);
        }
    }

    public void AccountLoginCallback(string messageFromServer)
    {
       
        if (messageFromServer.StartsWith("Error:"))
        {
            loadingScreen.SetActive(false);
            messageFromServer = messageFromServer.Remove(0, 7);
            if (messageFromServer == "Incorrect Username or PIN")
            {
                errorText.text = "Incorrect Username or PIN";
            }
            else
            {
                errorText.text = "Login Error, Please Try Again Later";
            }
        }
        else if (messageFromServer.StartsWith("Login success!"))
        {
            if (loginCanvas.activeSelf)
            {
                if (staySignedInToggle.isOn)
                {
                    Debug.Log("keeping prefs");
                    PlayerPrefs.SetInt("stayLoggedIn", 1);
                }
                else
                {
                    PlayerPrefs.SetInt("stayLoggedIn", 0);
                }
                PlayerPrefs.SetString("lastLoggedInUser", usernameIF.text);
                PlayerPrefs.SetString("lastUserPIN", pinIF.text);

                usernameIF.text = "";
                pinIF.text = "";
                loginCanvas.SetActive(false);
            }
 
            string playerID = messageFromServer.Remove(0, 14);
            Debug.Log("Login Success and mfs = " + playerID);
            GlobalData.instance.playerID = playerID;

            userHomepageCanvas.SetActive(true);
            currentScreen = userHomepageScreen;
            StartCoroutine(PopulateUserHomepage());
        }
        else
        {
            Debug.LogError("Unknown message from server: " + messageFromServer);
        }
    }

    public void OnButtonSignUp()
    {
        //Debug.Log("OnButtonSignUp()");

        errorText.text = "";

        loginCanvas.SetActive(false);
        createUserCanvas.SetActive(true);
        currentScreen = createUserScreen;
    }

    public void OnButtonCreateAccount()
    {
        //Debug.Log("OnButtonCreateAccount()");

        errorText.text = "";

        if (newUsernameIF.text == "" || newPinIF.text == "" || newPinConfIF.text == "" || newEmailIF.text == "")
        {
            errorText.text = "Please fill out all fields!";
        }
        else if(newPinIF.text.Length < 3 || newPinIF.text.Length > 8)
        {
            errorText.text = "PIN must be between 3 and 8 digits long!";
        }
        else if(newPinIF.text != newPinConfIF.text)
        {
            errorText.text = "PINs do not match!";
        }
        else if (!ValidateEmailAddress(newEmailIF.text))
        {
            errorText.text = "Please enter a valid email address!";
        }
        else
        {
            Debug.Log("All fields Valid");
            errorText.text = "";

            //Call php create account script
            StartCoroutine(NetworkController.CreateUserAccount(newUsernameIF.text, newPinIF.text, newEmailIF.text));
        }
    }

    public void CreateAccountCallback(string messageFromServer)
    {
        if (messageFromServer.StartsWith("Error:"))
        {
            messageFromServer = messageFromServer.Remove(0, 7);
            string[] splitMessage = messageFromServer.Split('\'');
            Debug.LogError("Create account error: " + messageFromServer);

            /*
            foreach(string st in splitMessage)
            {
                Debug.Log(st);
            }
            */

            if (splitMessage[0] == "Duplicate entry ")
            {
                errorText.text = splitMessage[3] + ": '" + splitMessage[1] + "' is already in use! Try again.";
            }
        }
        else if(messageFromServer == "New account successfully created")
        {
            Debug.Log("Account created successfully");
            newUsernameIF.text = "";
            newPinIF.text = "";
            newPinConfIF.text = "";
            newEmailIF.text = "";
            createUserCanvas.SetActive(false);
            loginCanvas.SetActive(true);
            currentScreen = loginScreen;
        }
        else
        {
            Debug.LogError("Unknown message from server: " + messageFromServer);
        }
    }

    bool ValidateEmailAddress(string addressToCheck)
    {
        if (addressToCheck.Contains("@"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator PopulateRequestsPage()
    {
        float startTime = Time.time;

        WWWForm getActiveRequests = new WWWForm();

        getActiveRequests.AddField("username", dbUsername);
        getActiveRequests.AddField("password", dbPassword);
        getActiveRequests.AddField("playerID", GlobalData.instance.playerID);

        WWW fetchGameRequests = new WWW(serverAddress + "fetchGameRequests.php", getActiveRequests);

        yield return fetchGameRequests;

        Debug.Log("returned from requesting");

        if(fetchGameRequests.error == null)
        {
            
            if(fetchGameRequests.text == "")
            {
                loadingScreen.SetActive(false);
                yield break;
            }
            string fullRequestString = fetchGameRequests.text.Remove(fetchGameRequests.text.Length - 1);
            Debug.Log("Requested Games: " + fullRequestString);
            string[] requestedGames = fullRequestString.Split('|');
            for(int i = 0; i < requestedGames.Length; i++)
            {
                string[] gameParts = requestedGames[i].Split('+');

                GameObject gameButton = pendingGameButtonPrefab;
                string buttonText = "Game Request From: " + gameParts[1];

                Vector3 newButtonPos = new Vector3(0, (i * -175), 0f);
                GameObject newGameButtonGO = GameObject.Instantiate(gameButton, requestedGamesContainer.transform);

                Button newGameButton = newGameButtonGO.GetComponent<Button>();
                Text newButtonText = newGameButtonGO.transform.Find("Text").GetComponent<Text>();
                newGameButtonGO.transform.localPosition = newButtonPos;

                newButtonText.text = buttonText;
                newGameButton.onClick.AddListener(() => AcceptGame(gameParts[0], "Yes"));
            }
        }
        else
        {
            Debug.LogError("Error while fetching game requests: " + fetchGameRequests.error);
        }
    }

    /// <summary>
    /// String returned from server: 
    /// 
    /// gameID,gamePlayerID,movesBehind,version,status,whoseTurn,gameBoardInfo,gameUnitsInfo,lastCMDIndex
    /// 0      1            2           3       4      5         6             7             8
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator PopulateUserHomepage()
    {
        float startTime = Time.time;

        WWWForm getActiveGames = new WWWForm();

        getActiveGames.AddField("username", dbUsername);
        getActiveGames.AddField("password", dbPassword);
        getActiveGames.AddField("playerID", GlobalData.instance.playerID);

        WWW fetchUserGames = new WWW(serverAddress + "fetchUserGames.php", getActiveGames);

        yield return fetchUserGames;

        if(fetchUserGames.error == null)
        {

            if (fetchUserGames.text == "")
            {
                Debug.Log("User Has No Games");
                NetworkController.instance.ServerCallTime(startTime, "PopulateUserHomepage");
                yield break;
            }

            Debug.Log("Users games: " + fetchUserGames.text);

            string[] userGamesInfo = fetchUserGames.text.Split('|');

            float height = userGamesInfo.Length * 175;
            RectTransform ugcRect = usersGamesContainer.GetComponent<RectTransform>();
            ugcRect.sizeDelta = new Vector2(1300, height);

            for (int i = userGamesInfo.Length - 1, j = 0; i >= 0 ; i--, j++)
            {
                string[] gameStringComponents = userGamesInfo[i].Split(',');
                string gameID = gameStringComponents[0];
                string gamePlayerID = gameStringComponents[1];
                string movesBehind = gameStringComponents[2];
                string gameVersion = gameStringComponents[3];
                string gameStatus = gameStringComponents[4];
                string whoseTurn = gameStringComponents[5];
                
                string gameBoardInfo = gameStringComponents[6];
                string gameUnitsInfo = gameStringComponents[7];
                string lastCmdIndex = gameStringComponents[8];
                
                string otherUsername = gameStringComponents[9];

                /*
                Debug.Log("Player with global ID: " + GlobalData.instance.playerID + " is Player: " + gamePlayerID +
                          " in game with ID: " + gameStringComponents[0] + ". And they are " + gameStringComponents[2] +
                          " moves behind. The game's version is: " + gameStringComponents[3] + ", the status is: " +
                          gameStringComponents[4] + ", and it is player " + whoseTurn + "'s turn.");
                */

                GameObject gameButton;
                string buttonText;

                switch (gameStatus)
                {
                    case "0":
                        gameButton = pendingGameButtonPrefab;
                        buttonText = (otherUsername == "NONE") ? "Searching for Opponent" : "Pending Game With: " + otherUsername;
                        break;

                    case "1":
                        gameButton = activeGameButtonPrefab;
                        string turnText = (gamePlayerID == whoseTurn) ? "Your Turn" : "Their Turn";
                        buttonText = "Match with: " + otherUsername + " - " + turnText;
                        break;

                    case "2":
                        gameButton = pastGameButtonPrefab;
                        buttonText = "Game Over - You ";
                        buttonText += (whoseTurn == gamePlayerID) ? "Lost to: " : "Beat: ";
                        buttonText += otherUsername;
                        break;

                    default:
                        Debug.LogError("Error: cannot load gameID: " + gameStringComponents[i] + " because it is in unkown state: " +
                                        gameStatus);
                        gameButton = null;
                        buttonText = "";
                        break;
                }
                
                Vector3 newButtonPos = new Vector3(0, (j * -175), 0f);
                //Debug.Log("New Button Pos: " + newButtonPos.ToString());
                GameObject newGameButtonGO = GameObject.Instantiate(gameButton, usersGamesContainer.transform);
                
                Button newGameButton = newGameButtonGO.GetComponent<Button>();
                Text newButtonText = newGameButtonGO.transform.Find("Text").GetComponent<Text>();
                newGameButtonGO.transform.localPosition = newButtonPos;

                newButtonText.text = buttonText;
                switch (gameStatus)
                {
                    case "0":
                        newGameButton.onClick.AddListener(() => AcceptGame(gameID, ""));
                        if(gamePlayerID == "0")
                        {
                            newGameButton.interactable = false;
                        }
                        break;

                    case "1":
                        newGameButton.onClick.AddListener(() => LoadGame(gameStringComponents));
                        break;

                    case "2":
                        //Show Game Stats
                        newGameButton.interactable = false;
                        break;

                    default:

                        break;
                }                 
            }
        }
        else
        {
            Debug.LogError("Could not fetch user's games.\n" + fetchUserGames.error);
            errorText.text = "Could not fetch your games, try again later.";
        }
        loadingScreen.SetActive(false);
        NetworkController.instance.ServerCallTime(startTime, "PopulateUserHomepage");
    }

    void LoadGame(string[] gameAttributes)
    {
        string gameID = gameAttributes[0];
        string gamePlayerID = gameAttributes[1];
        string movesBehind = gameAttributes[2];
        string gameVersion = gameAttributes[3];
        string gameStatus = gameAttributes[4];
        string whoseTurn = gameAttributes[5];
        string gameBoardInfo = gameAttributes[6];
        string gameUnitsInfo = gameAttributes[7];
        string lastCmdIndex = gameAttributes[8];

        Debug.Log("LoadGame(string " + gameID + ")");

        GlobalData.instance.SetupLoadGameDataHelper(gamePlayerID, gameID, lastCmdIndex, gameUnitsInfo, gameBoardInfo, whoseTurn);

        loadingScreen.SetActive(true);

        SceneManager.LoadScene("PublicAlphaField");
    }

    public void OnButtonNewGame()
    {
        Debug.Log("OnButtonNewGame()");

        userHomepageCanvas.SetActive(false);
        createGameCanvas.SetActive(true);
        otherUsernameIF.text = "";
        currentScreen = createGameScreen;
    }

    public void OnButtonCreateGame()
    {
        Debug.Log("OnButtonCreateGame()");

        tryAgainButtonHandler = OnButtonCreateGame;

        otherUsername = otherUsernameIF.text;

        createGameCanvas.SetActive(false);
        ShowDraftScreen();
        currentScreen = draftTeamScreen;
        //StartCoroutine(CreateGame());
    }

    public void OnButtonJoinGame()
    {
        Debug.Log("OnButtonJoinGame()");

        tryAgainButtonHandler = OnButtonJoinGame;

        string gameID = gameIDIF.text;

        if (gameID == "")
        {
            errorText.text = "Error: no game ID entered!";
            return;
        }
        else
        {
            errorText.text = "";
            gameIdToJoin = gameID;
            userHomepageCanvas.SetActive(false);
            ShowDraftScreen();
            currentScreen = draftTeamScreen;
        }
    }

    void AcceptGame(string gameID, string gameRequested)
    {
        gameIdToJoin = gameID;
        wasGameRequested = gameRequested;
        userHomepageCanvas.SetActive(false);
        requestedGamesCanvas.SetActive(false);
        ShowDraftScreen();
        currentScreen = draftTeamScreen;
    }

    void ShowDraftScreen()
    {
        draftTeamCanvas.SetActive(true);
    }

    public void OnButtonTryAgain()
    {
        Debug.Log("OnButtonTryAgain()");

        tryAgainButton.gameObject.SetActive(false);
        serverErrorText.text = "";
        tryAgainButtonHandler();
    }

    IEnumerator JoinGame()
    {
        string boardSize = "7X8";

        //Debug.Log("JoinGame()");

        //GlobalData.instance.inGamePlayerID = 0;

        string teamInfo = "spn!0!";

        string[] teamUnits = teamStr.Split(',');

        foreach (string str in teamUnits)
        {
            teamInfo += str + "*" + IntVector2.coordDownLeft.ToStarString() + "*-1" + "^";
        }

        teamInfo = teamInfo.Remove(teamInfo.Length - 1);    //Removes the ^ at the end of the string

        Debug.Log("JoinGame() gameUnitsInfo: " + teamInfo);

        WWWForm dbCredentials = new WWWForm();
        dbCredentials.AddField("username", dbUsername);
        dbCredentials.AddField("password", dbPassword);
        dbCredentials.AddField("playerID", GlobalData.instance.playerID);
        dbCredentials.AddField("boardSize", boardSize);
        dbCredentials.AddField("otherUsername", otherUsername);
        dbCredentials.AddField("teamInfo", teamInfo);
        dbCredentials.AddField("gameVersion", Application.version);
        //Debug.Log("PID = " + GlobalData.instance.playerID);

        WWW newGameRequest = new WWW(serverAddress + "createNewGame.php", dbCredentials);
        yield return newGameRequest;

        if (newGameRequest.error == null)
        {
            /*
            Debug.Log("New game created!");
            int convertedInt;
            int.TryParse(newGameRequest.text, out convertedInt);

            GlobalData.instance.currentGameID = convertedInt;

            Debug.Log("GameID is: " + convertedInt.ToString());
            Debug.Log("GameID is: " + newGameRequest.text);
            */
            Debug.Log("GameID is: " + newGameRequest.text);
            GlobalData.instance.SetupLoadGameDataHelper("0", newGameRequest.text, "0", teamInfo, boardSize, "1");
            //Coroutine sendCMD = StartCoroutine(NetworkController.instance.SendData("&grd|" + 7 + "," + 8, 0, newGameRequest.text, teamInfo));
            //yield return sendCMD;
            SceneManager.LoadScene("PublicAlphaField");
        }
        else
        {
            Debug.LogError("Error: could not create new game.\n" + newGameRequest.error);
            serverErrorText.text = "Error: " + newGameRequest.error;
            tryAgainButton.gameObject.SetActive(true);
        }
    }

    IEnumerator JoinGame(string gameID, string requestedGame)
    {
        Debug.Log("JoinGame(" + gameID + ")");

        //GlobalData.instance.inGamePlayerID = 1;
        /*
        int convertedInt;
        int.TryParse(gameID, out convertedInt);
        GlobalData.instance.currentGameID = convertedInt;
        */

        string teamInfo = "&spn!1!";

        string[] teamUnits = teamStr.Split(',');

        foreach (string str in teamUnits)
        {
            teamInfo += str + "*" + IntVector2.coordDownLeft.ToStarString() + "*-1" + "^";
        }

        teamInfo = teamInfo.Remove(teamInfo.Length - 1);    //Removes the ^ at the end of the string

        WWWForm gameJoinID = new WWWForm();
        gameJoinID.AddField("gID", gameID);
        gameJoinID.AddField("username", dbUsername);
        gameJoinID.AddField("password", dbPassword);
        gameJoinID.AddField("playerID", GlobalData.instance.playerID);
        gameJoinID.AddField("teamInfo", teamInfo);
        gameJoinID.AddField("requested", requestedGame);
        WWW attemptGameJoin = new WWW(serverAddress + "joinGame.php", gameJoinID);

        yield return attemptGameJoin;

        if (attemptGameJoin.error == null)
        {
            Debug.Log("Joining game with ID: " + gameID);
            Debug.Log("From Server: " + attemptGameJoin.text);
            string gameBoardSize = attemptGameJoin.text.Split('@')[1];
            string allTeamInfo = attemptGameJoin.text.Split('@')[3];
            GlobalData.instance.SetupLoadGameDataHelper("1", gameID, "0", allTeamInfo, gameBoardSize, "1");
            SceneManager.LoadScene("PublicAlphaField");
        }
        else
        {
            Debug.LogError("Error: could not join game with ID: " + gameID + "\nError from PHP script: " + attemptGameJoin.error);
            serverErrorText.text = "Error: " + attemptGameJoin.error;
            tryAgainButton.gameObject.SetActive(true);
        }   
    }

    IEnumerator SubmitFeedback()
    {
        Debug.Log("SendingFeedback");

        WWWForm feedbackForm = new WWWForm();
        feedbackForm.AddField("username", dbUsername);
        feedbackForm.AddField("password", dbPassword);
        feedbackForm.AddField("playerID", GlobalData.instance.playerID);
        feedbackForm.AddField("feedback", feedbackIF.text);

        WWW sendFeedback = new WWW(serverAddress + "submitFeedback.php", feedbackForm);

        yield return sendFeedback;

        loadingScreen.SetActive(false);

        if(sendFeedback.error == null)
        {
            Debug.Log("Feedback Sent!\nFrom server: " + sendFeedback.text);
            GoBack();
        }
        else
        {
            Debug.LogError("Error: could not send feedback!\n" + sendFeedback.error);
        }
    }

    public void OnPlayerSelect(int playerID)
    {

        if(numPlayersOnTeam >= maxNumPlayersOnTeam)
        {
            return;
        }

        Sprite imgToUse = athletes[playerID];

        team[numPlayersOnTeam].sprite = imgToUse;
        numPlayersOnTeam++;

        teamStr += playerID;

        if(numPlayersOnTeam == maxNumPlayersOnTeam)
        {
            confirmButton.interactable = true;
        }
        else
        {
            teamStr += ",";
        }
    }

    public void OnConfirmButtonSelect()
    {
        //GlobalData.instance.teamStr = teamStr;
        currentScreen = null;
        draftTeamCanvas.SetActive(false);
        loadingScreen.SetActive(true);
        if (gameIdToJoin == "")
        {
            StartCoroutine(JoinGame());
        }
        else
        {
            StartCoroutine(JoinGame(gameIdToJoin, wasGameRequested));
        }
    }

    public void OnClearButtonSelect()
    {
        ClearTeamSelection();
    }

    void ClearTeamSelection()
    {
        foreach (Image i in team)
        {
            i.sprite = SelectionCircleStartSpr;
        }
        confirmButton.interactable = false;
        teamStr = "";
        numPlayersOnTeam = 0;
    }

    public void SetErrorText(string newErrorText)
    {
        errorText.text = newErrorText;
    }

    public void OnButtonNotify()
    {
        StartCoroutine(Notify());
    }

    public void OnButtonClearData()
    {
        PlayerPrefs.DeleteAll();
    }

    public void OnButtonGetGameRequests()
    {
        userHomepageCanvas.SetActive(false);
        requestedGamesCanvas.SetActive(true);
        loadingScreen.SetActive(true);
        currentScreen = requestedGamesScreen;
        StartCoroutine(PopulateRequestsPage());
    }

    public void OnButtonBackFromRequests()
    {
        foreach(Transform child in requestedGamesContainer.GetComponentInChildren<Transform>())
        {
            Destroy(child.gameObject);
        }
        requestedGamesCanvas.SetActive(false);
        userHomepageCanvas.SetActive(true);
    }

    public void OnButtonLogout()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("PublicAlphaMainMenu");
    }

    public void OnButtonFeedback()
    {
        userHomepageCanvas.SetActive(false);
        feedbackCanvas.SetActive(true);
        currentScreen = feedbackScreen;
        feedbackIF.text = "";
    }

    public void OnButtonSubmitFeedback()
    {
        loadingScreen.SetActive(true);
        StartCoroutine(SubmitFeedback());
    }

    IEnumerator Notify()
    {
        WWW notificationTest = new WWW(serverAddress + "testPushNotification.php");

        yield return notificationTest;

        if(notificationTest.error == null)
        {
            Debug.Log("PING!");
            Debug.Log("Note From serv: " + notificationTest.text);
        }
        else
        {
            Debug.LogError("Error: " + notificationTest.error);
        }
    }
}

[System.Serializable]
public class MainMenuScreen
{
    GameObject m_screen;
    MainMenuScreen m_previousScreen;

    public MainMenuScreen GoBack()
    {
        if (m_previousScreen == null)
        {
            return this;
        }

        m_screen.SetActive(false);
        m_previousScreen.m_screen.SetActive(true);
        return m_previousScreen;
    }

    public MainMenuScreen(GameObject screen, MainMenuScreen previousScreen)
    {
        m_screen = screen;
        m_previousScreen = previousScreen;
    }
}