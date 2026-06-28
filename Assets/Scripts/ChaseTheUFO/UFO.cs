using UnityEngine;
using Random = UnityEngine.Random;

namespace BrakingBad.Gameplay
{
    public class UFO : MonoBehaviour
    {
        [Header("Size & Speed")]
        public float minSize = 0.5f;
        public float maxSize = 2.5f;
        public float minSpeed = 500f;
        public float maxSpeed = 1000f;
        public float maxSpinSpeed = 10f;

        [Header("Scoring")]
        public float baseScoreValue = 500f;

        [Header("Effects")]
        public GameObject explosionEffect;
        [SerializeField] public bool isDestroyed = false;

        [Header("Hit Sound")]
        [SerializeField] private AudioClip hitSound;
        [SerializeField, Range(0f, 1f)] private float hitVolume = 0.6f;

        private Minigame_ChaseTheUFO manager;
        private Rigidbody2D rb;
        private float currentSize;

        public void Initialize(Minigame_ChaseTheUFO owner)
        {
            manager = owner;
        }

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            currentSize = Random.Range(minSize, maxSize);
            transform.localScale = new Vector3(currentSize, currentSize, 1f);

            float speed = Random.Range(minSpeed, maxSpeed) / currentSize;
            Vector2 direction = Random.insideUnitCircle.normalized;
            rb.AddForce(direction * speed);
            rb.AddTorque(Random.Range(-maxSpinSpeed, maxSpinSpeed));

            ApplyRandomColor();
        }

        void ApplyRandomColor()
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length == 0) return;

            Color randomColor = Random.ColorHSV(
                0f, 1f,
                0.7f, 1f,
                0.8f, 1f
            );

            foreach (SpriteRenderer sr in renderers)
            {
                sr.color = randomColor;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDestroyed) return;

            TournamentPlayerAgent agent = collision.collider.GetComponentInParent<TournamentPlayerAgent>();
            if (agent == null) return;

            isDestroyed = true;

            float scoreForHit = Mathf.Round(baseScoreValue / currentSize);

            if (manager != null)
            {
                manager.RegisterUfoHit(agent, scoreForHit);
            }

            // ── Hit Sound ─────────────────────────────────────────────────
            // Pakai AudioSource.PlayClipAtPoint agar sound tetap bunyi
            // meskipun GameObject langsung di-Destroy
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
            }
            // ─────────────────────────────────────────────────────────────

            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}