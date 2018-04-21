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
        serverAddress = "homecookedgames.com/sbphp/scripts/";
    }

	public void SendStringToDB(string dataToSend, int nextPlayerID)
    {
        StartCoroutine(SendData(dataToSend, nextPlayerID));
    }

    public static IEnumerator AccountLogin(string user, string pin)
    {
        Debug.Log("AccountLogin(" + user + ", " + pin + ")");

        WWWForm form = new WWWForm();

        form.AddField("DBusername", dbUsername);
        form.AddField("password", dbPassword);
        form.AddField("username", user);
        form.AddField("pin", pin);

        WWW www = new WWW("http://" + serverAddress + "accountLogin.php", form);

        yield return www;

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

        WWWForm form = new WWWForm();

        form.AddField("DBusername", dbUsername);
        form.AddField("password", dbPassword);
        form.AddField("username", newUsername);
        form.AddField("pin", pin);
        form.AddField("email", emailAddress);

        WWW www = new WWW("http://" + serverAddress + "createAccount.php", form);

        yield return www;

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

    public IEnumerator SendData(string dataToSend, int nextPlayerID)
    {
        //Debug.Log("SendData(" + dataToSend + ", " + nextPlayerID + ")");
        WWWForm form = new WWWForm();

        form.AddField("newCmd", dataToSend);
        form.AddField("gID", GlobalData.instance.currentGameID);
        form.AddField("whoTurn", nextPlayerID);
        form.AddField("username", dbUsername);
        form.AddField("password", dbPassword);

        WWW www = new WWW("http://" + serverAddress + "sendCmd.php", form);

        yield return www;

        if (www.error == null)
        {
            //Debug.Log("WWW Sent! " + www.text);
        }
        else
        {
            Debug.LogError("WWW Did not send! " + www.error);
        }
    }


    public IEnumerator ReceiveData()
    {
        WWWForm form = new WWWForm();

        form.AddField("gID", GlobalData.instance.currentGameID);
        form.AddField("username", dbUsername);
        form.AddField("password", dbPassword);

        WWW www = new WWW("http://" + serverAddress + "receiveCmd.php", form);

        yield return www;

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

    public IEnumerator RecieveTurn()
    {
        WWWForm form = new WWWForm();

        form.AddField("gID", GlobalData.instance.currentGameID);
        form.AddField("username", dbUsername);
        form.AddField("password", dbPassword);

        WWW www = new WWW("http://" + serverAddress + "receiveTurn.php", form);

        yield return www;

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