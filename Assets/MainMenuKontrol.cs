using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenuKontrol : MonoBehaviour
{
   public void OnStartClick() {
    Debug.Log("OnStartClick called");
    SceneManager.LoadScene("CarSoccer");
   }

   public void OnExitClick () {
    Debug.Log("OnExitClick called");
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    Application.Quit();
   }
}
