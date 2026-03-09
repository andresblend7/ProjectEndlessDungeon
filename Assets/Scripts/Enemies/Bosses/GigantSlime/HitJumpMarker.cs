using UnityEngine;

public class HitJumpMarker : MonoBehaviour
{
    public Transform boss;

    [Header("Shadow Settings")]
    public float minScale = 0.3f;
    public float maxScale = 3f;
    public float maxHeight = 6f; // altura máxima esperada del salto

    void Update()
    {
        if (boss == null) return;

        // Mantener marcador debajo del boss
        Vector3 pos = boss.position;
        pos.y = transform.position.y;
        transform.position = pos;

        // Calcular altura del boss respecto al marcador
        float height = boss.position.y - transform.position.y;

        // Convertir altura a porcentaje
        float t = Mathf.Clamp01(height / maxHeight);

        // Invertir para que cuando esté alto sea pequeño
        float scale = Mathf.Lerp(maxScale, minScale, t);

        transform.localScale = new Vector3(scale, 1f, scale);
    }
}