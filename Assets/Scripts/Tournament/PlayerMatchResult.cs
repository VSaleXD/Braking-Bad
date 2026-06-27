using System;

namespace BrakingBad.Gameplay
{
    /// Snapshot hasil performa seorang pemain pada satu minigame.
    [Serializable]
    public sealed class PlayerMatchResult
    {
        public int playerID;
        public float gameplayScore;

        public PlayerMatchResult(int playerID, float gameplayScore)
        {
            this.playerID = playerID;
            this.gameplayScore = gameplayScore;
        }
    }
}