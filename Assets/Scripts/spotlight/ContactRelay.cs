using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Put this on player vehicles so contact with the target can swap the spotlight.
    public sealed class ContactRelay : MonoBehaviour
    {
        [SerializeField] private Minigame_Spotlight manager;
        private TournamentPlayerAgent cachedAgent;

        private void Awake()
        {
            cachedAgent = GetComponentInParent<TournamentPlayerAgent>();
            
            if (manager == null)
            {
                manager = FindFirstObjectByType<Minigame_Spotlight>();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (manager == null || cachedAgent == null)
            {
                return;
            }

            TournamentPlayerAgent otherAgent = collision.collider.GetComponentInParent<TournamentPlayerAgent>();
            
            if (otherAgent != null && otherAgent != cachedAgent)
            {
                if (cachedAgent.PlayerID == manager.CurrentTargetPlayerID)
                {
                    manager.TrySwapTarget(otherAgent);
                }
            }
        }
    }
}