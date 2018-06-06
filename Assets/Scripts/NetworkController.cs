using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviour {

    public static NetworkController instance;

    static string serverAddress;
    static string dbUsername = "johnnytu_testusr", dbPassword = "OAnF8TqR12PJ";

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //serverAddress = GlobalData.instance.serverAddress;
        serverAddress = "http://homecookedgames.com/sbphp/scripts/";
    }

    /*
	public void SendStringToDB(string dataToSend, int nextPlayerID, string gameID, string boardState)
    {
        StartCoroutine(SendData(dataToSend, nextPlayerID, gameID, boardState));
    }
    */

    public static IEnumerator AccountLogin(string user, string pin)
    {
        Debug.Log("AccountLogin(" + user + ", " + pin + ")");

        float networkFunctionStartTime = Time.time;

        WWWForm form = new WWWForm();

        form.AddField("DBusername", dbUsername);
        form.AddField("password", dbPassword);
        form.AddField("username", user);
        form.AddField("pin", pin);

        WWW www = new WWW(serverAddress + "accountLogin.php", form);

        yield return www;

        //instance.ServerCallTime(networkFunctionStartTime, "AccountLogin()");

        if (www.error == null)
        {
            Debug.Log("From accountLogin.php: " + www.text);
            MainMenuController.instance.AccountLoginCallback(www.text);
        }
        else
        {
            Debug.LogError("Could not login to account!\n " + www.error);
        }
    }

    public static IEnumerator CreateUserAccount(string newUsername, string pin, string emailAddress)
    {
        //Debug.Log("CreateUserAccount(" + newUsername + ", " + pin + ", " + emailAddress + ")");

        float networkFunctionStartTime = Time.time;

        WWWForm form = new WWWForm();

        form.AddField("DBusername", dbUsername);
        form.AddField("password", dbPassword);
        form.AddField("username", newUsername);
        form.AddField("pin", pin);
        form.AddField("email", emailAddress);
        form.AddField("deviceToken", GlobalData.instance.deviceToken);

        WWW www = new WWW(serverAddress + "createAccount.php", form);

        yield return www;

        //instance.ServerCallTime(networkFunctionStartTime, "CreateUserAccount()");

        if (www.error == null)
        {
            Debug.Log("WWW Sent! " + www.text);
            MainMenuController.instance.CreateAccountCallback(www.text);
        }
        else
        {
            Debug.LogError("Could not create new account!\n " + www.error);
        }
    }

    public IEnumerator SendData(string dataToSend, int nextPlayerID, string gameID, string boardState, string lastCMDIndex)
    {
        //Debug.Log("SendData(" + dataToSend + ", " + nextPlayerID + ")");

        float networkFunctionStartTime = Time.time;

        WWWForm form = new WWWForm();
        
        form.AddField("newCmd", dataToSend);
        form.AddField("gID", gameID);
        form.AddField("pID", GlobalData.instance.playerID);
        form.AddField("whoTurn", nextPlayerID);
        form.AddField("username", dbUsername);
        form.AddField("password", dbPassword);
        form.AddField("boardState", boardState);
        form.AddField("lastCMDIndex", lastCMDIndex);

        WWW www = new WWW(serverAddress + "sendCmd.php", form);

        yield return www;

        //ServerCallTime(networkFunctionStartTime, "SebdData()");

        if (www.error == null)
        {
            Debug.Log("WWW Sent! " + www.text);
        }
        else
        {
            Debug.LogError("WWW Did not send! " + www.error);
        }
    }


    public IEnumerator ReceiveData(string gameID)
    {
        //Debug.Log("ReceiveData()");

        float networkFunctionStartTime = Time.time;
        
        WWWForm form = new WWWForm();

        form.AddField("gID", gameID);
        form.AddField("username", dbUsername);
        form.AddField("password", dbPassword);

        WWW www = new WWW(serverAddress + "receiveCmd.php", form);

        yield return www;

        //ServerCallTime(networkFunctionStartTime, "ReceiveData()");

        if (www.error == null)
        {
            //Debug.Log("WWW Received! " + www.text);
            yield return www.text;
        }
        else
        {
            Debug.LogError("WWW did not receive! " + www.error);
        }
    }

    public IEnumerator RecieveTurn(string gameID)
    {
        //Debug.Log("RecieveTurn()");

        float networkFunctionStartTime = Time.time;
        
        WWWForm form = new WWWForm();

        form.AddField("gID", gameID);
        form.AddField("username", dbUsername);
        form.AddField("password", dbPassword);

        WWW www = new WWW(serverAddress + "receiveTurn.php", form);

        yield return www;

        //ServerCallTime(networkFunctionStartTime, "ReceiveTurn()");

        if (www.error == null)
        {
            //Debug.Log("WWW Received! " + www.text);
            yield return www.text;
        }
        else
        {
            Debug.LogError("WWW did not receive! " + www.error);
        }
    }

    public IEnumerator EndGame(string gameID)
    {
        Debug.Log("Ending Game");

        float networkFunctionStartTime = Time.time;

        WWWForm form = new WWWForm();

        form.AddField("username", dbUsername);
        form.AddField("password", dbPassword);
        form.AddField("gID", gameID);
        form.AddField("playerID", GlobalData.instance.playerID);

        WWW www = new WWW(serverAddress + "endGame.php", form);

        yield return www;

        //ServerCallTime(networkFunctionStartTime, "ReceiveTurn()");

        if (www.error == null)
        {
            Debug.Log("WWW Ended Game! " + www.text);
            yield return www.text;
        }
        else
        {
            Debug.LogError("WWW did not end game! " + www.error);
        }
    }

    public void ServerCallTime(float startTime, string functionName)
    {
        float duration = Time.time - startTime;
        Debug.Log("Server function: " + functionName + " took " + duration + " seconds to complete");
    }
}

public class CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}