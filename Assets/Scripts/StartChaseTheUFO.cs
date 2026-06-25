using UnityEngine.SceneManagement;
using UnityEngine;

public class StartChaseTheUfo : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("ChaseTheUfo");
    }
}