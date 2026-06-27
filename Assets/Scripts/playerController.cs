using UnityEngine;
using UnityEngine.InputSystem;
using BrakingBad.Gameplay;

public class playerController : MonoBehaviour
{
    [Header("Movement State")]
    public bool movementEnabled = true;
    public float maxSpeed = 10f;
    public float thrustforce = 5f;
    public float rotaionSpeed = 200f;

    [Header("Drift Settings")]
    [Range(0f, 1f)]
    public float driftFactor = 0.95f;
    public float driftTrailThreshold = 1.5f;
    [SerializeField] private float driftSteerLag = 0.5f;

    [Header("Chaos / Oversteer")]
    [SerializeField] private float oversteerBuildupDelay = 0.4f;
    [SerializeField] private float oversteerGainRate = 1.5f;
    [SerializeField] private float maxOversteerMultiplier = 2.2f;
    [SerializeField] private float oversteerJitterStrength = 15f;

    [Header("Anti Spin-Lock Safety")]
    [SerializeField] private float unwantedSpinThreshold = 120f;
    [SerializeField] private float spinLockGraceTime = 0.1f;
    [SerializeField] private float spinLockDamping = 0.3f;

    [Header("Collision Spin Damping")]
    [Tooltip("Kelipatan angularVelocity yang dipertahankan tepat saat collision (0 = langsung 0, 1 = tidak diredam)")]
    [SerializeField, Range(0f, 1f)] private float collisionSpinRetention = 0.2f;

    [Header("Effects & Destructions")]
    public GameObject explosionEffect;
    [SerializeField] public bool isDestroyed = false;

    public bool IsDrifting { get; private set; }

    private Rigidbody2D rb;
    private Collider2D carCollider;
    private TournamentPlayerAgent tournamentAgent;

    private float steerHeldTime = 0f;
    private float currentOversteerMultiplier = 1f;
    private float unwantedSpinTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        carCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (tournamentAgent == null)
        {
            tournamentAgent = GetComponent<TournamentPlayerAgent>();
        }
    }

    public bool isTireScreeching(out float lateralVelocity, out bool isDrifting)
    {
        lateralVelocity = getLateralVelocity();
        isDrifting = Mathf.Abs(lateralVelocity) > driftTrailThreshold;
        return isDrifting;
    }

    float getLateralVelocity()
    {
        return Vector2.Dot(transform.right, rb.linearVelocity);
    }

    void FixedUpdate()
    {
        if (movementEnabled)
        {
            movePlayer();
        }

        ApplyAntiSpinLockSafety();
        killOrthogonalVelocity();
    }

    private float GetSteerInput()
    {
        if (Keyboard.current == null) return 0f;

        int playerID = tournamentAgent != null ? tournamentAgent.PlayerID : 1;
        float steer = 0f;

        switch (playerID)
        {
            case 1:
                if (Keyboard.current.aKey.isPressed) steer -= 1f;
                if (Keyboard.current.dKey.isPressed) steer += 1f;
                break;
            case 2:
                if (Keyboard.current.leftArrowKey.isPressed) steer -= 1f;
                if (Keyboard.current.rightArrowKey.isPressed) steer += 1f;
                break;
            case 3:
                if (Keyboard.current.jKey.isPressed) steer -= 1f;
                if (Keyboard.current.lKey.isPressed) steer += 1f;
                break;
            case 4:
                if (Keyboard.current.numpad4Key.isPressed) steer -= 1f;
                if (Keyboard.current.numpad6Key.isPressed) steer += 1f;
                break;
        }

        return steer;
    }

    void movePlayer()
    {
        float steerInput = GetSteerInput();

        float steeringMultiplier = 1f;
        float throttleMultiplier = 1f;

        if (tournamentAgent != null)
        {
            steeringMultiplier = tournamentAgent.steeringMultiplier;
            throttleMultiplier = tournamentAgent.throttleMultiplier;
        }

        UpdateOversteerBuildup(steerInput);

        if (Mathf.Abs(steerInput) > 0.01f)
        {
            float speedT = Mathf.Clamp01(rb.linearVelocity.magnitude / Mathf.Max(maxSpeed, 0.01f));
            float effectiveRotationSpeed = Mathf.Lerp(rotaionSpeed, rotaionSpeed * driftSteerLag, speedT);
            effectiveRotationSpeed *= currentOversteerMultiplier;

            float rotationDelta = steerInput * steeringMultiplier * effectiveRotationSpeed * Time.fixedDeltaTime;

            if (currentOversteerMultiplier > 1.3f)
            {
                float jitter = (Random.value - 0.5f) * 2f * oversteerJitterStrength * Time.fixedDeltaTime;
                rotationDelta += jitter * Mathf.Sign(steerInput);
            }

            transform.Rotate(0f, 0f, -rotationDelta);
        }

        rb.AddForce(transform.up * (thrustforce * throttleMultiplier));

        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void UpdateOversteerBuildup(float steerInput)
    {
        float speedRatio = Mathf.Clamp01(rb.linearVelocity.magnitude / Mathf.Max(maxSpeed, 0.01f));
        bool isSteeringHard = Mathf.Abs(steerInput) > 0.01f && speedRatio > 0.5f;

        if (isSteeringHard)
        {
            steerHeldTime += Time.fixedDeltaTime;

            if (steerHeldTime > oversteerBuildupDelay)
            {
                float buildupTime = steerHeldTime - oversteerBuildupDelay;
                currentOversteerMultiplier = 1f + Mathf.Min(
                    buildupTime * oversteerGainRate,
                    maxOversteerMultiplier - 1f
                );
            }
        }
        else
        {
            steerHeldTime = Mathf.Max(0f, steerHeldTime - Time.fixedDeltaTime * 2f);
            currentOversteerMultiplier = Mathf.Lerp(currentOversteerMultiplier, 1f, Time.fixedDeltaTime * 3f);
        }
    }

    void killOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity = forwardVelocity + rightVelocity * driftFactor;

        IsDrifting = Mathf.Abs(Vector2.Dot(rb.linearVelocity, transform.right)) > driftTrailThreshold;
    }

    private void ApplyAntiSpinLockSafety()
    {
        float steerInput = GetSteerInput();
        bool playerIsSteering = Mathf.Abs(steerInput) > 0.01f;
        float angularSpeed = Mathf.Abs(rb.angularVelocity);

        bool suspectedSpinLock = !playerIsSteering && angularSpeed > unwantedSpinThreshold;

        if (suspectedSpinLock)
        {
            unwantedSpinTimer += Time.fixedDeltaTime;

            if (unwantedSpinTimer > spinLockGraceTime)
            {
                rb.angularVelocity *= spinLockDamping;
            }
        }
        else
        {
            unwantedSpinTimer = 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb != null)
        {
            rb.angularVelocity *= collisionSpinRetention;
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
}