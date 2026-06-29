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

        private readonly Dictionary<string, (string title, string desc)> minigameInfo = new Dictionary<string, (string, string)>
        {
            { "Minigame_CarSoccer", ("CAR SOCCER", "Tabrak bola raksasa masuk ke gawang lawan untuk mencetak poin sebanyak-banyaknya!") },
            { "Minigame_ObstacleSurvival", ("OBSTACLE SURVIVAL", "Hindari semua rintangan (hazard) yang berjatuhan. Bertahanlah paling lama!") },
            { "Minigame_CarSumo", ("CAR SUMO", "Saling dorong keluar arena! Jangan biarkan mobilmu melewati batas lingkaran luar.") },
            { "Minigame_ChaseTheUFO", ("CHASE THE UFO", "Kejar UFO yang terbang bebas dan tabrak dia untuk mencuri poin!") },
            { "Minigame_FloorIsLava", ("FLOOR IS LAVA", "Lantai akan retak dan hancur! Teruslah bergerak dan jadilah yang terakhir selamat.") },
            { "Minigame_Spotlight", ("SPOTLIGHT", "Kejar dan diamlah di bawah lampu sorot (spotlight) untuk mengumpulkan skor per detik.") },
            { "Minigame_CaptureTheFlag", ("CAPTURE THE FLAG", "Ambil bendera di tengah dan bawa pulang ke markas timmu untuk poin besar.") }
        };

        private void Start()
        {
            SetupIntro();
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
            Time.timeScale = 0f; 

            StartCoroutine(IntroSequenceRoutine());
        }

        private IEnumerator IntroSequenceRoutine()
        {
            introPanel.SetActive(true);
            countdownText.text = "";

            yield return new WaitForSecondsRealtime(3.5f);

            countdownText.gameObject.SetActive(true);
            
            countdownText.text = "3"; yield return new WaitForSecondsRealtime(1f);
            countdownText.text = "2"; yield return new WaitForSecondsRealtime(1f);
            countdownText.text = "1"; yield return new WaitForSecondsRealtime(1f);
            
            countdownText.text = "START!";
            
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