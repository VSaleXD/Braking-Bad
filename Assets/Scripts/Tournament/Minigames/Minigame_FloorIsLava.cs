using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    public sealed class Minigame_FloorIsLava : BaseMinigameManager
    {
        [SerializeField] private float survivalPointsPerSecond = 2f;
        [SerializeField] private float tileTriggerBonus = 5f;

        private readonly HashSet<int> fallenPlayers = new HashSet<int>();
        private bool matchEnded = false;
        private Coroutine survivalRoutine;

        protected override void OnMatchStarted()
        {
            fallenPlayers.Clear();
            matchEnded = false;
            survivalRoutine = StartCoroutine(SurvivalScoreRoutine());
        }

        protected override void OnMatchEnded()
        {
            matchEnded = true;
            if (survivalRoutine != null)
            {
                StopCoroutine(survivalRoutine);
                survivalRoutine = null;
            }
        }

        public void RegisterTileTrigger(Tile tile, TournamentPlayerAgent agent)
        {
            if (tile == null || agent == null) return;

            if (!fallenPlayers.Contains(agent.PlayerID))
            {
                AddGameplayScore(agent.PlayerID, tileTriggerBonus, "");
            }
        }

        public void RegisterFallenPlayer(TournamentPlayerAgent agent)
        {
            if (agent == null || fallenPlayers.Contains(agent.PlayerID) || matchEnded)
                return;

            fallenPlayers.Add(agent.PlayerID);
            agent.EliminateWithSplash();

            CheckSurvivorCount();
        }

        private void CheckSurvivorCount()
        {
            if (matchEnded) return;

            var allPlayers = GetRegisteredPlayers();
            int survivorCount = 0;
            TournamentPlayerAgent lastSurvivor = null;

            foreach (TournamentPlayerAgent agent in allPlayers)
            {
                if (!fallenPlayers.Contains(agent.PlayerID))
                {
                    survivorCount++;
                    lastSurvivor = agent;
                }
            }

            if (survivorCount <= 1)
            {
                if (lastSurvivor != null)
                {
                    AddGameplayScore(lastSurvivor.PlayerID, 10f, "Last Survivor Bonus");
                }

                CompleteMatch();
            }
        }

        private IEnumerator SurvivalScoreRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (matchEnded) yield break;

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