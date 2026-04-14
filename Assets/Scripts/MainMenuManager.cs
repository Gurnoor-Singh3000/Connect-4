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
        GameSettings.playAgainstAI = true; // Set the global state to AI
        SceneManager.LoadScene("Gameplay");
    }

    public void Load2PlayerGame()
    {
        GameSettings.playAgainstAI = false; // Set the global state to Player
        SceneManager.LoadScene("Gameplay");
    }

    public void ExitGame()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}