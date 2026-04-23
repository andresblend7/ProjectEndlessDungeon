using System.Collections.Generic;
using UnityEngine;

public class LootBag : MonoBehaviour
{
    [Header("Animación de caída")]
    public float upForce = 6f;
    public float horizontalForce = 2f;
    public float gravity = 9f;
    public float groundOffset = 0f;

    [Header("Flotado")]
    public float floatSpeed = 1.8f;
    public float floatHeight = 0.21f;

    [Header("Rotación")]
    public float rotationSpeed = 45f;
    public Vector3 rotationAxis = Vector3.up;

    [Header("Lifetime")]
    public float lifetime = 10f;
    public float blinkDuration = 2f;
    public float blinkInterval = 0.2f;

    // ─── Cinemática ───────────────────────────────
    private Vector3 launchOrigin;   // posición en el momento del lanzamiento
    private Vector3 launchVelocity; // velocidad inicial (no se modifica nunca)
    private float jumpTime;       // tiempo acumulado dentro de la fase de salto
    // ──────────────────────────────────────────────

    private Vector3 restPosition;   // posición final cuando aterriza

    private bool isJumping;
    private bool isBlinking;

    private float timer;
    private float blinkTimer;
    private bool visible = true;

    private Renderer objectRenderer;
    private List<ItemBagResource> resourcesInBag;

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
    }

    public void Play(Vector3 customVelocity)
    {
        Debug.Log($"Play() called. transform.position antes de reset: {transform.position}");

        timer = 0f;
        blinkTimer = 0f;
        jumpTime = 0f;

        isJumping = true;
        isBlinking = false;
        visible = true;

        if (objectRenderer != null)
            objectRenderer.enabled = true;

        // ✅ Forzar Y al groundOffset antes de capturar el origen
        // El objeto puede venir del flotado con Y desplazado
        transform.position = new Vector3(
            transform.position.x,
            groundOffset,           // <── anclar al suelo
            transform.position.z
        );

        launchOrigin = transform.position;
        launchVelocity = customVelocity != Vector3.zero
            ? customVelocity
            : new Vector3(
                Random.Range(-horizontalForce, horizontalForce),
                upForce,
                Random.Range(-horizontalForce, horizontalForce)
              );
        Debug.Log($"launchOrigin capturado: {launchOrigin}");
    }

    void Update()
    {
        float dt = Time.deltaTime;
        timer += dt;

        if (isJumping)
        {
            jumpTime += dt;

            // ─── Posición exacta con ecuación cinemática ───────────────
            // x(t) = x₀ + vx·t          (sin fricción horizontal)
            // y(t) = y₀ + vy·t - ½·g·t²
            // z(t) = z₀ + vz·t
            float t = jumpTime;
            Vector3 pos = launchOrigin + new Vector3(
                launchVelocity.x * t,
                launchVelocity.y * t - 0.5f * gravity * t * t,
                launchVelocity.z * t
            );
            // ───────────────────────────────────────────────────────────

            if (pos.y <= groundOffset)
            {
                // Velocidad vertical exacta en el momento del impacto: vy - g·t
                float impactVy = launchVelocity.y - gravity * t;

                if (Mathf.Abs(impactVy) > 2f)
                {
                    // Rebote: relanzar desde el punto de impacto con velocidad invertida
                    launchOrigin = new Vector3(pos.x, groundOffset, pos.z);
                    launchVelocity = new Vector3(
                        launchVelocity.x,
                        -impactVy * 0.3f,   // amortiguación
                        launchVelocity.z
                    );
                    jumpTime = 0f;
                    transform.position = launchOrigin;
                }
                else
                {
                    // Aterriza definitivamente
                    restPosition = new Vector3(pos.x, groundOffset, pos.z);
                    transform.position = restPosition;
                    isJumping = false;
                    Debug.Log("LootBag has landed at: " + restPosition);
                }

                return;
            }

            transform.position = pos;
        }
        else
        {
            // Fase idle: flota sobre el punto de descanso
            float newY = restPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(restPosition.x, newY, restPosition.z);
        }

        transform.Rotate(rotationAxis, rotationSpeed * dt);

        // ─── Blink ───
        if (!isBlinking && timer >= lifetime)
        {
            isBlinking = true;
            blinkTimer = 0f;
        }

        if (isBlinking)
        {
            blinkTimer += dt;

            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                ToggleVisibility();
            }

            if (timer >= lifetime + blinkDuration)
                Despawn();
        }
    }

    void ToggleVisibility()
    {
        visible = !visible;
        if (objectRenderer != null)
            objectRenderer.enabled = visible;
    }

    void Despawn() => gameObject.SetActive(false);

    public void SetResources(List<ItemBagResource> resources) => resourcesInBag = resources;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var item in resourcesInBag)
                ResourceManager.Instance.Add(item.resourceType, item.count);
            Despawn();
        }
    }
}