using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseNotificationController : MonoBehaviour {

    public void Start()
    {
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        //Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

        Firebase.Messaging.FirebaseMessaging.Subscribe("all");

    }

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        Debug.Log("Received Registration Token: " + token.Token);
        //MainMenuController.instance.SetErrorText("Received Registration Token: " + token.Token);
        GlobalData.instance.deviceToken = token.Token;
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        Debug.Log("Received a new message from: " + e.Message.From);
        //MainMenuController.instance.SetErrorText("Received a new message from: " + e.Message.From);
    }
}
