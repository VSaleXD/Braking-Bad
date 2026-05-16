using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class playerController : MonoBehaviour
{
    private float elapsedTime = 0f;
    public float score = 0f;
    public float scoreMultiplier = 10f;

    public float thrustforce = 1f;
    public float maxSpeed = 5f;
    Rigidbody2D rb;
    public UIDocument uiDocument;
    private Label scoreText;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        scoreText = uiDocument.rootVisualElement.Q<Label>("ScoreText");
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
        Vector2 direction = (mousePos - transform.position).normalized;

        // Rotate Player
        transform.up = direction;
        rb.AddForce(direction * thrustforce);

        // Limit Speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
    void FixedUpdate()
    {
        updateScore();
        movePlayer();
    }
}
