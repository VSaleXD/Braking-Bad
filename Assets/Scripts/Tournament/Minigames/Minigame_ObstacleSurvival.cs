using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Survival arena dengan mobil polisi yang menyebrang dari tepi ke tepi
    /// berlawanan, dan continuous survival scoring.
    public sealed class Minigame_ObstacleSurvival : BaseMinigameManager
    {
        [Header("Hazards")]
        [SerializeField] private GameObject policeCarPrefab;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private float hazardLifetime = 8f;
        [SerializeField] private float hazardMoveSpeed = 8f;

        [Header("Arena")]
        [SerializeField] private CircleCollider2D arenaBoundary;

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

            AddGameplayScore(agent.PlayerID, -collisionPenalty, "BUSTED!");

            if (eliminateOnCollision)
            {
                eliminatedPlayers.Add(agent.PlayerID);
                agent.EliminateWithSplash();
            }
        }

        private IEnumerator HazardSpawnRoutine()
        {
            while (true)
            {
                SpawnPoliceCar();
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

        private void SpawnPoliceCar()
        {
            if (policeCarPrefab == null || arenaBoundary == null)
            {
                Debug.LogWarning("[ObstacleSurvival] policeCarPrefab atau arenaBoundary belum di-assign.");
                return;
            }

            Vector2 center = arenaBoundary.transform.position;
            float radius = arenaBoundary.radius * arenaBoundary.transform.lossyScale.x;

            float randomAngle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 spawnDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

            Vector2 spawnPos = center + spawnDirection * (radius + 1.5f);

            Vector2 moveDirection = (center - spawnDirection * radius) - spawnPos;

            GameObject hazardInstance = Instantiate(policeCarPrefab, spawnPos, Quaternion.identity);

            PoliceCarHazard hazardScript = hazardInstance.GetComponent<PoliceCarHazard>();
            if (hazardScript != null)
            {
                hazardScript.Initialize(this, moveDirection);
            }

            Destroy(hazardInstance, hazardLifetime);
        }
    }
}