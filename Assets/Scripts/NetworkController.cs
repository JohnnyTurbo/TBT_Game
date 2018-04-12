using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviour {

    public static NetworkController instance;

    string serverAddress;
    string dbUsername = "johnnytu_testusr", dbPassword = "OAnF8TqR12PJ";

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

    /*
    public string ReceiveDataFromDB()
    {

    }
    */

    public IEnumerator SendData(string dataToSend, int nextPlayerID)
    {
        WWWForm form = new WWWForm();

        form.AddField("newCmd", dataToSend);
        form.AddField("gID", GlobalData.instance.gameID);
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

        form.AddField("gID", GlobalData.instance.gameID);
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

        form.AddField("gID", GlobalData.instance.gameID);
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