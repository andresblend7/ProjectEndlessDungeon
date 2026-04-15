using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class CoinBehaviour : MonoBehaviour
{
    private Transform target;

    [Header("Launch (El 'Salto' Inicial)")]
    public float upForce = 8f;
    public float horizontalRandom = 2f;
    public float gravity = 25f;
    public float launchDuration = 0.4f;

    [Header("Homing (Atracción)")]
    public float homingAcceleration = 50f;
    private float currentSpeed = 0f;

    [Header("Drop & Wait (Modo Enemigo)")]
    public float dropHorizontalRandom = 1.5f;   // Dispersión en X y Z al caer
    public float dropBounceDamping = 0.4f;       // Cuánto rebota al tocar el suelo (0 = nada, 1 = todo)
    public float dropPickupRadius = 1.2f;        // Radio de recogida manual
    public float dropLifetime = 8f;              // Segundos hasta que desaparece si nadie la recoge
    public float dropHoverHeight = 0.2f;         // Altura de flotación sobre el suelo
    public float dropBobAmplitude = 0.08f;       // Amplitud del bob vertical mientras flota
    public float dropBobSpeed = 2f;              // Velocidad del bob

    [Header("Visual Effects")]
    public float rotationSpeed = 600f;
    private float initialRotationSpeed;
    public AnimationCurve scaleCurve;
    public float totalLife = 1.2f;

    [Header("Sounds")]
    public AudioClip collectSound;

    // --- Privados ---
    private Vector3 velocity;
    private float timer;
    private bool isHoming;
    private Vector3 initialScale;

    // Estado del modo drop
    private enum CoinMode { GoToPlayer, DropAndWait }
    private CoinMode mode;
    private bool isWaiting;       // True cuando ya llegó al suelo y está flotando
    private float groundY;        // Altura del suelo donde cayó
    private float dropTimer;      // Cuenta el tiempo de vida en modo drop


    private bool useCustomVelocity; // Si se le dio una velocidad personalizada al caer (ej. cofres)

    void Awake()
    {
        initialScale = transform.localScale;
        initialRotationSpeed = rotationSpeed;
    }

    // ─────────────────────────────────────────────
    //  MODO 1: Moneda de objeto roto → sigue al jugador
    // ─────────────────────────────────────────────
    public void GoToPlayer(Transform player)
    {
        mode = CoinMode.GoToPlayer;
        target = player;
        timer = 0f;
        isHoming = false;
        isWaiting = false;
        currentSpeed = 0f;

        velocity = new Vector3(
            Random.Range(-horizontalRandom, horizontalRandom),
            upForce,
            Random.Range(-horizontalRandom, horizontalRandom)
        );
    }

    // ─────────────────────────────────────────────
    //  MODO 2: Moneda de enemigo → cae y espera
    // ─────────────────────────────────────────────
    public void DropAndWait(Vector3 spawnPosition, Vector3? customVelocity)
    {
        mode = CoinMode.DropAndWait;
        timer = 0f;
        dropTimer = 0f;
        isHoming = false;
        isWaiting = false;
        currentSpeed = 0f;

        // Guardamos la Y del suelo (asumimos que es la Y del spawn; ajusta si usas raycast)
        groundY = spawnPosition.y;

        if (customVelocity.HasValue)
        {
            velocity = customVelocity.Value;
            useCustomVelocity = true;
            return;
        }


        useCustomVelocity = false;
        // Salto inicial con dispersión configurable
        velocity = new Vector3(
            Random.Range(-dropHorizontalRandom, dropHorizontalRandom),
            upForce,
            Random.Range(-dropHorizontalRandom, dropHorizontalRandom)
        );
    }

    // ─────────────────────────────────────────────
    //  UPDATE
    // ─────────────────────────────────────────────
    void Update()
    {
        float dt = Time.deltaTime;
        timer += dt;

        // Rotación estilo moneda (siempre activa)
        transform.Rotate(Vector3.up * rotationSpeed * dt, Space.World);

        if (mode == CoinMode.GoToPlayer)
        {
            UpdateGoToPlayer(dt);
        }
        else
        {
            UpdateDropAndWait(dt);
        }
    }

    void UpdateGoToPlayer(float dt)
    {
        rotationSpeed = initialRotationSpeed * 2.5f;

        // Animación de escala
        float normalizedTime = Mathf.Clamp01(timer / totalLife);
        transform.localScale = initialScale * scaleCurve.Evaluate(normalizedTime);

        if (!isHoming)
        {
            velocity.y -= gravity * dt;
            transform.position += velocity * dt;
            if (timer >= launchDuration) isHoming = true;
        }
        else
        {
            if (target == null) { Despawn(); return; }

            currentSpeed += homingAcceleration * dt;
            Vector3 dir = (target.position - transform.position).normalized;
            velocity = Vector3.Lerp(velocity, dir * currentSpeed, dt * 5f);
            transform.position += velocity * dt;

            if (Vector3.Distance(transform.position, target.position) < 0.6f)
                Collect();
        }

        if (timer >= totalLife) Collect();
    }

    void UpdateDropAndWait(float dt)
    {
        if (!isWaiting)
        {
            // ── Fase arco: cae con gravedad ──
            velocity.y -= gravity * dt;
            transform.position += velocity * dt;

            // Cuando toca o pasa el suelo
            if (transform.position.y <= groundY + dropHoverHeight)
            {
                Vector3 pos = transform.position;
                pos.y = groundY + dropHoverHeight;
                transform.position = pos;

                // Rebote opcional (apaga el vertical, frena el horizontal)
                if (!useCustomVelocity)
                {
                    velocity.y = Mathf.Abs(velocity.y) * dropBounceDamping;
                    velocity.x *= 0.3f;
                    velocity.z *= 0.3f;

                    // Si el rebote es insignificante, pasamos directo a espera
                    if (velocity.y < 0.5f)
                    {
                        velocity = Vector3.zero;
                        isWaiting = true;
                    }
                }
                else
                {
                    velocity = Vector3.zero;
                    isWaiting = true;
                }
            }
        }
        else
        {
            // ── Fase flotación: bob suave + cuenta regresiva ──
            dropTimer += dt;

            float bobY = groundY + dropHoverHeight + Mathf.Sin(dropTimer * dropBobSpeed) * dropBobAmplitude;
            Vector3 pos = transform.position;
            pos.y = bobY;
            transform.position = pos;

            // Timeout → desaparece sin dar moneda
            if (dropTimer >= dropLifetime)
            {
                Despawn();
                return;
            }
        }
    }

    // ─────────────────────────────────────────────
    //  Recogida manual por proximidad (modo DropAndWait)
    //  Llama esto desde tu PlayerController o usa OnTriggerEnter
    // ─────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (mode == CoinMode.DropAndWait && isWaiting && other.CompareTag("Player"))
            Collect();
    }

    // ─────────────────────────────────────────────
    void Collect()
    {
        AudioSource.PlayClipAtPoint(collectSound, transform.position);
        ResourceManager.Instance.Add(ResourceType.Coin, 1);
        FloatingTextPool.Instance.SpawnText($"+{1} coin", FloatingTextType.Resource);
        Despawn();
    }

    void Despawn()
    {
        CoinPool.Instance.Return(this);
    }
}