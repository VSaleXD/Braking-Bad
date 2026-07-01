using UnityEngine;
using System.Linq;

namespace BrakingBad.Gameplay
{
    public sealed class PoliceCarHazard : MonoBehaviour
    {
        [SerializeField] private Minigame_PoliceChase manager;
        [SerializeField] private float moveSpeed = 8f;
        
        [Header("Homing")]
        [Tooltip("Seberapa cepat mobil membelokkan arah menuju player terdekat. Semakin besar, semakin agresif ngejar.")]
        [SerializeField] private float turnSpeed = 180f;

        [Header("Lifetime & Effects")]
        [SerializeField] private float lifetime = 8f;
        [SerializeField] private GameObject explosionEffect;

        private Rigidbody2D rb;
        private Vector2 moveDirection;
        private float lifeTimer;
        private bool hasExploded;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Minigame_PoliceChase owner, Vector2 direction)
        {
            manager = owner;
            moveDirection = direction.normalized;
            lifeTimer = lifetime;
            hasExploded = false;

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            if (hasExploded)
            {
                return;
            }

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                Explode();
            }
        }

        private void FixedUpdate()
        {
            if (hasExploded)
            {
                return;
            }

            UpdateHomingDirection();

            if (rb != null)
            {
                rb.linearVelocity = moveDirection * moveSpeed;
            }
            else
            {
                transform.position += (Vector3)(moveDirection * moveSpeed * Time.fixedDeltaTime);
            }
        }

        private void UpdateHomingDirection()
        {
            TournamentPlayerAgent target = FindNearestPlayer();
            if (target == null)
            {
                return;
            }

            Vector2 toTarget = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;

            float currentAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);

            moveDirection = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));
            transform.rotation = Quaternion.Euler(0f, 0f, newAngle - 90f);
        }

        private TournamentPlayerAgent FindNearestPlayer()
        {
            TournamentPlayerAgent[] agents = FindObjectsByType<TournamentPlayerAgent>(FindObjectsSortMode.None);
            if (agents == null || agents.Length == 0)
            {
                return null;
            }

            return agents
                .Where(agent => agent != null && !agent.IsEliminated)
                .OrderBy(agent => ((Vector2)agent.transform.position - (Vector2)transform.position).sqrMagnitude)
                .FirstOrDefault();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (hasExploded)
            {
                return;
            }

            TournamentPlayerAgent agent = collision.collider.GetComponentInParent<TournamentPlayerAgent>();
            if (agent != null && manager != null)
            {
                manager.HandleHazardCollision(agent);
            }
            Explode();
        }

        private void Explode()
        {
            if (hasExploded)
            {
                return;
            }

            hasExploded = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}