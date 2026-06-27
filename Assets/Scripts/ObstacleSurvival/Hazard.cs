using UnityEngine;

namespace BrakingBad.Gameplay
{
    
    /// Put this on a hazard prefab so it can notify the survival manager on impact.

    public sealed class Hazard : MonoBehaviour
    {
        [SerializeField] private Minigame_ObstacleSurvival manager;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (manager == null)
            {
                return;
            }

            TournamentPlayerAgent agent = collision.collider.GetComponentInParent<TournamentPlayerAgent>();
            if (agent != null)
            {
                manager.HandleHazardCollision(agent);
            }
        }
    }
}