using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData : MonoBehaviour {

    private static GlobalData _instance;

    public static GlobalData instance;

    public string playerID;
    public int inGamePlayerID;
    public int currentGameID;
    public string teamStr;
    public string serverAddress;
    public bool hasSeenIntroVid, hasPlayedTutorial;

    void Awake()
    {
        hasSeenIntroVid = true;
        hasPlayedTutorial = true;

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
