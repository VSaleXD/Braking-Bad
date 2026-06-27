using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Attach this to each portal entrance trigger.
    public sealed class TeleportTrigger : MonoBehaviour
    {
        [SerializeField] private Minigame_PortalRush manager;
        [SerializeField] private Transform destination;
        [SerializeField] private int sequenceIndex;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (manager == null || other == null)
            {
                return;
            }

            TournamentPlayerAgent agent = other.GetComponentInParent<TournamentPlayerAgent>();
            if (agent != null)
            {
                manager.TryTeleportPlayer(agent, destination, sequenceIndex);
            }
        }
    }
}