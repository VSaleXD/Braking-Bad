using UnityEngine;

namespace BrakingBad.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PowerUpItem : MonoBehaviour
    {
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
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
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