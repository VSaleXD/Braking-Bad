using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// <summary>
    /// 2v2 soccer variant: goals award 100 points to every member of the scoring team.
    /// </summary>
    public sealed class Minigame_CarSoccer : BaseMinigameManager
    {
        [SerializeField] private float goalReward = 100f;
        [SerializeField] private string ballTag = "Ball";

        public void RegisterGoal(int scoringTeamIndex)
        {
            foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
            {
                if (agent.TeamIndex == scoringTeamIndex)
                {
                    AddGameplayScore(agent.PlayerID, goalReward, $"Team {scoringTeamIndex + 1} scored!");
                }
            }
        }
    }

    /// <summary>
    /// Put this on left/right goal trigger colliders and point it at the soccer manager.
    /// </summary>
    public sealed class CarSoccerGoalTrigger : MonoBehaviour
    {
        [SerializeField] private Minigame_CarSoccer manager;
        [SerializeField, Range(0, 1)] private int scoringTeamIndex;
        [SerializeField] private string ballTag = "Ball";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (manager == null || other == null)
            {
                return;
            }

            if (!other.CompareTag(ballTag))
            {
                return;
            }

            manager.RegisterGoal(scoringTeamIndex);
        }
    }
}