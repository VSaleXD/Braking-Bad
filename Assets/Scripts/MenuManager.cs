using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OnPlayPressed()
    {
        SceneManager.LoadScene("Gamemode");
    }
    public void goToGarage()
    {
        SceneManager.LoadScene("Garage");
    }
    public void returnToMenu()
    {
        SceneManager.LoadScene("Menu");
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