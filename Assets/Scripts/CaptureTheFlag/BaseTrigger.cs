using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Put one of these on each team base trigger.
    public sealed class BaseTrigger : MonoBehaviour
    {
        [SerializeField] private Minigame_CaptureTheFlag manager;
        [SerializeField] private int teamIndex;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (manager == null || other == null)
            {
                return;
            }

            TournamentPlayerAgent agent = other.GetComponentInParent<TournamentPlayerAgent>();
            if (agent == null)
            {
                return;
            }

            if (agent.TeamIndex == teamIndex)
            {
                manager.CaptureFlag(agent);
            }
        }
    }
}