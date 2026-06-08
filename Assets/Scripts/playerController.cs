using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using BrakingBad.Gameplay;

public class playerController : MonoBehaviour
{
    private float elapsedTime = 0f;
    
    [Header("Scoring")]
    public float score = 0f;
    public float scoreMultiplier = 10f;
    public float ufoPoints = 100f;
    
    [Header("Movement State")]
    public bool movementEnabled = false;
    public float thrustforce = 5f;
    public float rotaionSpeed = 200f;

    [Header("Drift Settings")]
    [Range(0f, 1f)]
    public float driftFactor = 0.95f;
    public float driftTrailThreshold = 1.5f;
    [SerializeField] private float driftSteerLag = 0.5f;

    [Header("Wall Bouncing")]
    [SerializeField] private float wallBounceMultiplier = 1.02f;
    [SerializeField] private PhysicsMaterial2D wallBounceMaterial;
    [SerializeField] private string wallTag = "Wall";

    [Header("Effects & Destructions")]
    public GameObject explosionEffect;
    [SerializeField] public bool isDestroyed = false;

    // Properti Publik yang bisa dibaca oleh skrip WheelTrailHandler
    public bool IsDrifting { get; private set; }

    private Rigidbody2D rb;
    private Collider2D carCollider;
    private TournamentPlayerAgent tournamentAgent;
    public UIDocument uiDocument;
    private Label scoreText;
    private Vector2 lastVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        carCollider = GetComponent<Collider2D>();
        tournamentAgent = GetComponent<TournamentPlayerAgent>();

        if (carCollider != null && wallBounceMaterial != null)
        {
            carCollider.sharedMaterial = wallBounceMaterial;
        }
    }
    float getLateralvelocity()
    {
        return Vector2.Dot(transform.right, rb.linearVelocity);
    }
    public bool isTireScreeching(out float lateralVelocity, out bool isDrifting)
    {
        lateralVelocity = getLateralvelocity();
        isDrifting = Mathf.Abs(lateralVelocity) > driftTrailThreshold;
        return isDrifting;
    }

    void Start()
    {
        if (tournamentAgent == null)
        {
            tournamentAgent = GetComponent<TournamentPlayerAgent>();
        }

        if (uiDocument != null)
        {
            scoreText = uiDocument.rootVisualElement.Q<Label>("ScoreText");
        }
    }

    void Update()
    {
        updateScore();
    }

    void FixedUpdate()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            movementEnabled = true;
        }
        if (movementEnabled)
        {
            movePlayer();
        }
        killOrthogonalVelocity();
    }

    void updateScore()
    {
        elapsedTime += Time.deltaTime;
        score = Mathf.FloorToInt(elapsedTime * scoreMultiplier);

        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }



    void movePlayer()
    {
        if (Camera.main == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0f;

        float steeringMultiplier = 1f;
        float throttleMultiplier = 1f;

        if (tournamentAgent != null)
        {
            steeringMultiplier = tournamentAgent.SteeringMultiplier;
            throttleMultiplier = tournamentAgent.ThrottleMultiplier;
        }

        Vector2 targetDirection = mousePos - transform.position;
        if (Mathf.Abs(steeringMultiplier) > 0.001f && steeringMultiplier < 0f)
        {
            Vector3 mirroredMouse = transform.position + new Vector3(-(mousePos.x - transform.position.x), mousePos.y - transform.position.y, 0f);
            targetDirection = mirroredMouse - transform.position;
        }

        Vector2 directionToMouse = targetDirection.normalized;

        // Rotate Player
        float distanceToMouse = Vector2.Distance(mousePos, transform.position);
        if (distanceToMouse > 0.5f)
        {
            float speedT = Mathf.Clamp01(rb.linearVelocity.magnitude / driftTrailThreshold);
            float effectiveRotationSpeed = Mathf.Lerp(rotaionSpeed, rotaionSpeed * driftSteerLag, speedT);
            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            
            // Menggunakan Time.fixedDeltaTime karena fungsi ini dipanggil di FixedUpdate
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, effectiveRotationSpeed * Mathf.Abs(steeringMultiplier) * Time.fixedDeltaTime);
        }

        rb.AddForce(transform.up * (thrustforce * throttleMultiplier));
        lastVelocity = rb.linearVelocity;
    }

    void killOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity = forwardVelocity + rightVelocity * driftFactor;

        // Mengecek kecepatan menyamping untuk menentukan status drifting secara riil
        IsDrifting = Mathf.Abs(Vector2.Dot(rb.linearVelocity, transform.right)) > driftTrailThreshold;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("UFO"))
        {
            score += ufoPoints;
            if (scoreText != null)
            {
                scoreText.text = "Score: " + score.ToString();
            }
        }

        if (ShouldBounceOnCollision(collision))
        {
            BounceOffCollision(collision);
        }
        
        if (isDestroyed)
        {
            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, transform.rotation);
            }
            Destroy(gameObject);
        }
    }

    private bool ShouldBounceOnCollision(Collision2D collision)
    {
        if (collision == null || rb == null) return false;

        if (!string.IsNullOrWhiteSpace(wallTag) && collision.collider != null && collision.collider.CompareTag(wallTag))
        {
            return true;
        }

        return collision.rigidbody == null;
    }

    private void BounceOffCollision(Collision2D collision)
    {
        if (collision.contactCount == 0 || rb == null) return;

        Vector2 normal = collision.GetContact(0).normal;
        Vector2 bouncedVelocity = Vector2.Reflect(lastVelocity.sqrMagnitude > 0.0001f ? lastVelocity : rb.linearVelocity, normal) * wallBounceMultiplier;
        rb.linearVelocity = bouncedVelocity;
    }
}