using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemyBasicLogic : MonoBehaviour, IDamageable
{
    //  ----------------------------------------------  INSPECTOR — MOVIMIENTO ---------------------------------------------- 

    [Header("── Movimiento ──────────────────────────────────────")]
    [Tooltip("Velocidad de desplazamiento en el plano XZ.")]
    public float moveSpeed = 3.5f;

    [Tooltip("Segundos de espera tras detectar al jugador antes de perseguirlo.")]
    public float chaseDelay = 0.5f;

    [Tooltip("Altura desde el pivot del enemigo donde se origina la visión (centro del cuerpo). " +
             "Ajústala al centro de tu modelo, ej: 0.9 para una cápsula de altura 1.8.")]
    public float eyeHeight = 0.9f;

    //  ---------------------------------------------- INSPECTOR — DETECCIÓN ---------------------------------------------- 

    [Header("── Detección ───────────────────────────────────────")]
    [Tooltip("Radio máximo (en el plano XZ) al que el enemigo puede ver al jugador.")]
    public float detectionRange = 8f;


    // Tags que bloquean la visión — no en Inspector, son fijos por diseño
    private readonly string[] _visionBlockerTags = { "Obstacle", "Limits", "Enemy" };

    public float fieldOfViewAngle = 90f; // Ángulo total del cono (opcional)
    public bool useFieldOfView = true;

    [SerializeField]
    private bool playerDetected = false;
    private bool wasDamagedByPlayer = false;
    private NavMeshAgent agent;

    // Optimización para el navmesh:
    private float repathTimer = 0f;
    public float repathRate = 0.2f;

    // ----------------------------------------------  INSPECTOR — ATAQUE ---------------------------------------------- 

    [Header("── Ataque ──────────────────────────────────────────")]
    [Tooltip("Distancia XZ a la que puede golpear (ignora diferencia de altura).")]
    public float attackRange = 1f;

    [Tooltip("Tiempo de espera entre ataques consecutivos.")]
    public float attackCooldown = 1.2f;

    [Tooltip("GameObject hijo con el Collider trigger del hitbox de daño.")]
    public GameObject attackHitbox;


    // ----------------------------------------------  INSPECTOR — STATS ---------------------------------------------- 

    [Header("── Stats Base ──────────────────────────────────────")]
    public float maxHealth = 3f;
    public float defense = 5f;
    public int damage = 15;
    [Range(0f, 1f)]
    public float criticalChance = 0.1f;
    public int coinDrop = 3;
    [SerializeField]
    private bool canChasePlayer = false;
    public bool isChasingEnemy = false;
    public TMPro.TextMeshPro txtDamage;
    //public bool isPlayerReached = false;

    // ----------------------------------------------  INSPECTOR — Effects ---------------------------------------------- 

    [Header("── Effects  ──────────────────────────────────────")]
    // Knockback
    public bool canBeNockbacked = true;
    public float knockbackForce = 6f;
    private bool isKnockbackActive = false;
    //Squash
    public bool haveSquashEffect = true;
    private Vector3 originalScale;
    //text damage
    public float heightTextDamage = 1f;



    // ----------------------------------------------  INSPECTOR — CONTEXT STEERING ---------------------------------

    protected float _currentHealth;
    protected bool _canMove = true;
    private Transform player;
    private PlayerController playerController;


    // ---------------------------------------------- EVENTS TO CHILDRENS -------------------------------------------
    // evento que dice si el player está en el rango de visión o no, para que el hijo pueda decidir qué hacer (ej: empezar a perseguirlo)
    public event Action OnPlayerDetected;
    // Evento que indica si el jugador está dentro del rango de ataque, para que el hijo pueda decidir qué hacer (ej: activar hitbox de ataque)
    public event Action<bool> OnPlayerInAttackRange;
    public event Action<int> OnReceibeDamage;




    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
       

        // ── Estado inicial ─────────────────────────────────────
        _currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            player = playerObj.transform;
        }
        else
            Debug.LogWarning($"[{name}] No se encontró un GameObject con tag 'Player'.");

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        // EFFECTS
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (isKnockbackActive)
            return;

        playerDetected = CanSeePlayer();

        if (playerDetected)
        {
            OnPlayerDetected.Invoke();
        }

        if (canChasePlayer)
        {

            // Check if the agent has stopped based on stopping distance
            if (HasReachedDestination())
            {
                //isPlayerReached = true;
                OnPlayerInAttackRange.Invoke(true);
                //Debug.Log("Agent has reached its destination and stopped!!!!!!!!!!!!!!");
                // Add your custom logic here (e.g., trigger an animation, change state, etc.)
            }

            //if (playerDetected) // Este if se comenta porque el enemigo puede marcar como seguir siempre al jugador una vez lo ve por primera vez
            //{
            repathTimer -= Time.deltaTime;

            if (repathTimer <= 0f)
            {
                agent.SetDestination(player.position);
                repathTimer = repathRate;
            }
            //}
        }



    }

    #region movimiento
    private bool HasReachedDestination()
    {
        // First, check if the agent is enabled and has a path
        if (!agent.enabled || !agent.hasPath)
        {
            return false;
        }

        // Check if the path is still being calculated
        if (agent.pathPending)
        {
            return false;
        }

        // The primary condition: remainingDistance should be less than or equal to stoppingDistance
        // A small buffer might be needed for floating point inaccuracies
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // Additionally, check if the agent is actually moving or has zero velocity
            // This prevents false positives when the agent is initially at the destination
            if (agent.velocity.sqrMagnitude == 0f || agent.isStopped)
            {
                // Ensure the path status is complete
                if (agent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    return true;
                }
            }
        }

        return false;
    }
    bool CanSeePlayer()
    {
        if (player == null) return false;

        if(wasDamagedByPlayer)
            return true;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 1. Verificar distancia
        if (distanceToPlayer > detectionRange)
            return false;

        // 2. Verificar ángulo (campo de visión en cono)
        if (useFieldOfView)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle > fieldOfViewAngle / 2f)
                return false;
        }

        // 3. Raycast: verificar que no haya obstáculos en el camino
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, detectionRange))
        {
            if (hit.collider.CompareTag("Obstacle"))
                return false; // Hay un obstáculo bloqueando la visión

            if (hit.collider.CompareTag("Player"))
                return true; // El primer objeto golpeado es el jugador
        }

        return false;
    }

    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        if (isKnockbackActive) return;

        StartCoroutine(KnockbackRoutine(direction, force, duration));
    }

    IEnumerator KnockbackRoutine(Vector3 direction, float force, float duration)
    {
        isKnockbackActive = true;

        float timer = 0f;

        direction.y = 0f;
        direction.Normalize();

        while (timer < duration)
        {
            float t = timer / duration;

            // curva de desaceleración suave
            float currentForce = Mathf.Lerp(force, 0f, t);

            Vector3 move = direction * currentForce * Time.deltaTime;

            agent.Move(move); // 🔥 clave aquí

            timer += Time.deltaTime;
            yield return null;
        }

        isKnockbackActive = false;
    }

    public void PlaySquash(float intensity = 0.2f, float duration = 0.1f)
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
    #endregion

    #region Métodos para que el hijo dispare métodos de este padre

    public void CanChasePlayer(bool canChase)
    {
        if (canChase)
            agent.ResetPath(); // Limpia el path para que el enemigo deje de intentar llegar al jugador


        this.canChasePlayer = canChase;
    }

    public void EnableDisableAttackHitBox(bool enable)
    {
        attackHitbox.SetActive(enable);
    }

    public void LookAtPlayer()
    {
        //rotar hacia el jugador
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    // Método que se llama desde el hitbox de ataque cuando detecta que golpeó al jugador, para que el padre pueda decidir qué hacer (ej: mostrar daño, aplicar knockback, etc)
    public void ApplyDamageToPlayer()
    {
        playerController.ProcessDamageToPlayer(TypeOfDamage.Melee, damage);
    }

    #endregion

    // Visualización en el editor (Gizmos)
    void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Cono de visión
        if (useFieldOfView)
        {
            Gizmos.color = Color.cyan;
            Vector3 leftBound = Quaternion.Euler(0, -fieldOfViewAngle / 2f, 0) * transform.forward;
            Vector3 rightBound = Quaternion.Euler(0, fieldOfViewAngle / 2f, 0) * transform.forward;
            Gizmos.DrawRay(transform.position, leftBound * detectionRange);
            Gizmos.DrawRay(transform.position, rightBound * detectionRange);
        }

        // Línea hacia el jugador (rojo = bloqueado, verde = visible)
        if (player != null)
        {
            Gizmos.color = playerDetected ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }

    public void TakeDamage(TypeOfDamage typeOfDamage, int amount)
    {

        //Si no ve al enemigo pero recibe daño lo marca como detectado para que empiece a perseguirlo
        if (!playerDetected)
            wasDamagedByPlayer = true;

        int damage = amount;

        DamageNumberSpawner.Spawn(
            transform.position + Vector3.up * heightTextDamage,
            damage,
            false,
            false
        );
        //txtDamage.text = $"{damage}";
        OnReceibeDamage.Invoke(damage);

        if (canBeNockbacked)
        {
            Vector3 dir = transform.position - player.transform.position;
            ApplyKnockback(dir, knockbackForce, 0.15f);
        }

        if (haveSquashEffect)
        {
            PlaySquash();
        }



        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        gameObject.SetActive(false);
    }

}
