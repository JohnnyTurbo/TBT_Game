using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSceneController : MonoBehaviour {

	public void OnButtonHome()
    {
        Debug.Log("OnButtonHome()");
        UnityEngine.SceneManagement.SceneManager.LoadScene("PublicAlphaMainMenu");
    }
}
