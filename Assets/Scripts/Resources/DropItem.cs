using UnityEngine;

public class DropItem : MonoBehaviour
{
    [Header("Animación de caída")]
    public float upForce = 6f;
    public float horizontalForce = 2f;
    public float gravity = 18f;
    public float groundOffset = 0.05f;

    [Header("Flotado")]
    public float floatSpeed = 1.5f;
    public float floatHeight = 0.3f;

    [Header("Rotación")]
    public float rotationSpeed = 45f;
    public Vector3 rotationAxis = Vector3.up;

    [Header("Lifetime")]
    public float lifetime = 10f;
    public float blinkDuration = 2f;
    public float blinkInterval = 0.2f;

    private Vector3 startPosition;
    private Vector3 velocity;

    private bool isJumping;
    private bool isBlinking;

    private float timer;
    private float blinkTimer;
    private bool visible = true;

    private Renderer objectRenderer;

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
    }

    // -------------------------
    // IMPORTANTE: llamado por el pool
    // -------------------------
    public void Play()
    {
        timer = 0f;
        blinkTimer = 0f;

        isJumping = true;
        isBlinking = false;

        visible = true;

        if (objectRenderer != null)
            objectRenderer.enabled = true;

        // IMPORTANTE: resetear posición base
        startPosition = transform.position;

        velocity = new Vector3(
            Random.Range(-horizontalForce, horizontalForce),
            upForce,
            Random.Range(-horizontalForce, horizontalForce)
        );
    }

 

    void Update()
    {
        float dt = Time.deltaTime;
        timer += dt;

        // -------------------------
        // FASE 1: SALTO
        // -------------------------
        if (isJumping)
        {
            Vector3 prevPosition = transform.position;

            // aplicar gravedad
            velocity.y -= gravity * dt;

            Vector3 nextPosition = prevPosition + velocity * dt;

            // detectar cruce del suelo (y = 0)
            if (nextPosition.y <= groundOffset)
            {
                if (Mathf.Abs(velocity.y) > 2f)
                {
                    velocity.y *= -0.3f; // rebote pequeño
                    transform.position = new Vector3(nextPosition.x, groundOffset, nextPosition.z);
                }
                else
                {
                    transform.position = new Vector3(nextPosition.x, groundOffset, nextPosition.z);

                    isJumping = false;
                    startPosition = transform.position;
                    velocity = Vector3.zero;
                }

                return;
            }

            transform.position = nextPosition;
        }
        // -------------------------
        // FASE 2: IDLE
        // -------------------------
        else
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;

            transform.position = new Vector3(
                startPosition.x,
                newY,
                startPosition.z
            );
        }

        // Rotación siempre
        transform.Rotate(rotationAxis, rotationSpeed * dt);

        // -------------------------
        // BLINK
        // -------------------------
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
            {
                Despawn();
            }
        }
    }

    void ToggleVisibility()
    {
        visible = !visible;

        if (objectRenderer != null)
            objectRenderer.enabled = visible;
    }

    void Despawn()
    {
        // regresar al pool (ajusta según tu pool real)
        gameObject.SetActive(false);
    }
}