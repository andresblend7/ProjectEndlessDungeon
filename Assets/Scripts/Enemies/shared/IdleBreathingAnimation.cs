using UnityEngine;

public class IdleBreathingAnimation : MonoBehaviour
{
    [Header("Breathing Settings")]
    public float amplitude = 0.05f;     // qué tanto escala (0.02–0.1 recomendado)
    public float frequency = 1.5f;      // velocidad respiración
    public bool affectY = true;         // estira en Y
    public bool inverseXZ = true;       // squash en XZ cuando Y crece

    private Vector3 baseScale;
    private bool isActive = true;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (!isActive) return;

        float t = Mathf.Sin(Time.time * frequency) * amplitude;

        float scaleY = affectY ? (1 + t) : 1;
        float scaleXZ = inverseXZ ? (1 - t * 0.5f) : 1;

        transform.localScale = new Vector3(
            baseScale.x * scaleXZ,
            baseScale.y * scaleY,
            baseScale.z * scaleXZ
        );
    }

    // =========================
    // PUBLIC API
    // =========================

    public void EnableBreathing()
    {
        isActive = true;
    }

    public void DisableBreathing()
    {
        isActive = false;
        transform.localScale = baseScale;
    }

    public void SetIntensity(float newAmplitude)
    {
        amplitude = newAmplitude;
    }
}
