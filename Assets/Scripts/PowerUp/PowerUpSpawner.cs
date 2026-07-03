using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    public sealed class PowerUpSpawner : MonoBehaviour
    {
        [System.Serializable]
        public sealed class WeightedPowerUpPrefab
        {
            public GameObject prefab;
            [Min(0f)] public float weight = 1f;
        }

        [SerializeField] private List<WeightedPowerUpPrefab> powerUpPrefabs = new List<WeightedPowerUpPrefab>();
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
        [SerializeField] private float spawnInterval = 8f;
        [SerializeField] private int maxActivePowerUps = 2;
        [SerializeField] private bool autoStart = true;

        private readonly List<GameObject> activePowerUps = new List<GameObject>();
        private float spawnAccumulator;
        private bool isRunning;

        private void Start()
        {
            if (autoStart) isRunning = true;
        }

        public void StartSpawning() => isRunning = true;
        public void StopSpawning() => isRunning = false;

        private void Update()
        {
            if (!isRunning) return;

            activePowerUps.RemoveAll(item => item == null);

            spawnAccumulator += Time.deltaTime;
            if (spawnAccumulator >= spawnInterval)
            {
                spawnAccumulator = 0f;
                TrySpawn();
            }
        }

        private void TrySpawn()
        {
            if (activePowerUps.Count >= maxActivePowerUps) return;
            if (powerUpPrefabs.Count == 0 || spawnPoints.Count == 0) return;

            GameObject prefab = PickWeightedPrefab();
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
            if (prefab == null || point == null) return;

            activePowerUps.Add(Instantiate(prefab, point.position, Quaternion.identity));
        }

        private GameObject PickWeightedPrefab()
        {
            float totalWeight = 0f;
            foreach (var entry in powerUpPrefabs)
            {
                if (entry.prefab != null) totalWeight += entry.weight;
            }

            if (totalWeight <= 0f) return null;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var entry in powerUpPrefabs)
            {
                if (entry.prefab == null) continue;
                cumulative += entry.weight;
                if (roll <= cumulative) return entry.prefab;
            }

            return null;
        }
    }
}