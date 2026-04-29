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
    [SerializeField] private int damage = 10;

    private NavMeshAgent agent;

    private float lastAttackTime;
    private float repathTimer;

    private float sqrAttackRange;


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
        if (target == null || !agent.isOnNavMesh)
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
            TryAttack();
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

    private void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void Attack()
    {
        // Integrar con tu sistema de daño
        // target.GetComponent<PlayerHealth>()?.TakeDamage(damage);

        Debug.Log($"{name} atacó al jugador por {damage}");
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

   
}