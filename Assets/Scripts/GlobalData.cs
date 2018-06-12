using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData : MonoBehaviour {

    private static GlobalData _instance;

    public static GlobalData instance;

    public string playerID;
    //public int inGamePlayerID;
    //public int currentGameID;
    //public string teamStr;
    public string serverAddress;
    public bool hasSeenIntroVid, hasPlayedTutorial;
    public string deviceToken;
    public string otherTeam;

    LoadGameData loadGameDataHelper;

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
        loadGameDataHelper = new LoadGameData();
        loadGameDataHelper.ClearData();
    }

    public void SetupLoadGameDataHelper(string inGamePlayerID, string gameID, string lastCmdIndex, string gameUnitsInfo,
                                        string gameBoardInfo, string whoseTurn)
    {
        loadGameDataHelper = new LoadGameData(inGamePlayerID, gameID, lastCmdIndex, gameUnitsInfo, gameBoardInfo, whoseTurn);
    }

    public void ClearLoadGameDataHelper()
    {
        loadGameDataHelper.ClearData();
    }

    public bool GetCurGameData(out string[] gameData)
    {
        return loadGameDataHelper.GetGameData(out gameData);
    }

    public string CurGameDataToString()
    {
        return loadGameDataHelper.ToString();
    }
}


struct LoadGameData
{

    private string m_playerID, m_gameID, m_lastCmdIndex, m_gameUnitsInfo, m_gameBoardInfo, m_whoseTurn;
    private bool m_hasData;

    public LoadGameData(string inGamePlayerID, string gameID, string lastCmdIndex, string gameUnitsInfo, string gameBoardInfo,
                        string whoseTurn)
    {
        m_playerID = inGamePlayerID;
        m_gameID = gameID;
        m_lastCmdIndex = lastCmdIndex;
        m_gameUnitsInfo = gameUnitsInfo;
        m_gameBoardInfo = gameBoardInfo;
        m_whoseTurn = whoseTurn;
        m_hasData = true;
    }

    public void ClearData()
    {
        m_playerID = "";
        m_gameID = "";
        m_lastCmdIndex = "";
        m_gameUnitsInfo = "";
        m_gameBoardInfo = "";
        m_whoseTurn = "";
        m_hasData = false;
    }

    public bool GetGameData(out string[] gameData)
    {
        if (m_hasData)
        {
            gameData = new string[6];

            gameData[0] = m_playerID;
            gameData[1] = m_gameID;
            gameData[2] = m_lastCmdIndex;
            gameData[3] = m_gameUnitsInfo;
            gameData[4] = m_gameBoardInfo;
            gameData[5] = m_whoseTurn;
            return true;
        }
        else
        {
            gameData = null;
            return false;
        }
    }
    public override string ToString()
    {
        string returnStr = "";
        string[] gameData;
        if (GetGameData(out gameData))
        {
            foreach (string gd in gameData)
            {
                returnStr += gd + ", ";
            }
            returnStr = returnStr.Remove(returnStr.Length - 2);
            return returnStr;
        }
        return "No Game Data Stored.";
    }
}