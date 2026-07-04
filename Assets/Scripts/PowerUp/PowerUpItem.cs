using UnityEngine;

namespace BrakingBad.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PowerUpItem : MonoBehaviour
    {
        public static event System.Action<PowerUpType, Vector3> PowerUpPickedUp;

        [SerializeField] private PowerUpType type;
        [SerializeField] private float speedSurgeMultiplier = 1.5f;
        [SerializeField] private float speedSurgeDuration = 3f;
        [SerializeField] private float freezeDuration = 1.5f;
        [SerializeField] private float shieldDuration = 3f;
        [SerializeField] private GameObject pickupEffect;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TournamentPlayerAgent picker = other.GetComponentInParent<TournamentPlayerAgent>();
            if (picker == null) return;

            ApplyEffect(picker);

            if (pickupEffect != null)
            {
                GameObject effectInstance = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                ApplyEffectColor(effectInstance, GetPowerUpColor(type));
            }

            PowerUpPickedUp?.Invoke(type, transform.position);

            Destroy(gameObject);
        }
        public static Color GetPowerUpColor(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.SpeedSurge:
                    return Color.yellow;
                case PowerUpType.Freeze:
                    return new Color(0.45f, 0.8f, 1f, 1f);
                case PowerUpType.Shield:
                    return Color.gray;
                default:
                    return Color.white;
            }
        }

        private void ApplyEffectColor(GameObject effectInstance, Color color)
        {
            if (effectInstance == null)
            {
                return;
            }

            SpriteRenderer[] spriteRenderers = effectInstance.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.color = color;
            }

            ParticleSystem[] particleSystems = effectInstance.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                ParticleSystem.MainModule main = particleSystem.main;
                main.startColor = color;
            }
        }

        private void ApplyEffect(TournamentPlayerAgent picker)
        {
            playerController pickerController = picker.GetComponent<playerController>();

            switch (type)
            {
                case PowerUpType.SpeedSurge:
                    pickerController?.ApplySpeedSurge(speedSurgeMultiplier, speedSurgeDuration);
                    break;

                case PowerUpType.Freeze:
                    TournamentPlayerAgent target = FindNearestOpponent(picker);
                    target?.GetComponent<playerController>()?.ApplyFreeze(freezeDuration);
                    break;

                case PowerUpType.Shield:
                    pickerController?.ApplyShield(shieldDuration);
                    break;
            }
        }
        
        private TournamentPlayerAgent FindNearestOpponent(TournamentPlayerAgent picker)
        {
            TournamentPlayerAgent[] agents = FindObjectsByType<TournamentPlayerAgent>(FindObjectsSortMode.None);
            TournamentPlayerAgent nearest = null;
            float nearestSqr = float.MaxValue;

            foreach (TournamentPlayerAgent agent in agents)
            {
                if (agent == null || agent == picker || agent.IsEliminated) continue;

                float distSqr = (agent.transform.position - picker.transform.position).sqrMagnitude;
                if (distSqr < nearestSqr)
                {
                    nearestSqr = distSqr;
                    nearest = agent;
                }
            }

            return nearest;
        }
    }
}