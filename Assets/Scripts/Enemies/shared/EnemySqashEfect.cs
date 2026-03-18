using System.Collections;
using UnityEngine;

public class EnemySqashEfect : MonoBehaviour
{
    private Vector3 originalScale;
    public float intensity = 0.2f;
    public float duration = 0.1f;


    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void PlaySquash()
    {
        StartCoroutine(SquashRoutine(intensity, duration));
    }
    IEnumerator SquashRoutine(float intensity, float duration)
    {
        float timer = 0f;

        Vector3 squashScale = new Vector3(
            originalScale.x * (1f + intensity),
            originalScale.y * (1f - intensity),
            originalScale.z * (1f + intensity)
        );

        // squash
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, squashScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;

        // volver a normal
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(squashScale, originalScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
