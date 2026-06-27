using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// 2v2 soccer variant: goals award 100 points to every member of the scoring team.
    public sealed class Minigame_CarSoccer : BaseMinigameManager
    {
        [SerializeField] private float goalReward = 100f;

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
}