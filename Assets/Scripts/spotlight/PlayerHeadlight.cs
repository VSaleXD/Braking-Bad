using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BrakingBad.Gameplay
{
    /// Headlight individual tiap mobil untuk mode Spotlight (dan mode lain yang
    /// butuh visibility terbatas). Light ini otomatis ikut posisi & rotasi mobil
    /// karena jadi child Transform-nya.
    public sealed class PlayerHeadlight : MonoBehaviour
    {
        [SerializeField] private Light2D headlight;
        [SerializeField] private float normalIntensity = 1f;
        [SerializeField] private float normalOuterRadius = 3f;
        [SerializeField] private float normalOuterAngle = 50f;

        [SerializeField] private float targetIntensity = 2f;
        [SerializeField] private float targetOuterRadius = 7f;
        [SerializeField] private float targetOuterAngle = 80f;

        private void Awake()
        {
            if (headlight == null)
            {
                headlight = GetComponentInChildren<Light2D>();
            }
        }

        public void SetAsTarget(bool isTarget)
        {
            if (headlight == null) return;

            headlight.intensity = isTarget ? targetIntensity : normalIntensity;
            headlight.pointLightOuterRadius = isTarget ? targetOuterRadius : normalOuterRadius;
            headlight.pointLightOuterAngle = isTarget ? targetOuterAngle : normalOuterAngle;
        }
    }
}