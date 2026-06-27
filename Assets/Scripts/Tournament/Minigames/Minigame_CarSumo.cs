using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Circle arena elimination mode. The first car out scores 0, the survivor gets the highest score.
    public sealed class Minigame_CarSumo : BaseMinigameManager
    {
        [SerializeField] private float survivorBonus = 3f;

        private readonly HashSet<int> eliminatedPlayers = new HashSet<int>();

        protected override void OnMatchStarted()
        {
            eliminatedPlayers.Clear();

            foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
            {
                agent.ResetForNewMatch();
            }
        }

        public void HandleArenaExit(TournamentPlayerAgent agent)
        {
            if (agent == null || eliminatedPlayers.Contains(agent.PlayerID))
            {
                return;
            }

            int eliminationOrder = eliminatedPlayers.Count;
            eliminatedPlayers.Add(agent.PlayerID);
            agent.EliminateWithSplash();
            SetGameplayScore(agent.PlayerID, eliminationOrder);
        }

        protected override List<PlayerMatchResult> CollectFinalScores()
        {
            foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
            {
                if (!eliminatedPlayers.Contains(agent.PlayerID))
                {
                    SetGameplayScore(agent.PlayerID, survivorBonus);
                }
            }

            return base.CollectFinalScores();
        }
    }
}