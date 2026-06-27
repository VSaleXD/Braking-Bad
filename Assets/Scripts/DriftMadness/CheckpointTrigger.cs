using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Trigger placed on each checkpoint collider.
    public sealed class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private Minigame_DriftMadness manager;
        [SerializeField] private int checkpointIndex;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (manager == null || other == null)
            {
                return;
            }

            TournamentPlayerAgent agent = other.GetComponentInParent<TournamentPlayerAgent>();
            if (agent != null)
            {
                manager.TryRegisterCheckpoint(agent, checkpointIndex);
            }
        }
    }
}