using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Put one of these on each team base trigger.
    public sealed class BaseTrigger : MonoBehaviour
    {
        [SerializeField] private Minigame_CaptureTheFlag manager;
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
            manager.CaptureFlag(agent);
        }
    }
}