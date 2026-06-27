using UnityEngine;

public class SelfDestructParticle : MonoBehaviour
{
    void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 2f;
        Destroy(gameObject, lifetime);
    }
}