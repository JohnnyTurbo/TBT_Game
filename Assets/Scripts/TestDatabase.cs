using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestDatabase : MonoBehaviour {

    public GameObject dataCanvas;

    InputField inputDataField;
    Text outputDataText;

    void Start()
    {
        inputDataField = dataCanvas.transform.Find("InputData").GetComponent<InputField>();
        outputDataText = dataCanvas.transform.Find("OutputData").GetComponent<Text>();
    }

    public void OnButton_SendData()
    {
        //Debug.Log("Send Data button has been pressed.");

        string dataToSend = inputDataField.text;

        Debug.Log("Sending '" + dataToSend + "' to server");

        StartCoroutine(SendData(dataToSend));
    }

    public void OnButton_ReceiveData()
    {
        //Debug.Log("Receive Data button has been pressed.");

        string dataReceived = inputDataField.text;

        //Debug.Log("Mock receiving of string '" + dataReceived + "'");
        //outputDataText.text = dataReceived;

        StartCoroutine(ReceiveData());
    }

    IEnumerator SendData(string dataToSend)
    {
        WWWForm form = new WWWForm();

        form.AddField("name", dataToSend);
        form.AddField("owner", "Jon");
        form.AddField("species", "dawg");

        WWW www = new WWW("http://localhost/testDataSend.php", form);

        yield return www;

        if (www.error == null)
        {
            Debug.Log("WWW Sent! " + www.text);
        }
        else
        {
            Debug.LogError("WWW Did not send! " + www.error);
        }
    }

    IEnumerator ReceiveData()
    {
        WWW www = new WWW("http://localhost/testDataReceive.php");

        yield return www;

        if (www.error == null)
        {
            Debug.Log("WWW Received! " + www.text);
        }
        else
        {
            Debug.LogError("WWW did not receive! " + www.error);
        }
    }
}
