using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private string modeGameSceneName = "Gamemode";

    public void OnPlayPressed()
    {
        SceneManager.LoadScene(modeGameSceneName);
    }
    public void goToGarage()
    {
        SceneManager.LoadScene("Garage");
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();

// #if UNITY_EDITOR
//         UnityEditor.EditorApplication.isPlaying = false;
// #else
//         Application.Quit();
// #endif
    }
}