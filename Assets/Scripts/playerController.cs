using UnityEngine;
using UnityEngine.InputSystem;
using BrakingBad.Gameplay;

public class playerController : MonoBehaviour
{
    [Header("Movement State")]
    public bool movementEnabled = false;
    public float maxSpeed = 10f;
    public float thrustforce = 5f;
    public float rotaionSpeed = 200f;

    [Header("Drift Settings")]
    [Range(0f, 1f)]
    public float driftFactor = 0.95f;
    public float driftTrailThreshold = 1.5f;
    [SerializeField] private float driftSteerLag = 0.5f;

    [Header("Wall Bouncing")]
    // FIX: nilai di atas 1.0 bikin mobil makin cepat tiap kena dinding.
    // Pakai nilai 0.4-0.7 untuk pantulan yang masuk akal.
    [SerializeField] private float wallBounceMultiplier = 0.5f;
    [SerializeField] private PhysicsMaterial2D wallBounceMaterial;
    [SerializeField] private string wallTag = "Wall";

    [Header("Effects & Destructions")]
    public GameObject explosionEffect;
    [SerializeField] public bool isDestroyed = false;

    public bool IsDrifting { get; private set; }

    private Rigidbody2D rb;
    private Collider2D carCollider;
    private TournamentPlayerAgent tournamentAgent;
    private Vector2 lastVelocity;

    // FIX: flag agar bounce tidak dipanggil dua kali dalam satu collision
    private bool hasBounced = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        carCollider = GetComponent<Collider2D>();

        if (carCollider != null && wallBounceMaterial != null)
        {
            carCollider.sharedMaterial = wallBounceMaterial;
        }
    }

    void Start()
    {
        if (tournamentAgent == null)
        {
            tournamentAgent = GetComponent<TournamentPlayerAgent>();
        }
    }

    float getLateralVelocity()
    {
        return Vector2.Dot(transform.right, rb.linearVelocity);
    }

    public bool isTireScreeching(out float lateralVelocity, out bool isDrifting)
    {
        lateralVelocity = getLateralVelocity();
        isDrifting = Mathf.Abs(lateralVelocity) > driftTrailThreshold;
        return isDrifting;
    }

    void FixedUpdate()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            movementEnabled = true;
        }

        // FIX: simpan velocity SEBELUM physics update, bukan sesudah
        // supaya nilai lastVelocity selalu valid saat collision terjadi
        lastVelocity = rb.linearVelocity;

        if (movementEnabled)
        {
            movePlayer();
        }

        killOrthogonalVelocity();

        // Reset bounce flag setiap physics frame
        hasBounced = false;
    }

    void movePlayer()
    {
        if (Camera.main == null) return;
        if (Mouse.current == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0f;

        float steeringMultiplier = 1f;
        float throttleMultiplier = 1f;

        if (tournamentAgent != null)
        {
            steeringMultiplier = tournamentAgent.steeringMultiplier;
            throttleMultiplier = tournamentAgent.throttleMultiplier;
        }

        Vector2 targetDirection = mousePos - transform.position;

        if (Mathf.Abs(steeringMultiplier) > 0.001f && steeringMultiplier < 0f)
        {
            Vector3 mirroredMouse = transform.position + new Vector3(
                -(mousePos.x - transform.position.x),
                mousePos.y - transform.position.y,
                0f
            );
            targetDirection = mirroredMouse - transform.position;
        }

        // FIX: threshold rotate diturunkan dari 0.5 ke 0.1
        // supaya rotate tetap jalan meski mouse dekat dengan mobil
        float distanceToMouse = targetDirection.magnitude;
        if (distanceToMouse > 0.1f)
        {
            Vector2 directionToMouse = targetDirection.normalized;

            float speedT = Mathf.Clamp01(rb.linearVelocity.magnitude / Mathf.Max(maxSpeed, 0.01f));
            float effectiveRotationSpeed = Mathf.Lerp(rotaionSpeed, rotaionSpeed * driftSteerLag, speedT);

            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                effectiveRotationSpeed * Mathf.Abs(steeringMultiplier) * Time.fixedDeltaTime
            );
        }

        rb.AddForce(transform.up * (thrustforce * throttleMultiplier));

        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void killOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity = forwardVelocity + rightVelocity * driftFactor;

        IsDrifting = Mathf.Abs(Vector2.Dot(rb.linearVelocity, transform.right)) > driftTrailThreshold;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (ShouldBounceOnCollision(collision) && !hasBounced)
        {
            BounceOffCollision(collision);
            hasBounced = true;
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

        if (!string.IsNullOrWhiteSpace(wallTag) &&
            collision.collider != null &&
            collision.collider.CompareTag(wallTag))
        {
            return true;
        }

        return collision.rigidbody == null;
    }

    private void BounceOffCollision(Collision2D collision)
    {
        if (collision.contactCount == 0 || rb == null) return;

        Vector2 normal = collision.GetContact(0).normal;

        // FIX: gunakan lastVelocity yang disimpan di awal FixedUpdate
        // agar nilai selalu valid dan tidak nol
        Vector2 velocityToReflect = lastVelocity.sqrMagnitude > 0.01f
            ? lastVelocity
            : rb.linearVelocity;

        // FIX: clamp hasil bounce agar tidak melebihi maxSpeed
        Vector2 bouncedVelocity = Vector2.Reflect(velocityToReflect, normal) * wallBounceMultiplier;
        bouncedVelocity = Vector2.ClampMagnitude(bouncedVelocity, maxSpeed);

        rb.linearVelocity = bouncedVelocity;
    }
}