using TMPro;
using UnityEngine;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro textMesh;

    private Coroutine animRoutine;

    public void Play(string content, Color color, FloatingTextPool.Settings settings)
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        textMesh.text = content;
        textMesh.color = new Color(color.r, color.g, color.b, 0f);

        transform.localScale = transform.localScale * settings.startScale;

        animRoutine = StartCoroutine(Animate(settings));
    }

    private IEnumerator Animate(FloatingTextPool.Settings settings)
    {
        float time = 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + settings.offset;

        while (time < settings.duration)
        {
            float t = time / settings.duration;

            // Movimiento
            transform.position = Vector3.Lerp(startPos, endPos, settings.motionCurve.Evaluate(t));

            // Escala
            float scale = Mathf.Lerp(settings.startScale, settings.endScale, settings.scaleCurve.Evaluate(t));
            transform.localScale = Vector3.one * scale;

            // Alpha
            float alpha = settings.alphaCurve.Evaluate(t);
            var c = textMesh.color;
            textMesh.color = new Color(c.r, c.g, c.b, alpha);

            time += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}