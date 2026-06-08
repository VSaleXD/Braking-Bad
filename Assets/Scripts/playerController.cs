using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public class playerController : MonoBehaviour
{
    private float elapsedTime = 0f;
    
    public float score = 0f;
    public float scoreMultiplier = 10f;
    public float ufoPoints = 100f;
    
    public float thrustforce = 5f;
    public float maxSpeed = 5f;
    public float rotaionSpeed = 200f;
    [Range(0f, 1f)]
    public float driftFactor = 0.95f;
    public float driftTrailThreshold = 1.5f;
    [SerializeField] private float driftSteerLag = 0.5f;
    [SerializeField] private float wallBounceMultiplier = 1.02f;
    [SerializeField] private PhysicsMaterial2D wallBounceMaterial;
    [SerializeField] private string wallTag = "Wall";

    public TrailRenderer[] tireTrails;

    [SerializeField] public bool isDestroyed = false;
    Rigidbody2D rb;
    Collider2D carCollider;
    public UIDocument uiDocument;
    public GameObject explosionEffect;
    private Label scoreText;
    private Vector2 lastVelocity;

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
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (uiDocument != null)
        {
            scoreText = uiDocument.rootVisualElement.Q<Label>("ScoreText");
        }

        SetTrailsEmitting(false);
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
        if (Mouse.current == null || Camera.main == null || rb == null)
        {
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0f;

        float steeringMultiplier = 1f;
        float throttleMultiplier = 1f;

        //if (tournamentAgent != null)
        //{
        //    steeringMultiplier = tournamentAgent.SteeringMultiplier;
        //    throttleMultiplier = tournamentAgent.ThrottleMultiplier;
        //}

        Vector2 targetDirection = mousePos - transform.position;
        if (Mathf.Abs(steeringMultiplier) > 0.001f && steeringMultiplier < 0f)
        {
            Vector3 mirroredMouse = transform.position + new Vector3(-(mousePos.x - transform.position.x), mousePos.y - transform.position.y, 0f);
            targetDirection = mirroredMouse - transform.position;
        }

        Vector2 directionToMouse = targetDirection.normalized;

        // Rotate Player
        float distanceToMouse = Vector2.Distance(mousePos, transform.position);
        if(distanceToMouse > 0.5f)
        {
            float speedT = Mathf.InverseLerp(driftTrailThreshold, maxSpeed, rb.linearVelocity.magnitude);
            float effectiveRotationSpeed = Mathf.Lerp(rotaionSpeed, rotaionSpeed * driftSteerLag, speedT);
            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, effectiveRotationSpeed * Mathf.Abs(steeringMultiplier) * Time.deltaTime);
        }

        rb.AddForce(transform.up * (thrustforce * throttleMultiplier));

        // Limit Speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        lastVelocity = rb.linearVelocity;
    }
    void applyDrift(){
        if (rb == null)
        {
            return;
        }

        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);

        float speedT = Mathf.InverseLerp(driftTrailThreshold, maxSpeed, rb.linearVelocity.magnitude);
        float dynamicDriftFactor = Mathf.Lerp(driftFactor, Mathf.Min(0.995f, driftFactor + 0.03f), speedT);
        rb.linearVelocity = forwardVelocity + rightVelocity * dynamicDriftFactor;

        // Efek trail
        if(Mathf.Abs(Vector2.Dot(rb.linearVelocity, transform.right)) > driftTrailThreshold)
        {
            SetTrailsEmitting(true);
        }
        else
        {
            SetTrailsEmitting(false);
        }
    }

    void SetTrailsEmitting(bool isEmitting)
    {
        foreach (TrailRenderer trail in tireTrails)
        {
            trail.emitting = isEmitting;
        }
    }
    void Update()
    {
        updateScore();
    }
    void FixedUpdate()
    {
        movePlayer();
        applyDrift();
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Cek jika menabrak UFO
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
        
        if (isDestroyed){
            Destroy(gameObject);
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }
    }

    private bool ShouldBounceOnCollision(Collision2D collision)
    {
        if (collision == null || rb == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(wallTag) && collision.collider != null && collision.collider.CompareTag(wallTag))
        {
            return true;
        }

        return collision.rigidbody == null;
    }

    private void BounceOffCollision(Collision2D collision)
    {
        if (collision.contactCount == 0 || rb == null)
        {
            return;
        }

        Vector2 normal = collision.GetContact(0).normal;
        Vector2 bouncedVelocity = Vector2.Reflect(lastVelocity.sqrMagnitude > 0.0001f ? lastVelocity : rb.linearVelocity, normal) * wallBounceMultiplier;
        rb.linearVelocity = bouncedVelocity;
    }
}
