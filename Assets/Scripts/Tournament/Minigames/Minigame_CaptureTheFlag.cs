using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Free-for-all flag capture: 1 flag spawn random dalam area border (exclude
    /// radius base netral), base netral di tengah menerima capture dari siapapun,
    /// flag respawn random lagi setiap kali ter-capture.
    public sealed class Minigame_CaptureTheFlag : BaseMinigameManager
    {
        [Header("Flag")]
        [SerializeField] private float captureBonus = 200f;
        [SerializeField] private GameObject flagPrefab;

        [Header("Spawn Area")]
        [SerializeField] private PolygonCollider2D borderCollider;
        [SerializeField] private Transform baseCenter;
        [SerializeField] private float baseExclusionRadius = 3f;

        private Item activeFlag;

        protected override void OnMatchStarted()
        {
            if (flagPrefab != null && activeFlag == null)
            {
                SpawnFlagAtRandomPosition();
            }
        }

        public bool TryPickupFlag(TournamentPlayerAgent agent)
        {
            if (agent == null || activeFlag == null || activeFlag.IsHeld)
            {
                return false;
            }

            activeFlag.Pickup(agent);
            ShowComboMessage($"P{agent.PlayerID} picked up the flag");
            return true;
        }

        public void DropFlag(Vector3 worldPosition)
        {
            if (activeFlag == null)
            {
                return;
            }

            activeFlag.Drop(worldPosition);
        }

        public void CaptureFlag(TournamentPlayerAgent carrier)
        {
            if (carrier == null || activeFlag == null || activeFlag.Carrier != carrier)
            {
                return;
            }

            AddGameplayScore(carrier.PlayerID, captureBonus, $"P{carrier.PlayerID} captured the flag!");

            RespawnFlagAtRandomPosition();
        }

        public void NotifyFlagCarrierHit(TournamentPlayerAgent attacker)
        {
            if (activeFlag == null || !activeFlag.IsHeld)
            {
                return;
            }

            if (attacker != null && activeFlag.Carrier != null && attacker.PlayerID != activeFlag.Carrier.PlayerID)
            {
                DropFlag(activeFlag.transform.position);
            }
        }

        private void SpawnFlagAtRandomPosition()
        {
            Vector3 spawnPos = GetSpawnPositionInsideBorder();
            GameObject flagInstance = Instantiate(flagPrefab, spawnPos, Quaternion.identity);
            activeFlag = flagInstance.GetComponent<Item>();

            if (activeFlag != null)
            {
                activeFlag.Initialize(this);
            }
        }

        private void RespawnFlagAtRandomPosition()
        {
            if (activeFlag == null)
            {
                return;
            }

            Vector3 spawnPos = GetSpawnPositionInsideBorder();
            activeFlag.ResetFlag(spawnPos);
        }

        private Vector3 GetSpawnPositionInsideBorder()
        {
            if (borderCollider == null)
            {
                Debug.LogWarning("[CaptureTheFlag] borderCollider belum di-assign! Flag spawn di tengah.");
                return Vector3.zero;
            }

            Bounds bounds = borderCollider.bounds;

            for (int i = 0; i < 30; i++)
            {
                Vector2 candidate = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );

                bool insideBorder = borderCollider.OverlapPoint(candidate);
                bool outsideBaseExclusion = IsOutsideBaseExclusion(candidate);

                if (insideBorder && outsideBaseExclusion)
                {
                    return new Vector3(candidate.x, candidate.y, 0f);
                }
            }

            return GetFallbackPositionAwayFromBase(bounds);
        }

        private bool IsOutsideBaseExclusion(Vector2 candidate)
        {
            if (baseCenter == null)
            {
                return true;
            }

            float distanceFromBase = Vector2.Distance(candidate, baseCenter.position);
            return distanceFromBase >= baseExclusionRadius;
        }

        private Vector3 GetFallbackPositionAwayFromBase(Bounds bounds)
        {
            if (baseCenter == null)
            {
                return bounds.center;
            }

            Vector2 directionFromCenter = ((Vector2)bounds.center - (Vector2)baseCenter.position).normalized;
            if (directionFromCenter.sqrMagnitude < 0.01f)
            {
                directionFromCenter = Vector2.right;
            }

            float fallbackDistance = baseExclusionRadius + 1f;
            return (Vector2)baseCenter.position + directionFromCenter * fallbackDistance;
        }
    }
}