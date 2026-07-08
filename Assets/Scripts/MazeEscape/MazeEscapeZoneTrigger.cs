using UnityEngine;

namespace BrakingBad.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class MazeEscapeZoneTrigger : MonoBehaviour
    {
        private Minigame_MazeEscape manager;

        private void Start()
        {
            manager = FindFirstObjectByType<Minigame_MazeEscape>();
            if (manager == null)
            {
                Debug.LogWarning("MazeEscapeZoneTrigger could not find Minigame_MazeEscape in the scene.");
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (manager == null || !manager.IsMatchStarted) return;

            TournamentPlayerAgent player = collision.GetComponentInParent<TournamentPlayerAgent>();
            if (player != null && !player.IsEliminated)
            {
                manager.OnPlayerEscaped(player);
            }
        }
    }
}
