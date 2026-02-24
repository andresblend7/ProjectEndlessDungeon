using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro text;

    [Header("Animation")]
    public float lifetime = 0.8f;
    public float floatSpeed = 2f;
    public float horizontalSpread = 0.5f;
    public float startScaleMultiplier = 1.4f;

    [Header("Colors")]
    public Color damageColor = Color.red;
    public Color healColor = Color.green;
    public Color critColor = new Color(1f, 0.85f, 0.2f);

    float timer;
    Vector3 velocity;
    Vector3 baseScale;
    Color baseColor;
    bool active;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    public void Show(int amount, bool isHeal, bool isCrit = false)
    {
        active = true;
        timer = 0f;

        text.text = amount.ToString();

        if (isCrit)
            baseColor = critColor;
        else
            baseColor = isHeal ? healColor : damageColor;

        text.color = baseColor;

        transform.localScale = baseScale * startScaleMultiplier;

        velocity = new Vector3(
            Random.Range(-horizontalSpread, horizontalSpread),
            floatSpeed,
            Random.Range(-horizontalSpread, horizontalSpread)
        );
    }

    void Update()
    {
        if (!active) return;

        timer += Time.deltaTime;
        float t = timer / lifetime;

        // Movimiento flotante
        transform.position += velocity * Time.deltaTime;

        // Desaceleración suave
        velocity *= 0.96f;

        // Escala vuelve a normal
        transform.localScale = Vector3.Lerp(
            baseScale * startScaleMultiplier,
            baseScale,
            t * 4f
        );

        // Fade out
        Color c = baseColor;
        c.a = 1f - t;
        text.color = c;

        // Billboard simple
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;

        if (timer >= lifetime)
        {
            active = false;
            DamageNumberPool.Instance.Return(this);
        }
    }
}