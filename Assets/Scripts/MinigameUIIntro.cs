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

        // Kamus deskripsi untuk 7 minigame yang tersisa
        private readonly Dictionary<string, (string title, string desc)> minigameInfo = new Dictionary<string, (string, string)>
        {
            { "Car Soccer", ("CAR SOCCER", "Tabrak bola raksasa masuk ke gawang lawan untuk mencetak poin sebanyak-banyaknya!") },
            { "Obstacle Survival", ("OBSTACLE SURVIVAL", "Hindari semua mobil polisi yang berdatangan. Bertahanlah paling lama!") },
            { "Car Sumo", ("CAR SUMO", "Saling dorong keluar arena! Jangan biarkan mobilmu melewati batas lingkaran luar.") },
            { "Chase The UFO", ("CHASE THE UFO", "Kejar UFO yang terbang bebas dan tabrak dia untuk mendapat poin!") },
            { "Floor Is Lava", ("FLOOR IS LAVA", "Lantai akan retak dan hancur! Teruslah bergerak dan jadilah yang terakhir selamat.") },
            { "Spotlight", ("SPOTLIGHT", "Kejar mobil yang (spotlight) untuk mengumpulkan skor per detik. (seperti tag)") },
            { "Capture The Flag", ("CAPTURE THE FLAG", "Ambil bendera di sekeliling arena dan bawa pulang ke tengah lingkaran") }
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

            SetPlayersControl(false);

            StartCoroutine(IntroSequenceRoutine());
        }

        private IEnumerator IntroSequenceRoutine()
        {
            introPanel.SetActive(true);
            countdownText.text = "";

            yield return new WaitForSeconds(3.5f);

            countdownText.gameObject.SetActive(true);
            
            countdownText.text = "3";
            yield return new WaitForSeconds(1f);
            
            countdownText.text = "2";
            yield return new WaitForSeconds(1f);
            
            countdownText.text = "1";
            yield return new WaitForSeconds(1f);
            
            countdownText.text = "START!";
            BaseMinigameManager minigameManager = FindObjectOfType<BaseMinigameManager>();
            if (minigameManager != null)
            {
                minigameManager.StartMinigameMatch();
            }
            else
            {
                Debug.LogWarning("MinigameIntroUI: BaseMinigameManager tidak ditemukan di scene.");
            }
            
            SetPlayersControl(true);
            
            yield return new WaitForSeconds(0.8f);

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