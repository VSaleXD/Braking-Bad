using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// <summary>
    /// Survival arena with falling hazards and continuous survival scoring.
    /// </summary>
    public sealed class Minigame_ObstacleSurvival : BaseMinigameManager
    {
        [Header("Hazards")]
        [SerializeField] private GameObject[] hazardPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private float hazardLifetime = 8f;

        [Header("Scoring")]
        [SerializeField] private float survivalPointsPerTick = 5f;
        [SerializeField] private float survivalTickInterval = 1f;
        [SerializeField] private float collisionPenalty = 25f;
        [SerializeField] private bool eliminateOnCollision = true;

        private readonly HashSet<int> eliminatedPlayers = new HashSet<int>();
        private Coroutine hazardRoutine;
        private Coroutine survivalRoutine;

        protected override void OnMatchStarted()
        {
            eliminatedPlayers.Clear();
            hazardRoutine = StartCoroutine(HazardSpawnRoutine());
            survivalRoutine = StartCoroutine(SurvivalScoreRoutine());
        }

        protected override void OnMatchEnded()
        {
            if (hazardRoutine != null)
            {
                StopCoroutine(hazardRoutine);
                hazardRoutine = null;
            }

            if (survivalRoutine != null)
            {
                StopCoroutine(survivalRoutine);
                survivalRoutine = null;
            }
        }

        public void HandleHazardCollision(TournamentPlayerAgent agent)
        {
            if (agent == null || eliminatedPlayers.Contains(agent.PlayerID))
            {
                return;
            }

            AddGameplayScore(agent.PlayerID, -collisionPenalty, "Ouch!");

            if (eliminateOnCollision)
            {
                eliminatedPlayers.Add(agent.PlayerID);
                agent.SetEliminated(true);
            }
        }

        private IEnumerator HazardSpawnRoutine()
        {
            while (true)
            {
                SpawnHazard();
                yield return new WaitForSeconds(Mathf.Max(0.1f, spawnInterval));
            }
        }

        private IEnumerator SurvivalScoreRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Mathf.Max(0.1f, survivalTickInterval));

                foreach (TournamentPlayerAgent agent in GetRegisteredPlayers())
                {
                    if (!eliminatedPlayers.Contains(agent.PlayerID))
                    {
                        AddGameplayScore(agent.PlayerID, survivalPointsPerTick);
                    }
                }
            }
        }

        private void SpawnHazard()
        {
            if (hazardPrefabs == null || hazardPrefabs.Length == 0)
            {
                return;
            }

            GameObject prefab = hazardPrefabs[Random.Range(0, hazardPrefabs.Length)];
            if (prefab == null)
            {
                return;
            }

            Transform spawnPoint = spawnPoints != null && spawnPoints.Length > 0
                ? spawnPoints[Random.Range(0, spawnPoints.Length)]
                : transform;

            GameObject hazardInstance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            Destroy(hazardInstance, hazardLifetime);
        }
    }

    /// <summary>
    /// Put this on a hazard prefab so it can notify the survival manager on impact.
    /// </summary>
    public sealed class ObstacleHazard : MonoBehaviour
    {
        [SerializeField] private Minigame_ObstacleSurvival manager;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (manager == null)
            {
                return;
            }

            TournamentPlayerAgent agent = collision.collider.GetComponentInParent<TournamentPlayerAgent>();
            if (agent != null)
            {
                manager.HandleHazardCollision(agent);
            }
        }
    }
}