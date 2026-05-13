using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class UFO : MonoBehaviour
{
    public float minSize = 0.5f;
    public float maxSize = 2.5f;
    public float minSpeed = 500f;
    public float maxSpeed = 1000f;
    public float maxSpinSpeed = 10f;
    [SerializeField] public bool isDestroyed = false;
    Rigidbody2D rb;
    void Start()
    {
        float randomSize = Random.Range(minSize, maxSize);
        transform.localScale = new Vector3(randomSize, randomSize, 1);

        float randomSpeed = Random.Range(minSpeed, maxSpeed) / randomSize;
        rb = GetComponent<Rigidbody2D>();
        Vector2 randomDirection = Random.insideUnitCircle;
        rb.AddForce(randomDirection * randomSpeed);

        float randomTorque = Random.Range(-maxSpinSpeed, maxSpinSpeed);
        rb.AddTorque(randomTorque);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDestroyed)
        {
            Destroy(gameObject);
        }
    }
    
}
