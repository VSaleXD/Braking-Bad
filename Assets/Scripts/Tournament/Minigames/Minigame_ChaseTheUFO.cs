using System.Collections.Generic;
using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// <summary>
    /// UFO chase mode: the manager spawns UFOs and awards points whenever players ram them.
    /// </summary>
    public sealed class Minigame_ChaseTheUFO : BaseMinigameManager
    {
        [Header("Spawner")]
        [SerializeField] private GameObject ufoPrefab;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxActiveUfos = 3;
        [SerializeField] private float minLaunchForce = 500f;
        [SerializeField] private float maxLaunchForce = 1000f;

        [Header("Scoring")]
        [SerializeField] private float hitReward = 100f;

        private readonly List<GameObject> activeUfos = new List<GameObject>();
        private float spawnAccumulator;

        protected override void Update()
        {
            base.Update();

            if (matchTimer <= 0f)
            {
                return;
            }

            spawnAccumulator += Time.deltaTime;
            if (spawnAccumulator >= spawnInterval)
            {
                spawnAccumulator = 0f;
                SpawnUfo();
            }

            activeUfos.RemoveAll(ufo => ufo == null);
        }

        public void RegisterUfoHit(TournamentPlayerAgent agent)
        {
            if (agent == null)
            {
                return;
            }

            AddGameplayScore(agent.PlayerID, hitReward, "UFO hit!");
        }

        private void SpawnUfo()
        {
            if (ufoPrefab == null || activeUfos.Count >= maxActiveUfos)
            {
                return;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            Vector3 viewportEdge = GetRandomEdgeSpawnPosition(camera);
            GameObject ufoInstance = Instantiate(ufoPrefab, viewportEdge, Quaternion.identity);
            activeUfos.Add(ufoInstance);

            Rigidbody2D rigidbody2D = ufoInstance.GetComponent<Rigidbody2D>();
            if (rigidbody2D != null)
            {
                Vector2 launchDirection = Random.insideUnitCircle.normalized;
                float launchForce = Random.Range(minLaunchForce, maxLaunchForce);
                rigidbody2D.AddForce(launchDirection * launchForce);
                rigidbody2D.AddTorque(Random.Range(-10f, 10f));
            }

            ChaseTheUFOActor actor = ufoInstance.GetComponent<ChaseTheUFOActor>();
            if (actor != null)
            {
                actor.Initialize(this);
            }
        }

        private Vector3 GetRandomEdgeSpawnPosition(Camera camera)
        {
            Vector3 cameraPosition = camera.transform.position;
            Vector2 worldBounds = camera.ViewportToWorldPoint(new Vector3(1f, 1f, Mathf.Abs(cameraPosition.z)));

            bool spawnFromHorizontalEdge = Random.value > 0.5f;
            Vector3 spawnPosition = Vector3.zero;

            if (spawnFromHorizontalEdge)
            {
                spawnPosition.x = Random.value > 0.5f ? worldBounds.x + 1f : -worldBounds.x - 1f;
                spawnPosition.y = Random.Range(-worldBounds.y, worldBounds.y);
            }
            else
            {
                spawnPosition.x = Random.Range(-worldBounds.x, worldBounds.x);
                spawnPosition.y = Random.value > 0.5f ? worldBounds.y + 1f : -worldBounds.y - 1f;
            }

            return spawnPosition;
        }
    }

    /// <summary>
    /// UFO behaviour relay. Add this to the UFO prefab so collisions score the hitting player.
    /// </summary>
    public sealed class ChaseTheUFOActor : MonoBehaviour
    {
        private Minigame_ChaseTheUFO manager;

        public void Initialize(Minigame_ChaseTheUFO owner)
        {
            manager = owner;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (manager == null)
            {
                return;
            }

            TournamentPlayerAgent agent = collision.collider.GetComponentInParent<TournamentPlayerAgent>();
            if (agent != null)
            {
                manager.RegisterUfoHit(agent);
            }
        }
    }
}