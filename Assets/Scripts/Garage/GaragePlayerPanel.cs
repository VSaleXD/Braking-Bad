using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BrakingBad.Garage
{
    /// <summary>
    /// Satu panel kolom untuk satu player di scene Garage.
    /// Pasang di GameObject panel, isi referensi UI di Inspector.
    /// GarageManager yang memanggil Init() dan HandleInput() tiap frame.
    /// </summary>
    public sealed class GaragePlayerPanel : MonoBehaviour
    {
        [Header("Referensi UI")]
        [SerializeField] private Image vehiclePreviewImage;
        [SerializeField] private TextMeshProUGUI playerLabel;       // "P1", "P2", dst
        [SerializeField] private TextMeshProUGUI vehicleNameLabel;  // nama kendaraan
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;

        [Header("Warna label per player")]
        [SerializeField] private Color labelColor = Color.white;

        // State internal
        private VehicleRegistry registry;
        private int playerID;
        private int currentIndex;

        public int CurrentIndex => currentIndex;

        public void Init(int id, VehicleRegistry reg)
        {
            playerID = id;
            registry  = reg;

            // Load pilihan sebelumnya dari PlayerPrefs
            currentIndex = VehicleSelectionSave.Load(playerID);

            // Pastikan index masih valid (registry bisa berubah)
            if (currentIndex >= registry.Count) currentIndex = 0;

            // Label player
            if (playerLabel != null)
            {
                playerLabel.text  = $"P{playerID}";
                playerLabel.color = labelColor;
            }

            // Tombol prev/next (opsional, bisa juga keyboard-only)
            if (prevButton != null) prevButton.onClick.AddListener(Previous);
            if (nextButton != null) nextButton.onClick.AddListener(Next);

            RefreshUI();
        }

        private void Previous()
        {
            currentIndex = ((currentIndex - 1) + registry.Count) % registry.Count;
            RefreshUI();
            SaveCurrent();
        }

        private void Next()
        {
            currentIndex = (currentIndex + 1) % registry.Count;
            RefreshUI();
            SaveCurrent();
        }

        private void RefreshUI()
        {
            VehicleData data = registry.Get(currentIndex);
            if (data == null) return;

            // Preview pakai sprite warna player ini
            if (vehiclePreviewImage != null)
                vehiclePreviewImage.sprite = data.GetSpriteForPlayer(playerID);

            if (vehicleNameLabel != null)
                vehicleNameLabel.text = data.vehicleName;
        }

        private void SaveCurrent()
        {
            VehicleSelectionSave.Save(playerID, currentIndex);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}