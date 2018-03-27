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
    Text errorText;

    void Start()
    {
        gameIDinputField = mainMenuCanvas.transform.Find("GameIDInputField").GetComponent<InputField>();
        serverInputField = mainMenuCanvas.transform.Find("ServerInputField").GetComponent<InputField>();
        errorText = mainMenuCanvas.transform.Find("ErrorText").GetComponent<Text>();
        errorText.text = "";
        confirmButton.interactable = false;
        mainMenuCanvas.SetActive(false);
        SelectionCircleStartSpr = team[0].sprite;
    }

    public void OnButtonCreateGame()
    {
        GlobalData.instance.serverAddress = serverInputField.text;
        StartCoroutine(CreateGame());
    }

    public void OnButtonJoinGame()
    {
        string gameID = gameIDinputField.text;
        GlobalData.instance.serverAddress = serverInputField.text;

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

    IEnumerator CreateGame()
    {
        GlobalData.instance.playerID = 0;
        

        //WWW newGameRequest = new WWW("http://localhost/sb/createNewGame.php");
        WWW newGameRequest = new WWW("http://" + GlobalData.instance.serverAddress + "/sb/createNewGame.php");
        yield return newGameRequest;

        if (newGameRequest.error == null)
        {
            Debug.Log("New game created! ");
            int convertedInt;
            int.TryParse(newGameRequest.text, out convertedInt);
            GlobalData.instance.gameID = convertedInt;
            Debug.Log("GameID is: " + convertedInt.ToString());
        }
        else
        {
            Debug.LogError("Error: could not create new game.\n" + newGameRequest.error);
        }
        SceneManager.LoadScene("Scene2");
    }

    IEnumerator JoinGame(string gameID)
    {
        GlobalData.instance.playerID = 1;
        int convertedInt;
        int.TryParse(gameIDinputField.text, out convertedInt);
        GlobalData.instance.gameID = convertedInt;
        WWWForm gameJoinID = new WWWForm();
        gameJoinID.AddField("gID", gameID);

        //WWW attemptGameJoin = new WWW("http://localhost/sb/joinGame.php", gameJoinID);
        WWW attemptGameJoin = new WWW("http://" + GlobalData.instance.serverAddress + "/sb/joinGame.php", gameJoinID);

        yield return attemptGameJoin;

        if (attemptGameJoin.error == null)
        {
            Debug.Log("Joining game with ID: " + gameID);
            Debug.Log("From Server: " + attemptGameJoin.text);
        }
        else
        {
            Debug.LogError("Error: could not join game with ID: " + gameID + "\nError from PHP script: " + attemptGameJoin.error);
        }

        SceneManager.LoadScene("Scene2");
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
