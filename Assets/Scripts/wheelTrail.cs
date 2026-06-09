using UnityEngine;

public class wheelTrail: MonoBehaviour
{
    private TrailRenderer[] tireTrails;

    private playerController carController;

    void Awake()
    {
        carController = GetComponent<playerController>();
    }

    void Start()
    {
        SetTrailsEmitting(false);
    }

    void Update()
    {
        if (carController != null)
        {
            SetTrailsEmitting(carController.IsDrifting && carController.movementEnabled);
        }
    }

    private void SetTrailsEmitting(bool isEmitting)
    {
        if (tireTrails == null) return;

        foreach (TrailRenderer trail in tireTrails)
        {
            if (trail != null)
            {
                trail.emitting = isEmitting;
            }
        }
    }
}