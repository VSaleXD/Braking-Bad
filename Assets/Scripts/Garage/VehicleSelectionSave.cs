using UnityEngine;

namespace BrakingBad.Garage
{
    /// <summary>
    /// Helper statik untuk simpan/baca pilihan kendaraan tiap player ke PlayerPrefs.
    /// Key: "VehicleIndex_P1", "VehicleIndex_P2", dst.
    /// Default: index 0 (kendaraan pertama di registry).
    /// </summary>
    public static class VehicleSelectionSave
    {
        private const string KeyPrefix = "VehicleIndex_P";

        public static void Save(int playerID, int vehicleIndex)
        {
            PlayerPrefs.SetInt(KeyPrefix + playerID, vehicleIndex);
            PlayerPrefs.Save();
        }

        public static int Load(int playerID)
        {
            return PlayerPrefs.GetInt(KeyPrefix + playerID, 0);
        }

        /// <summary>
        /// Ambil VehicleData untuk player tertentu langsung dari registry.
        /// Dipakai oleh TournamentPlayerAgent saat Awake untuk apply sprite.
        /// </summary>
        public static VehicleData LoadVehicle(int playerID, VehicleRegistry registry)
        {
            if (registry == null || registry.Count == 0) return null;
            int index = Load(playerID);
            return registry.Get(index);
        }
    }
}