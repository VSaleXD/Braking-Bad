using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    public class Minigame_CarMaze : BaseMinigameManager
    {
        [SerializeField] private mazeGenerator generator;
        [SerializeField] private float cellSize = 2f;

        private Dictionary<int, int> finishOrder = new(); // playerID -> rank
        private Dictionary<Vector2Int, int> distanceToExit;
        private int nextRank = 1;

        protected override void OnMatchStarted()
        {
            finishOrder.Clear();
            nextRank = 1;

            generator.Generate(seed: System.DateTime.Now.Millisecond);

            distanceToExit = generator.BFSDistances(generator.ExitCell);

            SpawnPlayersAtStart();
        }

        private void SpawnPlayersAtStart()
        {
            var startPos = CellToWorld(generator.StartCell);
            int i = 0;
            foreach (var player in GetRegisteredPlayers())
            {
                Vector3 offset = new Vector3((i % 2) * 0.5f, (i / 2) * 0.5f, 0);
                player.transform.position = startPos + offset;
                i++;
            }
        }

        public void OnPlayerReachExit(int playerID)
        {
            if (finishOrder.ContainsKey(playerID)) return;

            finishOrder[playerID] = nextRank++;

            int totalPlayers = GetRegisteredPlayers().Count();
            float points = (totalPlayers - finishOrder[playerID] + 1) * 100f;
            SetGameplayScore(playerID, points);

            ShowComboMessage($"P{playerID} finished #{finishOrder[playerID]}!");

            if (finishOrder.Count >= totalPlayers)
                CompleteMatch();
        }

        protected override List<PlayerMatchResult> CollectFinalScores()
        {
            foreach (var player in GetRegisteredPlayers())
            {
                if (finishOrder.ContainsKey(player.PlayerID)) continue;

                Vector2Int cell = WorldToCell(player.transform.position);
                int distLeft = distanceToExit.TryGetValue(cell, out int d) ? d : int.MaxValue;

                float progressScore = Mathf.Max(0f, 100f - distLeft * 5f);
                SetGameplayScore(player.PlayerID, progressScore);
            }

            return base.CollectFinalScores();
        }

        private Vector3 CellToWorld(Vector2Int cell) => new Vector3(cell.x * cellSize, cell.y * cellSize, 0);
        private Vector2Int WorldToCell(Vector3 world) =>
            new Vector2Int(Mathf.RoundToInt(world.x / cellSize), Mathf.RoundToInt(world.y / cellSize));
    }
}