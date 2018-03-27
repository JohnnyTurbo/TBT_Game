using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviour {

    public static NetworkController instance;

    string serverAddress;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        serverAddress = GlobalData.instance.serverAddress;
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
        //WWW www = new WWW("http://localhost/sb/sendCmd.php", form);
        WWW www = new WWW("http://" + serverAddress + "/sb/sendCmd.php", form);

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

        WWW www = new WWW("http://" + serverAddress + "/sb/receiveCmd.php", form);

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

        WWW www = new WWW("http://" + serverAddress + "/sb/receiveTurn.php", form);

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