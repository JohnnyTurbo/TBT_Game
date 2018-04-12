using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {

    public GameObject mainMenuCanvas, selectPlayerCanvas;
    public Sprite[] athletes;
    public Image[] team;
    public Button confirmButton;

    int numPlayersOnTeam = 0;
    string teamStr;
    Sprite SelectionCircleStartSpr;
    InputField gameIDinputField, serverInputField;
    Text errorText, serverErrorText;
    GameObject loadingScreen;
    Button tryAgainButton;
    string dbUsername = "johnnytu_testusr", dbPassword = "OAnF8TqR12PJ";
    delegate void tryAgainFunction();
    tryAgainFunction handler;

    void Start()
    {
        gameIDinputField = mainMenuCanvas.transform.Find("GameIDInputField").GetComponent<InputField>();
        serverInputField = mainMenuCanvas.transform.Find("ServerInputField").GetComponent<InputField>();
        loadingScreen = mainMenuCanvas.transform.Find("LoadingScreen").gameObject;
        errorText = mainMenuCanvas.transform.Find("ErrorText").GetComponent<Text>();
        errorText.text = "";
        serverErrorText = loadingScreen.transform.Find("ServerErrorText").GetComponent<Text>();
        serverErrorText.text = "";
        tryAgainButton = loadingScreen.transform.Find("TryAgainButton").GetComponent<Button>();
        tryAgainButton.gameObject.SetActive(false);
        loadingScreen.SetActive(false);
        confirmButton.interactable = false;
        mainMenuCanvas.SetActive(false);
        SelectionCircleStartSpr = team[0].sprite;
    }

    public void OnButtonCreateGame()
    {
        Debug.Log("OnButtonCreateGame()");

        handler = OnButtonCreateGame;
        
        GlobalData.instance.serverAddress = serverInputField.text;
        loadingScreen.SetActive(true);
        
        StartCoroutine(CreateGame());
    }

    public void OnButtonJoinGame()
    {
        Debug.Log("OnButtonJoinGame()");

        handler = OnButtonJoinGame;

        string gameID = gameIDinputField.text;
        GlobalData.instance.serverAddress = serverInputField.text;
        loadingScreen.SetActive(true);

        if (gameID == "")
        {
            errorText.text = "Error: no game ID entered!";
            return;
        }
        else
        {
            errorText.text = "";
            StartCoroutine(JoinGame(gameID));
        }
    }

    public void OnButtonTryAgain()
    {
        Debug.Log("OnButtonTryAgain()");

        tryAgainButton.gameObject.SetActive(false);
        serverErrorText.text = "";
        handler();
    }

    IEnumerator CreateGame()
    {
        GlobalData.instance.playerID = 0;


        //WWW newGameRequest = new WWW("http://localhost/sb/createNewGame.php");
        //WWW newGameRequest = new WWW("http://" + GlobalData.instance.serverAddress + "/sb/createNewGame.php");
        WWWForm dbCredentials = new WWWForm();
        dbCredentials.AddField("username", dbUsername);
        dbCredentials.AddField("password", dbPassword);
        WWW newGameRequest = new WWW("http://homecookedgames.com/sbphp/scripts/createNewGame.php", dbCredentials);
        yield return newGameRequest;

        if (newGameRequest.error == null)
        {
            Debug.Log("New game created! ");
            int convertedInt;
            int.TryParse(newGameRequest.text, out convertedInt);
            GlobalData.instance.gameID = convertedInt;
            Debug.Log("GameID is: " + convertedInt.ToString());
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
        GlobalData.instance.playerID = 1;
        int convertedInt;
        int.TryParse(gameIDinputField.text, out convertedInt);
        GlobalData.instance.gameID = convertedInt;
        WWWForm gameJoinID = new WWWForm();
        gameJoinID.AddField("gID", gameID);
        gameJoinID.AddField("username", dbUsername);
        gameJoinID.AddField("password", dbPassword);
        //WWW attemptGameJoin = new WWW("http://localhost/sb/joinGame.php", gameJoinID);
        //WWW attemptGameJoin = new WWW("http://" + GlobalData.instance.serverAddress + "/sb/joinGame.php", gameJoinID);
        WWW attemptGameJoin = new WWW("http://homecookedgames.com/sbphp/scripts/joinGame.php", gameJoinID);

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

        if(numPlayersOnTeam >= 3)
        {
            return;
        }

        Sprite imgToUse = athletes[playerID];

        team[numPlayersOnTeam].sprite = imgToUse;
        numPlayersOnTeam++;

        teamStr += playerID;

        if(numPlayersOnTeam == 3)
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
        //teamStr = "&spn|" + teamStr;
        GlobalData.instance.teamStr = teamStr;
        selectPlayerCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
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
