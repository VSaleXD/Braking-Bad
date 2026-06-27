using UnityEngine;

namespace BrakingBad.Gameplay
{
    /// Flag item that can be attached to a vehicle roof, dropped, or reset.
    public sealed class Item : MonoBehaviour
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
}