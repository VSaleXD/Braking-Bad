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
        // Skor base yang akan dibagi dengan ukuran UFO
        // UFO kecil (0.5) → skor tinggi, UFO besar (2.5) → skor rendah
        public float baseScoreValue = 500f;

        [Header("Effects")]
        public GameObject explosionEffect;

        [SerializeField] public bool isDestroyed = false;

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

            // --- Ukuran acak ---
            currentSize = Random.Range(minSize, maxSize);
            transform.localScale = new Vector3(currentSize, currentSize, 1f);

            // --- Kecepatan berbanding terbalik dengan ukuran ---
            // UFO kecil = cepat, UFO besar = lambat
            float speed = Random.Range(minSpeed, maxSpeed) / currentSize;
            Vector2 direction = Random.insideUnitCircle.normalized;
            rb.AddForce(direction * speed);
            rb.AddTorque(Random.Range(-maxSpinSpeed, maxSpinSpeed));

            // --- Warna acak ---
            ApplyRandomColor();
        }

        void ApplyRandomColor()
        {
            // Cari semua SpriteRenderer termasuk child (untuk UFO yang terdiri dari beberapa part)
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length == 0) return;

            // Buat satu warna HSV yang vivid supaya tidak gelap/kusam
            Color randomColor = Random.ColorHSV(
                0f, 1f,     // hue: semua warna
                0.7f, 1f,   // saturation: vivid
                0.8f, 1f    // value: terang
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

            // --- Hitung skor berbasis ukuran ---
            // Makin kecil UFO → skor makin tinggi
            // Contoh: size 0.5 → 500/0.5 = 1000 poin
            //         size 2.5 → 500/2.5 = 200 poin
            float scoreForHit = Mathf.Round(baseScoreValue / currentSize);

            if (manager != null)
            {
                manager.RegisterUfoHit(agent, scoreForHit);
            }

            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}