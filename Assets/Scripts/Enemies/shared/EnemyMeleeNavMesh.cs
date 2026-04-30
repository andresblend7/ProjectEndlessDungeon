using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMeleeNavMesh : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform target;

    [Header("NavMesh")]
    [SerializeField] private float moveSpeed = 1.6f;
    [SerializeField] private float stoppingDistance = 1.3f;
    [SerializeField] private float repathRate = 0.2f; // optimización

    [Header("Combate")]
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float attackCooldown = 1.2f;

    private NavMeshAgent agent;

    private float lastAttackTime;
    private float repathTimer;

    private float sqrAttackRange;

    private bool isPaused = false;

    // Evento que indica si el jugador está dentro del rango de ataque, para que el hijo pueda decidir qué hacer (ej: activar hitbox de ataque)
    public event Action<bool> OnPlayerInAttackRange;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        sqrAttackRange = attackRange * attackRange;

        // Config inicial (evita hacerlo en runtime cada frame)
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = false; // rotamos manual para control

        agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        agent.radius = 0.2f;
    }

    private void OnEnable()
    {
        lastAttackTime = 0f;
        repathTimer = 0f;

        if (target == null)
            target = FindPlayer();

        if (agent.isOnNavMesh)
            agent.isStopped = false;

   
    }

    private void Update()
    {
        if (target == null || !agent.isOnNavMesh || isPaused)
            return;

        repathTimer -= Time.deltaTime;

        // Recalcular path cada cierto tiempo (clave en hordas)
        if (repathTimer <= 0f)
        {
            agent.SetDestination(target.position);
            repathTimer = repathRate;
        }

        float sqrDistance = (target.position - transform.position).sqrMagnitude;

        if (sqrDistance <= sqrAttackRange)
        {
            agent.isStopped = true;
            PlayerInrange();
        }
        else
        {
            agent.isStopped = false;
        }

        RotateTowardsVelocity();
    }

    private void RotateTowardsVelocity()
    {
        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
        }
    }

    private void PlayerInrange()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            OnPlayerInAttackRange?.Invoke(true);
            lastAttackTime = Time.time;
        }
    }

    private Transform FindPlayer()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        return go ? go.transform : null;
    }

    private void OnDisable()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetPaused(bool pause)
    {
        isPaused = pause;

        if (agent == null || !agent.isOnNavMesh)
            return;

        if (pause)
        {
            agent.isStopped = true;
            agent.ResetPath(); // importante para que no conserve rutas viejas
        }
        else
        {
            agent.isStopped = false;

            if (target != null)
                agent.SetDestination(target.position); // retoma inmediatamente
        }
    }

    public void ForceMove(Vector3 target)
    {
        agent.Move(target); //  clave aquí
    }
  
}