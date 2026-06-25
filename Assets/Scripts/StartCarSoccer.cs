using UnityEngine;
using UnityEngine.SceneManagement;

public class StartCarSoccer : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("CarSoccer");
    }
}