using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    public sealed class Minigame_ChaseTheUFO : BaseMinigameManager
    {
        [Header("Spawner")]
        [SerializeField] private GameObject ufoPrefab;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxActiveUfos = 3;

        [Header("Border")]
        [SerializeField] private PolygonCollider2D borderCollider;

        private readonly List<GameObject> activeUfos = new List<GameObject>();
        private float spawnAccumulator;

        protected override void Update()
        {
            base.Update();

            if (matchTimer <= 0f) return;

            spawnAccumulator += Time.deltaTime;
            if (spawnAccumulator >= spawnInterval)
            {
                spawnAccumulator = 0f;
                SpawnUfo();
            }

            activeUfos.RemoveAll(ufo => ufo == null);
        }

        public void RegisterUfoHit(TournamentPlayerAgent agent, float scoreAmount)
        {
            if (agent == null) return;

            string comboText = scoreAmount >= 800f ? "TINY UFO! BONUS!" :
                               scoreAmount >= 500f ? "SMALL UFO!" : "UFO HIT!";

            AddGameplayScore(agent.PlayerID, scoreAmount, comboText);
        }

        private void SpawnUfo()
        {
            if (ufoPrefab == null || activeUfos.Count >= maxActiveUfos) return;

            Vector3 spawnPos = GetSpawnPositionInsideBorder();
            GameObject ufoInstance = Instantiate(ufoPrefab, spawnPos, Quaternion.identity);
            activeUfos.Add(ufoInstance);

            UFO ufoScript = ufoInstance.GetComponent<UFO>();
            if (ufoScript != null)
            {
                ufoScript.Initialize(this);
            }
        }

        private Vector3 GetSpawnPositionInsideBorder()
        {
            // Fallback ke tengah scene jika border tidak di-assign
            if (borderCollider == null)
            {
                Debug.LogWarning("[ChaseTheUFO] borderCollider belum di-assign! UFO spawn di tengah.");
                return Vector3.zero;
            }

            Bounds bounds = borderCollider.bounds;

            // Coba random point di dalam bounds, maksimal 30 percobaan
            for (int i = 0; i < 30; i++)
            {
                Vector2 candidate = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );

                if (borderCollider.OverlapPoint(candidate))
                {
                    return new Vector3(candidate.x, candidate.y, 0f);
                }
            }

            return bounds.center;
        }
    }
}