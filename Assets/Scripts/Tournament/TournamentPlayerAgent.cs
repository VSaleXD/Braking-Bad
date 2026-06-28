using UnityEngine;

namespace BrakingBad.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class TournamentPlayerAgent : MonoBehaviour
    {
        [SerializeField, Range(1, 4)] private int playerID = 1;
        [SerializeField] private int teamIndex = 0;
        [SerializeField] private Rigidbody2D cachedRigidbody;
        [SerializeField] private bool isEliminated;

        [Header("Control Modifiers")]
        public float steeringMultiplier = 1f;
        public float throttleMultiplier = 1f;

        [Header("Elimination Effect")]
        [SerializeField] private GameObject splashEffectPrefab;

        public int PlayerID => playerID;
        public int TeamIndex => teamIndex;
        public Rigidbody2D CachedRigidbody => cachedRigidbody;
        public bool IsEliminated => isEliminated;
        public float SteeringMultiplier => steeringMultiplier;
        public float ThrottleMultiplier => throttleMultiplier;

        private void Awake()
        {
            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody2D>();
            }

            if (TournamentManager.Instance != null && playerID > TournamentManager.Instance.ActivePlayerCount)
            {
                gameObject.SetActive(false);
                return;
            }
        }

        private void OnValidate()
        {
            playerID = Mathf.Clamp(playerID, 1, 4);
            teamIndex = Mathf.Max(0, teamIndex);

            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody2D>();
            }
        }

        public void SetEliminated(bool eliminated)
        {
            isEliminated = eliminated;
        }
        public void EliminateWithSplash()
        {
            SetEliminated(true);

            if (splashEffectPrefab != null)
            {
                Instantiate(splashEffectPrefab, transform.position, Quaternion.identity);
            }

            playerController controller = GetComponent<playerController>();
            if (controller != null)
            {
                controller.movementEnabled = false;
            }

            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = Vector2.zero;
                cachedRigidbody.angularVelocity = 0f;
                cachedRigidbody.simulated = false;
            }

            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
public void ResetForNewMatch()
{
    SetEliminated(false);

    playerController controller = GetComponent<playerController>();
    if (controller != null)
    {
        controller.movementEnabled = true;
    }

    if (cachedRigidbody != null)
    {
        cachedRigidbody.simulated = true;
    }

    SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
    foreach (SpriteRenderer renderer in renderers)
    {
        renderer.enabled = true;
    }
}
    

        public void SetControlMultipliers(float steering, float throttle)
        {
            steeringMultiplier = steering;
            throttleMultiplier = throttle;
        }

        public void Teleport(Vector2 worldPosition)
        {
            if (cachedRigidbody != null)
            {
                cachedRigidbody.position = worldPosition;
                cachedRigidbody.linearVelocity = cachedRigidbody.linearVelocity;
            }

            transform.position = worldPosition;
            Physics2D.SyncTransforms();
        }

        public void AddImpulse(Vector2 impulse)
        {
            if (cachedRigidbody != null)
            {
                cachedRigidbody.AddForce(impulse, ForceMode2D.Impulse);
            }
        }

        public float GetSpeed()
        {
            return cachedRigidbody != null ? cachedRigidbody.linearVelocity.magnitude : 0f;
        }

        public void ResetVelocity()
        {
            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = Vector2.zero;
                cachedRigidbody.angularVelocity = 0f;
            }
        }
    }
}