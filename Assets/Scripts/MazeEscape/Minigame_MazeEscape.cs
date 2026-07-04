using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    public class Minigame_MazeEscape : BaseMinigameManager
    {
        private int currentEscapeRank = 1;
        private HashSet<int> escapedPlayers = new HashSet<int>();

        protected override void OnMatchStarted()
        {
            base.OnMatchStarted();
            currentEscapeRank = 1;
            escapedPlayers.Clear();
        }

        public void OnPlayerEscaped(TournamentPlayerAgent player)
        {
            if (player == null || escapedPlayers.Contains(player.PlayerID) || player.IsEliminated)
            {
                return;
            }

            escapedPlayers.Add(player.PlayerID);

            // Assign score based on escape rank
            int scoreToAward = 0;
            switch (currentEscapeRank)
            {
                case 1: scoreToAward = 1000; break;
                case 2: scoreToAward = 750; break;
                case 3: scoreToAward = 500; break;
                case 4: scoreToAward = 250; break;
                default: scoreToAward = 0; break; // Should not happen with 4 players max
            }

            SetGameplayScore(player.PlayerID, scoreToAward);
            currentEscapeRank++;

            // Visual and physical elimination so they don't keep moving
            player.EliminateWithSplash();

            // Check if all active players have escaped
            if (escapedPlayers.Count >= TournamentManager.Instance.ActivePlayerCount)
            {
                // End the match early
                matchTimer = 0f;
            }
        }
    }
}
