using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemyBasicLogic : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════
    //  INSPECTOR — MOVIMIENTO
    // ═══════════════════════════════════════════════════════════

    [Header("── Movimiento ──────────────────────────────────────")]
    [Tooltip("Velocidad de desplazamiento en el plano XZ.")]
    public float moveSpeed = 3.5f;

    [Tooltip("Segundos de espera tras detectar al jugador antes de perseguirlo.")]
    public float chaseDelay = 0.5f;

    [Tooltip("Altura desde el pivot del enemigo donde se origina la visión (centro del cuerpo). " +
             "Ajústala al centro de tu modelo, ej: 0.9 para una cápsula de altura 1.8.")]
    public float eyeHeight = 0.9f;

    // ═══════════════════════════════════════════════════════════
    //  INSPECTOR — DETECCIÓN
    // ═══════════════════════════════════════════════════════════

    [Header("── Detección ───────────────────────────────────────")]
    [Tooltip("Radio máximo (en el plano XZ) al que el enemigo puede ver al jugador.")]
    public float detectionRange = 8f;


    // Tags que bloquean la visión — no en Inspector, son fijos por diseño
    private readonly string[] _visionBlockerTags = { "Obstacle", "Limits", "Enemy" };

    public float fieldOfViewAngle = 90f; // Ángulo total del cono (opcional)
    public bool useFieldOfView = true;

    [SerializeField]
    private bool playerDetected = false;
    private NavMeshAgent agent;

    // Optimización para el navmesh:
    private float repathTimer =0f;
    public float repathRate = 0.2f;

    // ═══════════════════════════════════════════════════════════
    //  INSPECTOR — ATAQUE
    // ═══════════════════════════════════════════════════════════

    [Header("── Ataque ──────────────────────────────────────────")]
    [Tooltip("Distancia XZ a la que puede golpear (ignora diferencia de altura).")]
    public float attackRange = 1f;

    [Tooltip("Duración en segundos del hitbox de ataque activo.")]
    public float attackDuration = 0.3f;

    [Tooltip("Tiempo de espera entre ataques consecutivos.")]
    public float attackCooldown = 1.2f;

    [Tooltip("GameObject hijo con el Collider trigger del hitbox de daño.")]
    public GameObject attackHitbox;

    // ═══════════════════════════════════════════════════════════
    //  INSPECTOR — STATS
    // ═══════════════════════════════════════════════════════════

    [Header("── Stats Base ──────────────────────────────────────")]
    public float maxHealth = 80f;
    public float defense = 5f;
    public float damage = 15f;
    [Range(0f, 1f)]
    public float criticalChance = 0.1f;
    public int coinDrop = 3;
    public bool isChasingEnemy = false;
    // ═══════════════════════════════════════════════════════════
    //  INSPECTOR — CONTEXT STEERING
    // ═══════════════════════════════════════════════════════════


    protected float _currentHealth;
    protected bool _canMove = true;
    private Transform player;

    // ═══════════════════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();


        // ── Estado inicial ─────────────────────────────────────
        _currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"[{name}] No se encontró un GameObject con tag 'Player'.");

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    private void Update()
    {
        playerDetected = CanSeePlayer();

        if (playerDetected)
        {
            repathTimer -= Time.deltaTime;

            if (repathTimer <= 0f)
            {
                agent.SetDestination(player.position);
                repathTimer = repathRate;
            }
        }
    }




    bool CanSeePlayer()
    {
        if (player == null) return false;

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


}
