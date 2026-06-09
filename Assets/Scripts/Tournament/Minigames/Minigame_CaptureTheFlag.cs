using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// <summary>
    /// Central flag capture mode with pick up, carry, drop, and delivery states.
    /// </summary>
    public sealed class Minigame_CaptureTheFlag : BaseMinigameManager
    {
        [SerializeField] private float captureBonus = 200f;
        [SerializeField] private Transform flagResetPoint;
        [SerializeField] private GameObject flagPrefab;
        
        private CaptureTheFlagItem activeFlag;

        protected override void OnMatchStarted()
        {
            if (flagPrefab != null && activeFlag == null)
            {
                GameObject flagInstance = Instantiate(flagPrefab, flagResetPoint != null ? flagResetPoint.position : Vector3.zero, Quaternion.identity);
                activeFlag = flagInstance.GetComponent<CaptureTheFlagItem>();

                if (activeFlag != null)
                {
                    activeFlag.Initialize(this);
                }
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
            if (carrier == null || activeFlag == null)
            {
                return;
            }

            AddGameplayScore(carrier.PlayerID, captureBonus, "Flag captured!");
            activeFlag.ResetFlag(flagResetPoint != null ? flagResetPoint.position : Vector3.zero);
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
    }

    /// <summary>
    /// Flag item that can be attached to a vehicle roof, dropped, or reset.
    /// </summary>
    public sealed class CaptureTheFlagItem : MonoBehaviour
    {
        [SerializeField] private Vector3 carryLocalOffset = new Vector3(0f, 1.15f, 0f);
        [SerializeField] private float dropForwardOffset = 0.5f;
        
        // PERBAIKAN 1: Mengubah nama variabel menjadi rb2D agar tidak konflik dengan properti bawaan Unity
        [SerializeField] private Rigidbody2D rb2D;

        private Minigame_CaptureTheFlag manager;
        private Transform originalParent;
        private Vector3 originalLocalPosition;

        public TournamentPlayerAgent Carrier { get; private set; }
        public bool IsHeld => Carrier != null;

        private void Awake()
        {
            originalParent = transform.parent;
            originalLocalPosition = transform.localPosition;

            if (rb2D == null)
            {
                rb2D = GetComponent<Rigidbody2D>();
            }
        }

        public void Initialize(Minigame_CaptureTheFlag owner)
        {
            manager = owner;
        }

        public void Pickup(TournamentPlayerAgent carrier)
        {
            Carrier = carrier;

            if (rb2D != null)
            {
                rb2D.linearVelocity = Vector2.zero;
                rb2D.angularVelocity = 0f;
                rb2D.simulated = false;
            }

            transform.SetParent(carrier.transform, worldPositionStays: false);
            transform.localPosition = carryLocalOffset;
            transform.localRotation = Quaternion.identity;
        }

        public void Drop(Vector3 worldPosition)
        {
            if (Carrier == null)
            {
                return;
            }

            Transform carrierTransform = Carrier.transform;
            Carrier = null;

            transform.SetParent(null, worldPositionStays: true);
            transform.position = worldPosition + carrierTransform.up * dropForwardOffset;

            if (rb2D != null)
            {
                rb2D.simulated = true;
                rb2D.linearVelocity = carrierTransform.GetComponent<Rigidbody2D>() != null
                    ? carrierTransform.GetComponent<Rigidbody2D>().linearVelocity
                    : Vector2.zero;
            }
        }

        public void ResetFlag(Vector3 worldPosition)
        {
            Carrier = null;
            transform.SetParent(originalParent, worldPositionStays: true);
            transform.localPosition = originalLocalPosition;
            transform.position = worldPosition;

            if (rb2D != null)
            {
                rb2D.simulated = true;
                rb2D.linearVelocity = Vector2.zero;
                rb2D.angularVelocity = 0f;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision == null || collision.collider == null) return;

            // PERBAIKAN 2: Proteksi ekstra pengecekan null untuk mencegah bug NullReferenceException
            if (Carrier == null)
            {
                TournamentPlayerAgent freePickupAgent = collision.collider.GetComponentInParent<TournamentPlayerAgent>();
                if (freePickupAgent != null && manager != null)
                {
                    manager.TryPickupFlag(freePickupAgent);
                }
                return;
            }

            TournamentPlayerAgent attacker = collision.collider.GetComponentInParent<TournamentPlayerAgent>();
            if (attacker != null && Carrier != null && attacker.PlayerID != Carrier.PlayerID && manager != null)
            {
                manager.NotifyFlagCarrierHit(attacker);
            }
        }
    }

    /// <summary>
    /// Put one of these on each team base trigger.
    /// </summary>
    public sealed class CaptureTheFlagBaseTrigger : MonoBehaviour
    {
        [SerializeField] private Minigame_CaptureTheFlag manager;
        [SerializeField] private int teamIndex;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (manager == null || other == null)
            {
                return;
            }

            TournamentPlayerAgent agent = other.GetComponentInParent<TournamentPlayerAgent>();
            if (agent == null)
            {
                return;
            }

            if (agent.TeamIndex == teamIndex)
            {
                manager.CaptureFlag(agent);
            }
        }
    }
}