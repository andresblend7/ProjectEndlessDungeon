using System.Collections;
using UnityEngine;

public class AcidSplash : MonoBehaviour
{
    [Header("Tiempo")]
    public float lifeTime = 4f;
    public float shrinkDuration = 0.8f;

    Vector3 originalScale;
    Material materialInstance;
    Color originalColor;

    void Start()
    {
        originalScale = transform.localScale;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            materialInstance = rend.material; // instancia del material
            originalColor = materialInstance.color;
        }

        StartCoroutine(ShrinkRoutine());
    }

    IEnumerator ShrinkRoutine()
    {
        yield return new WaitForSeconds(lifeTime);

        float timer = 0f;

        while (timer < shrinkDuration)
        {
            float t = timer / shrinkDuration;

            // Escala
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            // Fade del material
            if (materialInstance != null)
            {
                Color c = originalColor;
                c.a = Mathf.Lerp(originalColor.a, 0f, t);
                materialInstance.color = c;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
