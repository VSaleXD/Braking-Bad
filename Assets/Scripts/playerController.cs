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

    [Header("Effects & Destructions")]
    public GameObject explosionEffect;
    [SerializeField] public bool isDestroyed = false;

    public bool IsDrifting { get; private set; }

    private Rigidbody2D rb;
    private Collider2D carCollider;
    private TournamentPlayerAgent tournamentAgent;
    private Vector2 lastVelocity;

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
        lastVelocity = rb.linearVelocity;

        if (movementEnabled)
        {
            movePlayer();
        }

        killOrthogonalVelocity();
    }

    // Mapping keyboard split 4 pemain:
    // P1: A/D | P2: Arrow Left/Right | P3: J/L | P4: Numpad4/Numpad6
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

        if (Mathf.Abs(steerInput) > 0.01f)
        {
            float speedT = Mathf.Clamp01(rb.linearVelocity.magnitude / Mathf.Max(maxSpeed, 0.01f));
            float effectiveRotationSpeed = Mathf.Lerp(rotaionSpeed, rotaionSpeed * driftSteerLag, speedT);

            float rotationDelta = steerInput * steeringMultiplier * effectiveRotationSpeed * Time.fixedDeltaTime;
            transform.Rotate(0f, 0f, -rotationDelta);
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