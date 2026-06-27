using UnityEngine;

namespace BrakingBad.Gameplay
{
    public sealed class PoliceCarHazard : MonoBehaviour
    {
        [SerializeField] private Minigame_ObstacleSurvival manager;
        [SerializeField] private float moveSpeed = 8f;

        private Rigidbody2D rb;
        private Vector2 moveDirection;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Minigame_ObstacleSurvival owner, Vector2 direction)
        {
            manager = owner;
            moveDirection = direction.normalized;

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void FixedUpdate()
        {
            if (rb != null)
            {
                rb.linearVelocity = moveDirection * moveSpeed;
            }
            else
            {
                transform.position += (Vector3)(moveDirection * moveSpeed * Time.fixedDeltaTime);
            }
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
                manager.HandleHazardCollision(agent);
            }
        }
    }
}