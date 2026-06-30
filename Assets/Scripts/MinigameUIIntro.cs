using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace BrakingBad.Gameplay
{
    public sealed class MinigameIntroUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject introPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI countdownText;
        private bool isWaitingForInput = false;

        private readonly Dictionary<string, (string title, string desc)> minigameInfo = new Dictionary<string, (string, string)>
        {
            { "CarSoccer", ("CAR SOCCER", "Tabrak bola masuk ke gawang untuk mencetak poin sebanyak-banyaknya!") },
            { "PoliceChase", ("POLICE CHASE", "Hindari semua mobil polisi yang datang. Bertahanlah paling lama!") },
            { "CarSumo", ("CAR SUMO", "Saling dorong keluar arena! Jangan biarkan mobilmu melewati batas lingkaran luar.") },
            { "ChaseTheUFO", ("CHASE THE UFO", "Kejar UFO yang terbang bebas dan tabrak dia untuk mencuri poin!") },
            { "FloorIsLava", ("FLOOR IS LAVA", "Lantai akan retak dan hancur! Teruslah bergerak dan jadilah yang terakhir selamat.") },
            { "Spotlight", ("SPOTLIGHT", "REBUT LAMPU SOROT! Tabrak pemegang spotlight untuk mencuri poin per detik!") },
            { "CaptureTheFlag", ("CAPTURE THE FLAG", "Ambil bendera dan bawa pulang ke tengah arena.") }
        };

        private void Start()
        {
            SetupIntro();
        }

        private void Update()
        {
            if (isWaitingForInput)
            {
                bool keyboardPressed = UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame;
                bool mousePressed = UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;

                if (keyboardPressed || mousePressed)
                {
                    isWaitingForInput = false;
                    StartCoroutine(CountdownSequenceRoutine());
                }
            }
        }

        private void SetupIntro()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (minigameInfo.ContainsKey(currentScene))
            {
                titleText.text = minigameInfo[currentScene].title;
                descriptionText.text = minigameInfo[currentScene].desc;
            }
            else
            {
                titleText.text = "MINIGAME MATCH";
                descriptionText.text = "Kumpulkan poin tertinggi untuk memenangkan turnamen!";
            }

            if (countdownText != null) 
            {
                countdownText.gameObject.SetActive(false);
            }

            Time.timeScale = 0f; 
            SetPlayersControl(false);

            isWaitingForInput = true;
        }

        private IEnumerator CountdownSequenceRoutine()
        {
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                
                countdownText.text = "3"; yield return new WaitForSecondsRealtime(1f);
                countdownText.text = "2"; yield return new WaitForSecondsRealtime(1f);
                countdownText.text = "1"; yield return new WaitForSecondsRealtime(1f);
                
                countdownText.text = "START!";
            }


            Time.timeScale = 1f;

            BaseMinigameManager minigameManager = FindFirstObjectByType<BaseMinigameManager>();
            if (minigameManager != null)
            {
                minigameManager.StartMinigameMatch(); 
            }

            SetPlayersControl(true);
            
            yield return new WaitForSecondsRealtime(0.8f);
            introPanel.SetActive(false);
        }

        private void SetPlayersControl(bool enabled)
        {
            TournamentPlayerAgent[] agents = FindObjectsByType<TournamentPlayerAgent>(FindObjectsSortMode.None);
            foreach (var agent in agents)
            {
                playerController controller = agent.GetComponent<playerController>();
                if (controller != null)
                {
                    controller.movementEnabled = enabled;
                }
                if (!enabled)
                {
                    agent.ResetVelocity();
                }
            }
        }
    }
}