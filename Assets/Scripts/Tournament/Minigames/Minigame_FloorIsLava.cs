using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// <summary>
    /// Tile-collapse survival mode with crack feedback and time-based scoring.
    /// </summary>
    public sealed class Minigame_FloorIsLava : BaseMinigameManager
    {
        [SerializeField] private float survivalPointsPerSecond = 2f;
        [SerializeField] private float tileTriggerBonus = 5f;

        private readonly HashSet<int> fallenPlayers = new HashSet<int>();
        private Coroutine survivalRoutine;

        protected override void OnMatchStarted()
        {
            fallenPlayers.Clear();
            survivalRoutine = StartCoroutine(SurvivalScoreRoutine());
        }

        protected override void OnMatchEnded()
        {
            if (survivalRoutine != null)
            {
                StopCoroutine(survivalRoutine);
                survivalRoutine = null;
            }
        }

        public void RegisterTileTrigger(Tile tile, TournamentPlayerAgent agent)
        {
            if (tile == null || agent == null)
            {
                return;
            }

            if (!fallenPlayers.Contains(agent.PlayerID))
            {
                AddGameplayScore(agent.PlayerID, tileTriggerBonus, "");
            }
        }

        public void RegisterFallenPlayer(TournamentPlayerAgent agent)
        {
            if (agent == null || fallenPlayers.Contains(agent.PlayerID))
            {
                return;
            }

            fallenPlayers.Add(agent.PlayerID);
            agent.EliminateWithSplash();
        }

        private IEnumerator SurvivalScoreRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
                {
                    if (!fallenPlayers.Contains(agent.PlayerID))
                    {
                        AddGameplayScore(agent.PlayerID, survivalPointsPerSecond);
                    }
                }
            }
        }
    }
}