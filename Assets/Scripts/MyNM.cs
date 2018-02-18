using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking; 

public class MyNM : NetworkManager {

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("OnClientConnect()");
        base.OnClientConnect(conn);
    }

    public override void OnStartServer()
    {
        Debug.Log("OnStartServer()");
        
        base.OnStartServer();
    }
}
