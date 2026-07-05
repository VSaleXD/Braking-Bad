using UnityEngine;

namespace BrakingBad.Garage
{
    [CreateAssetMenu(fileName = "VehicleData", menuName = "Braking Bad/Vehicle Data")]
    public sealed class VehicleData : ScriptableObject
    {
        [Header("Info")]
        public string vehicleName = "Car";

        public Sprite thumbnailSprite;
        
        [Header("Sprites per Player")]
        [Tooltip("Sprite untuk P1 (putih)")]
        public Sprite spriteP1White;
        [Tooltip("Sprite untuk P2 (merah)")]
        public Sprite spriteP2Red;
        [Tooltip("Sprite untuk P3 (biru)")]
        public Sprite spriteP3Blue;
        [Tooltip("Sprite untuk P4 (oranye)")]
        public Sprite spriteP4Orange;
        public Sprite GetSpriteForPlayer(int playerID)
        {
            switch (playerID)
            {
                case 1: return spriteP1White;
                case 2: return spriteP2Red;
                case 3: return spriteP3Blue;
                case 4: return spriteP4Orange;
                default: return thumbnailSprite;
            }
        }
    }
}