using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour {

    public GameObject mainMenuCanvas;

    InputField gameIDinputField;
    Text errorText;

    void Start()
    {
        gameIDinputField = mainMenuCanvas.transform.Find("GameIDInputField").GetComponent<InputField>();
        errorText = mainMenuCanvas.transform.Find("ErrorText").GetComponent<Text>();
        errorText.text = "";
    }

    public void OnButtonCreateGame()
    {
        StartCoroutine(CreateGame());
    }

    public void OnButtonJoinGame()
    {
        string gameID = gameIDinputField.text;

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
        WWW newGameRequest = new WWW("http://localhost/sb/createNewGame.php");

        yield return newGameRequest;

        if (newGameRequest.error == null)
        {
            Debug.Log("New game created! " + newGameRequest.text);
        }
        else
        {
            Debug.LogError("Error: could not create new game.\n" + newGameRequest.error);
        }
    }

    IEnumerator JoinGame(string gameID)
    {
        WWWForm gameJoinID = new WWWForm();
        gameJoinID.AddField("gID", gameID);

        WWW attemptGameJoin = new WWW("http://localhost/sb/joinGame.php", gameJoinID);

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


    }

}
