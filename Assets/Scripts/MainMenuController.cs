using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuController : MonoBehaviour {

    private static MainMenuController _instance;

    public static MainMenuController instance;
    
    public GameObject mainMenuCanvas, loginCanvas, createUserCanvas, userHomepageCanvas, createGameCanvas, draftTeamCanvas,
                      loadingScreenCanvas, mainOptionsCanvas;
    public GameObject activeGameButtonPrefab, pendingGameButtonPrefab, pastGameButtonPrefab;
    public Sprite[] athletes;
    public Image[] team;
    public Button confirmButton;

    readonly string serverAddress = "http://homecookedgames.com/sbphp/scripts/";
    readonly string dbUsername = "johnnytu_testusr", dbPassword = "OAnF8TqR12PJ";
    readonly int maxNumPlayersOnTeam = 3;

    int numPlayersOnTeam = 0;
    string teamStr = "", gameIdToJoin = "";
    Sprite SelectionCircleStartSpr;
    InputField usernameIF, pinIF, newUsernameIF, newPinIF, newPinConfIF, newEmailIF, gameIDIF;
    Text errorText, serverErrorText;
    GameObject loadingScreen, usersGamesContainer;
    Button tryAgainButton, playButton, tutorialButton;
    
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
    }

    void Start()
    {
        //Find needed objects

        errorText = mainMenuCanvas.transform.Find("ErrorText").GetComponent<Text>();

        playButton = mainOptionsCanvas.transform.Find("PlayButton").GetComponent<Button>();
        tutorialButton = mainOptionsCanvas.transform.Find("TutorialButton").GetComponent<Button>();

        usernameIF = loginCanvas.transform.Find("UsernameIF").GetComponent<InputField>();
        pinIF = loginCanvas.transform.Find("PinIF").GetComponent<InputField>();

        newUsernameIF = createUserCanvas.transform.Find("NewUsernameIF").GetComponent<InputField>();
        newPinIF = createUserCanvas.transform.Find("NewPinIF").GetComponent<InputField>();
        newPinConfIF = createUserCanvas.transform.Find("NewPinConfIF").GetComponent<InputField>();
        newEmailIF = createUserCanvas.transform.Find("NewEmailIF").GetComponent<InputField>();

        gameIDIF = userHomepageCanvas.transform.Find("GameIDIF").GetComponent<InputField>();
        usersGamesContainer = userHomepageCanvas.transform.Find("UsersGamesContainer").gameObject;

        loadingScreen = loadingScreenCanvas.transform.Find("LoadingScreen").gameObject;
        serverErrorText = loadingScreen.transform.Find("ServerErrorText").GetComponent<Text>();
        tryAgainButton = loadingScreen.transform.Find("TryAgainButton").GetComponent<Button>();

        introVP = this.GetComponent<VideoPlayer>();

        //Set defaults on objects

        errorText.text = "";
        serverErrorText.text = "";

        //TODO Check if they have played the tutorial before
        playButton.interactable = GlobalData.instance.hasPlayedTutorial;

        //TODO Check if they have watched the intro video before
        tutorialButton.interactable = GlobalData.instance.hasSeenIntroVid;

        confirmButton.interactable = false;
        SelectionCircleStartSpr = team[0].sprite;

        //deactivate unneeded componenets.

        loginCanvas.SetActive(false);
        createUserCanvas.SetActive(false);
        userHomepageCanvas.SetActive(false);
        createGameCanvas.SetActive(false);
        draftTeamCanvas.SetActive(false);

        tryAgainButton.gameObject.SetActive(false);
        loadingScreen.SetActive(false);

        //mainMenuCanvas.SetActive(false);


        //DELETE AFTER FILMING TUTORIAL 4
        //GlobalData.instance.teamStr = "1,0,3";
    }

    public void OnButtonPlay()
    {
        Debug.Log("OnButtonPlay()");
        mainOptionsCanvas.SetActive(false);
        loginCanvas.SetActive(true);
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

            //Call php login script
            StartCoroutine(NetworkController.AccountLogin(usernameIF.text, pinIF.text));
        }
    }

    public void AccountLoginCallback(string messageFromServer)
    {
        if (messageFromServer.StartsWith("Error:"))
        {
            messageFromServer = messageFromServer.Remove(0, 7);
            if (messageFromServer == "Incorrect Username or PIN")
            {
                errorText.text = "Incorrect Username or PIN";
            }
        }
        else if (messageFromServer.StartsWith("Login success!"))
        {
            usernameIF.text = "";
            pinIF.text = "";
            string playerID = messageFromServer.Remove(0, 14);
            Debug.Log("Login Success and mfs = " + playerID);
            GlobalData.instance.playerID = playerID;
            loginCanvas.SetActive(false);
            userHomepageCanvas.SetActive(true);
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

    /// <summary>
    /// String returned from server uses syntxax: gameID,gamePlayerID,movesBehind,version,status,whoseTurn|gameID...
    /// String index reference                    0      1            2           3       4      5        |0
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

            //Debug.Log("Users games: " + fetchUserGames.text);

            string[] userGamesInfo = fetchUserGames.text.Split('|');

            for (int i = 0; i < userGamesInfo.Length; i++)
            {
                string[] gameStringComponents = userGamesInfo[i].Split(',');
                string gamePlayerID = (gameStringComponents[1] == "0") ? "1" : "2";
                string whoseTurn = (gameStringComponents[5] == "0") ? "1" : "2";
                /*
                Debug.Log("Player with global ID: " + GlobalData.instance.playerID + " is Player: " + gamePlayerID +
                          " in game with ID: " + gameStringComponents[0] + ". And they are " + gameStringComponents[2] +
                          " moves behind. The game's version is: " + gameStringComponents[3] + ", the status is: " +
                          gameStringComponents[4] + ", and it is player " + whoseTurn + "'s turn.");
                */
                GameObject gameButton;

                switch (gameStringComponents[4])
                {
                    case "0":
                        gameButton = pendingGameButtonPrefab;
                        break;

                    case "1":
                        gameButton = activeGameButtonPrefab;
                        break;

                    case "2":
                        gameButton = pastGameButtonPrefab;
                        break;

                    default:
                        Debug.LogError("Error: cannot load gameID: " + gameStringComponents[i] + " because it is in unkown state: " +
                                        gameStringComponents[4]);
                        gameButton = null;
                        break;
                }
                Vector3 newButtonPos = new Vector3(0, (i * -175f) - 255f, 0f);

                GameObject newGameButtonGO = GameObject.Instantiate(gameButton, usersGamesContainer.transform);
                Button newGameButton = newGameButtonGO.GetComponent<Button>();
                Text newButtonText = newGameButtonGO.transform.Find("Text").GetComponent<Text>();
                newGameButtonGO.transform.localPosition = newButtonPos;

                string turnText = (gameStringComponents[1] == gameStringComponents[5]) ? "Your Turn" : "Opponent's Turn";
                newButtonText.text = "Game ID: " + gameStringComponents[0] + " - " + turnText;
                newGameButton.onClick.AddListener(() => LoadGame(gameStringComponents[0]));                
            }

            

            /*
            string strToParse = fetchUserGames.text;
            string[] usersGames = strToParse.Split('|');

            for (int i = 0; i < usersGames.Length - 1; i++)
            {
                string[] gameStringComponents = usersGames[i].Split(',');
                string gamePlayerID = (gameStringComponents[1] == "0") ? "1" : "2";

                WWWForm getGameInfo = new WWWForm();

                getGameInfo.AddField("username", dbUsername);
                getGameInfo.AddField("password", dbPassword);
                getGameInfo.AddField("gameID", gameStringComponents[0]);

                WWW fetchGameInfo = new WWW(serverAddress + "fetchGameInfo.php", getGameInfo);

                yield return fetchGameInfo;

                if (fetchGameInfo.error == null)
                {
                    string[] gameInfoComponents = fetchGameInfo.text.Split('|');
                    string whoseTurn = (gameInfoComponents[1] == "0") ? "1" : "2";
                    Debug.Log("Player with global ID: " + GlobalData.instance.playerID + " is Player: " + gamePlayerID +
                              " in game with ID: " + gameStringComponents[0] + ". And they are " + gameStringComponents[2] +
                              " moves behind. The game's version is: " + gameInfoComponents[0] + ", the status is: " +
                              gameInfoComponents[1] + ", and it is player " + whoseTurn + "'s turn");
                }
                else
                {
                    Debug.LogError("Could not fetch game data.\n" + fetchGameInfo.error);
                    errorText.text = "Could not fetch game data, try again later.";
                }
            }
            */
        }
        else
        {
            Debug.LogError("Could not fetch user's games.\n" + fetchUserGames.error);
            errorText.text = "Could not fetch your games, try again later.";
        }

        NetworkController.instance.ServerCallTime(startTime, "PopulateUserHomepage");
    }

    void LoadGame(string gameID)
    {
        Debug.Log("LoadGame(string " + gameID + ")");
    }

    public void OnButtonCreateGame()
    {
        Debug.Log("OnButtonCreateGame()");

        tryAgainButtonHandler = OnButtonCreateGame;

        userHomepageCanvas.SetActive(false);
        draftTeamCanvas.SetActive(true);
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
            draftTeamCanvas.SetActive(true);
        }
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
        GlobalData.instance.inGamePlayerID = 0;

        WWWForm dbCredentials = new WWWForm();
        dbCredentials.AddField("username", dbUsername);
        dbCredentials.AddField("password", dbPassword);
        dbCredentials.AddField("playerID", GlobalData.instance.playerID);
        Debug.Log("PID = " + GlobalData.instance.playerID);
        WWW newGameRequest = new WWW(serverAddress + "createNewGame.php", dbCredentials);
        yield return newGameRequest;

        if (newGameRequest.error == null)
        {
            Debug.Log("New game created! ");
            int convertedInt;
            int.TryParse(newGameRequest.text, out convertedInt);
            GlobalData.instance.currentGameID = convertedInt;
            Debug.Log("GameID is: " + convertedInt.ToString());
            //Debug.Log("GameID is: " + newGameRequest.text);
            SceneManager.LoadScene("Scene2");
        }
        else
        {
            Debug.LogError("Error: could not create new game.\n" + newGameRequest.error);
            serverErrorText.text = "Error: " + newGameRequest.error;
            tryAgainButton.gameObject.SetActive(true);
        }
    }

    IEnumerator JoinGame(string gameID)
    {
        GlobalData.instance.inGamePlayerID = 1;
        int convertedInt;
        int.TryParse(gameID, out convertedInt);
        GlobalData.instance.currentGameID = convertedInt;
        WWWForm gameJoinID = new WWWForm();
        gameJoinID.AddField("gID", gameID);
        gameJoinID.AddField("username", dbUsername);
        gameJoinID.AddField("password", dbPassword);
        gameJoinID.AddField("playerID", GlobalData.instance.playerID);
        WWW attemptGameJoin = new WWW(serverAddress + "joinGame.php", gameJoinID);

        yield return attemptGameJoin;

        if (attemptGameJoin.error == null)
        {
            Debug.Log("Joining game with ID: " + gameID);
            Debug.Log("From Server: " + attemptGameJoin.text);
            SceneManager.LoadScene("Scene2");
        }
        else
        {
            Debug.LogError("Error: could not join game with ID: " + gameID + "\nError from PHP script: " + attemptGameJoin.error);
            serverErrorText.text = "Error: " + attemptGameJoin.error;
            tryAgainButton.gameObject.SetActive(true);
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
        GlobalData.instance.teamStr = teamStr;
        draftTeamCanvas.SetActive(false);
        loadingScreen.SetActive(true);
        if (gameIdToJoin == "")
        {
            StartCoroutine(JoinGame());
        }
        else
        {
            StartCoroutine(JoinGame(gameIdToJoin));
        }
    }

    public void OnClearButtonSelect()
    {
        foreach (Image i in team)
        {
            i.sprite = SelectionCircleStartSpr;
        }
        confirmButton.interactable = false;
        teamStr = "";
        numPlayersOnTeam = 0;
    }
}