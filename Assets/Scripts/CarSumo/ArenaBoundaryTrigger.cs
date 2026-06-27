using UnityEngine;

namespace BrakingBad.Gameplay
{
    public sealed class ArenaBoundaryTrigger : MonoBehaviour
    {
        [SerializeField] private Minigame_CarSumo manager;

        private void OnTriggerExit2D(Collider2D other)
        {
            if (manager == null || other == null)
            {
                return;
            }

            TournamentPlayerAgent agent = other.GetComponentInParent<TournamentPlayerAgent>();
            if (agent != null)
            {
                manager.HandleArenaExit(agent);
            }
        }
    }
}