using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Procedural checkpoint minigame: players earn score from checkpoint progress, speed, and remaining time.
    public class Minigame_DriftMadness : BaseMinigameManager
    {
        [SerializeField] private float checkpointBaseScore = 15f;
        [SerializeField] private float speedScoreMultiplier = 3f;
        [SerializeField] private float remainingTimeScoreMultiplier = 0.35f;
        [SerializeField] private bool requireSequentialCheckpoints = true;

        private readonly Dictionary<int, int> nextCheckpointIndex = new Dictionary<int, int>();

        protected override void OnMatchStarted()
        {
            nextCheckpointIndex.Clear();

            foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
            {
                nextCheckpointIndex[agent.PlayerID] = 0;
            }
        }

        public bool TryRegisterCheckpoint(TournamentPlayerAgent agent, int checkpointIndex)
        {
            if (agent == null)
            {
                return false;
            }

            if (!nextCheckpointIndex.ContainsKey(agent.PlayerID))
            {
                nextCheckpointIndex[agent.PlayerID] = 0;
            }

            if (requireSequentialCheckpoints && checkpointIndex != nextCheckpointIndex[agent.PlayerID])
            {
                return false;
            }

            nextCheckpointIndex[agent.PlayerID] = checkpointIndex + 1;

            float checkpointScore = checkpointBaseScore;
            checkpointScore += agent.GetSpeed() * speedScoreMultiplier;
            checkpointScore += matchTimer * remainingTimeScoreMultiplier;

            AddGameplayScore(agent.PlayerID, checkpointScore, $"Checkpoint {checkpointIndex + 1}");
            return true;
        }
    }
}