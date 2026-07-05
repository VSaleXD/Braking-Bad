using UnityEngine;
using UnityEngine.SceneManagement;

namespace BrakingBad.Garage
{
    /// <summary>
    /// Koordinator utama scene Garage.
    /// - Inisialisasi panel per player sesuai activePlayerCount dari TournamentManager.
    /// - Forward input ke tiap panel di Update().
    /// - Tombol "Back to Menu" dan (opsional) "Confirm" terhubung ke method ini.
    /// </summary>
    public sealed class GarageManager : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("Drag VehicleRegistry asset dari Project window ke sini.")]
        [SerializeField] private VehicleRegistry registry;

        [Header("Panel per Player (urut: panel P1, P2, P3, P4)")]
        [SerializeField] private GaragePlayerPanel[] playerPanels = new GaragePlayerPanel[4];

        [Header("Scene Navigation")]
        [SerializeField] private string menuSceneName = "MenuBaru";

        private void Start()
        {
            if (registry == null || registry.Count == 0)
            {
                Debug.LogError("GarageManager: VehicleRegistry belum di-assign atau kosong!");
                return;
            }

            // Cek berapa player yang aktif dari TournamentManager
            // Kalau TournamentManager belum ada (masuk Garage langsung dari Editor),
            // default ke 4 supaya bisa preview semua panel.
            int activeCount = 4;
            if (BrakingBad.Gameplay.TournamentManager.Instance != null)
            {
                activeCount = BrakingBad.Gameplay.TournamentManager.Instance.ActivePlayerCount;
            }
            else
            {
                Debug.LogWarning("GarageManager: TournamentManager.Instance tidak ditemukan, tampilkan semua 4 panel.");
            }

            // Init dan set visibility tiap panel
            for (int i = 0; i < playerPanels.Length; i++)
            {
                if (playerPanels[i] == null) continue;

                int playerID = i + 1; // panel index 0 = P1, dst
                bool isActive = playerID <= activeCount;

                playerPanels[i].SetVisible(isActive);

                if (isActive)
                {
                    playerPanels[i].Init(playerID, registry);
                }
            }
        }

        public void BackToMenu()
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}