using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;

    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = baseScale * scale;
    }
}