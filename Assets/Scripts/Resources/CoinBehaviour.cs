using UnityEngine;

public class CoinBehaviour : MonoBehaviour
{
    private Transform target;

    [Header("Launch (El 'Salto' Inicial)")]
    public float upForce = 8f;
    public float horizontalRandom = 2f;
    public float gravity = 25f;
    public float launchDuration = 0.4f; // Un poco más largo para que se vea el arco

    [Header("Homing (Atracción)")]
    public float homingAcceleration = 50f;
    private float currentSpeed = 0f;

    [Header("Visual Effects")]
    public float rotationSpeed = 600f;
    public AnimationCurve scaleCurve; // Diseñala de 0 a 1 y luego a 0 (0 -> 1.2 -> 0)
    public float totalLife = 1.2f;

    [Header("Sounds")]  
    public AudioClip collectSound;

    private Vector3 velocity;
    private float timer;
    private bool isHoming;
    private Vector3 initialScale;

    void Awake()
    {
        initialScale = transform.localScale;
    }

    public void Play(Transform player)
    {

        //Debug.Log($"Coin spawned targeting player at {player.position}");
        target = player;
        timer = 0f;
        isHoming = false;
        currentSpeed = 0f;

        // Velocidad inicial explosiva
        velocity = new Vector3(
            Random.Range(-horizontalRandom, horizontalRandom),
            upForce,
            Random.Range(-horizontalRandom, horizontalRandom)
        );


        //transform.localScale = Vector3.zero; // Empieza invisible para el "pop"
    }

    void Update()
    {
        float dt = Time.deltaTime;
        timer += dt;

        // 1. Rotación estilo moneda
        transform.Rotate(Vector3.left * rotationSpeed * dt, Space.World);

        // 2. Animación de Escala (Usando tu Curve)
        // Tip: En el Inspector, haz que la curva empiece en 0, suba a 1.2 (overshoot) y termine en 0.
        float normalizedTime = Mathf.Clamp01(timer / totalLife);
        transform.localScale = initialScale * scaleCurve.Evaluate(normalizedTime);

        if (!isHoming)
        {
            // FASE ARCO: Física simple
            velocity.y -= gravity * dt;
            transform.position += velocity * dt;

            if (timer >= launchDuration) isHoming = true;
        }
        else
        {
            // FASE HOMING: Hacia el jugador
            if (target == null) { 
                Despawn(); return; 
            }

            currentSpeed += homingAcceleration * dt;
            Vector3 dir = (target.position - transform.position).normalized;

            // Suavizamos el giro hacia el jugador para que no sea lineal y aburrido
            velocity = Vector3.Lerp(velocity, dir * currentSpeed, dt * 5f);
            transform.position += velocity * dt;

            if (Vector3.Distance(transform.position, target.position) < 0.6f)
            {
                Collect();
            }
        }

        if (timer >= totalLife) Collect();
    }

    void Collect()
    {
        AudioSource.PlayClipAtPoint(collectSound, transform.position);
        FloatingTextPool.Instance.SpawnText($"+{1} coin", FloatingTextType.Resource);

        // Tu lógica de puntos aquí
        Despawn();
    }

    void Despawn()
    {
        CoinPool.Instance.Return(this);
    }
}