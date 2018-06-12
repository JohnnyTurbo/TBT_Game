using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSceneController : MonoBehaviour {

    int tutScreen = 0;
    public Text tutorialText;
    public Text buttonText;
	public void OnButtonHome()
    {
        Debug.Log("OnButtonHome()");
        UnityEngine.SceneManagement.SceneManager.LoadScene("PublicAlphaMainMenu");
    }

    public void OnButtonNext()
    {
        tutScreen++;
        tutScreen %= 6;

        switch (tutScreen)
        {
            case 0:
                tutorialText.text = "From the main menu, press Play, then create an account and login.";
                buttonText.text = "Next";
                break;

            case 1:
                tutorialText.text = "Now either create a new game or accept an existing request.";
                break;

            case 2:
                tutorialText.text = "Next draft the team of your liking.";
                break;
            case 3:
                tutorialText.text = "Once both players have joined, Player 2 makes the first move.";
                break;
            case 4:
                tutorialText.text = "Each turn, every athelete can move and attack.";
                break;
            case 5:
                tutorialText.text = "Have fun and experiment with different things!";
                buttonText.text = "Again";
                break;
            default:

                break;
        }
    }
}
