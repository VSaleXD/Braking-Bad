using UnityEngine;
using System.Collections.Generic;

namespace BrakingBad.Garage
{
    [CreateAssetMenu(fileName = "VehicleRegistry", menuName = "Braking Bad/Vehicle Registry")]
    public sealed class VehicleRegistry : ScriptableObject
    {
        [Tooltip("Daftar semua kendaraan yang tersedia. Urutan ini yang tampil di Garage.")]
        public List<VehicleData> vehicles = new List<VehicleData>();

        public int Count => vehicles.Count;

        public VehicleData Get(int index)
        {
            if (vehicles == null || vehicles.Count == 0) return null;
            int safeIndex = ((index % vehicles.Count) + vehicles.Count) % vehicles.Count;
            return vehicles[safeIndex];
        }
        public int IndexOf(VehicleData data)
        {
            return vehicles.IndexOf(data);
        }
    }
}