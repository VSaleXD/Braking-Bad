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

    public TrailRenderer[] tireTrails;

    [SerializeField] public bool isDestroyed = false;
    Rigidbody2D rb;
    public UIDocument uiDocument;
    public GameObject explosionEffect;
    private Label scoreText;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        scoreText = uiDocument.rootVisualElement.Q<Label>("ScoreText");

        SetTrailsEmitting(false);
    }
    void updateScore()
    {
        elapsedTime += Time.deltaTime;
        score = Mathf.FloorToInt(elapsedTime * scoreMultiplier);
        scoreText.text = "Score: " + score.ToString();
    }
    void movePlayer()
    {
        // Calculate Mouse Direction
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 directionToMouse = (mousePos - transform.position).normalized;

        // Rotate Player
        if(Vector2.Distance(mousePos, transform.position) > 0.5f)
        {
            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotaionSpeed * Time.deltaTime);
        }
        rb.AddForce(transform.up * thrustforce);

        // Limit Speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
    void applyDrift(){
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity = forwardVelocity + rightVelocity * driftFactor;

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
            scoreText.text = "Score: " + score.ToString();
        }
        
        if (isDestroyed){
            Destroy(gameObject);
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }
    }
}
