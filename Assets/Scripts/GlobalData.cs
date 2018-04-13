using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData : MonoBehaviour {

    private static GlobalData _instance;

    public static GlobalData instance;

    public int playerID;
    public int gameID;
    public string teamStr;
    public string serverAddress;
    public bool hasSeenIntroVid = false, hasPlayedTutorial = false;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            _instance = this;
        }

        instance = _instance;
        DontDestroyOnLoad(gameObject);
    }
}
