using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string modeGameSceneName = "ModeGame";

    /// <summary>
    /// Dipanggil oleh tombol PLAY.
    /// Pindah ke scene pemilihan mode (2 Player / 4 Player).
    /// </summary>
    public void OnPlayPressed()
    {
        SceneManager.LoadScene(modeGameSceneName);
    }

    /// <summary>
    /// Dipanggil oleh tombol EXIT.
    /// Menutup aplikasi (di editor Unity akan stop Play Mode).
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("Exiting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}