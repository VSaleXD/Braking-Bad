using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{

    public sealed class Minigame_PoliceChase : BaseMinigameManager
    {
        [Header("Hazards")]
        [SerializeField] private GameObject policeCarPrefab;
        [SerializeField] private float spawnInterval = 2f;

        [Header("Highway Lanes")]
        [Tooltip("Titik-titik di tepi KIRI jalan, satu per lane.")]
        [SerializeField] private Transform[] leftLaneSpawnPoints;
        [Tooltip("Titik-titik di tepi KANAN jalan, satu per lane.")]
        [SerializeField] private Transform[] rightLaneSpawnPoints;

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
            if (policeCarPrefab == null)
            {
                Debug.LogWarning("[PoliceChase] policeCarPrefab belum di-assign.");
                return;
            }

            bool hasLeftLanes = leftLaneSpawnPoints != null && leftLaneSpawnPoints.Length > 0;
            bool hasRightLanes = rightLaneSpawnPoints != null && rightLaneSpawnPoints.Length > 0;

            if (!hasLeftLanes && !hasRightLanes)
            {
                Debug.LogWarning("[PoliceChase] leftLaneSpawnPoints dan rightLaneSpawnPoints belum di-assign.");
                return;
            }

            bool spawnFromLeft = hasLeftLanes && (!hasRightLanes || Random.value > 0.5f);

            Transform spawnPoint = spawnFromLeft
                ? leftLaneSpawnPoints[Random.Range(0, leftLaneSpawnPoints.Length)]
                : rightLaneSpawnPoints[Random.Range(0, rightLaneSpawnPoints.Length)];

            if (spawnPoint == null)
            {
                return;
            }

            Vector2 spawnPos = spawnPoint.position;

            Vector2 moveDirection = spawnFromLeft ? Vector2.right : Vector2.left;

            GameObject hazardInstance = Instantiate(policeCarPrefab, spawnPos, Quaternion.identity);

            PoliceCarHazard hazardScript = hazardInstance.GetComponent<PoliceCarHazard>();
            if (hazardScript != null)
            {
                hazardScript.Initialize(this, moveDirection);
            }
        }
    }
}