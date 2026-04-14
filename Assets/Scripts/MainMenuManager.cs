using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button onePlayerButton;
    public Button twoPlayerButton;
    public Button quitButton;

    void Start()
    {
        if (onePlayerButton != null) onePlayerButton.onClick.AddListener(Load1PlayerGame);
        if (twoPlayerButton != null) twoPlayerButton.onClick.AddListener(Load2PlayerGame);
        if (quitButton != null) quitButton.onClick.AddListener(ExitGame);
    }

    public void Load1PlayerGame()
    {
        GameSettings.playAgainstAI = true; 
    }

    public void Load2PlayerGame()
    {
        GameSettings.playAgainstAI = false; 
        SceneManager.LoadScene("Gameplay");
    }

    public void ExitGame()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}