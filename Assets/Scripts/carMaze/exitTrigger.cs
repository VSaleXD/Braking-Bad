using UnityEngine;

namespace BrakingBad.Gameplay
{
    public class exitTrigger : MonoBehaviour
    {
        private Minigame_CarMaze manager;

        private void Awake()
        {
            manager = FindFirstObjectByType<Minigame_CarMaze>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (manager == null || !manager.IsMatchStarted) return;

            var agent = collision.GetComponent<TournamentPlayerAgent>();
            if (agent == null) return;

            manager.OnPlayerReachExit(agent.PlayerID);
        }
    }
}
