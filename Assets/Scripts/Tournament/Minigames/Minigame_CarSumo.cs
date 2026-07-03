using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// <summary>
    /// Circle arena elimination mode.
    /// Scoring: pemain yang jatuh lebih awal dapat skor lebih rendah.
    /// Match berakhir lebih awal kalau sudah hanya 1 pemain tersisa.
    /// </summary>
    public sealed class Minigame_CarSumo : BaseMinigameManager
    {
        [SerializeField] private float survivorBonus = 3f;

        private static readonly string[] eliminationMessages =
        {
            "OUT! 💥",
            "OUT! 💥",
            "OUT! 💥",
            "WINNER! 🏆"
        };

        private readonly HashSet<int> eliminatedPlayers = new HashSet<int>();

        protected override void OnMatchStarted()
        {
            eliminatedPlayers.Clear();

            foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
            {
                agent.SetEliminated(false);
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
            agent.SetEliminated(true);

            SetGameplayScore(agent.PlayerID, eliminationOrder);
            ShowComboMessage($"P{agent.PlayerID} OUT!");

            CheckForEarlyEnd();
        }

        private void CheckForEarlyEnd()
        {
            List<TournamentPlayerAgent> allPlayers = GetRegisteredPlayers().ToList();
            List<TournamentPlayerAgent> activePlayers = allPlayers
                .Where(player => player != null && !eliminatedPlayers.Contains(player.PlayerID))
                .ToList();

            if (activePlayers.Count > 1)
            {
                return;
            }

            if (activePlayers.Count == 1)
            {
                TournamentPlayerAgent winner = activePlayers[0];
                SetGameplayScore(winner.PlayerID, survivorBonus);
                ShowComboMessage($"P{winner.PlayerID} WINNER! 🏆");
            }

            CompleteMatch();
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