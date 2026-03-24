using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    public List<Vector3> patrolPoints = new List<Vector3>();

    [Header("Settings")]
    public float waitTime = 1.5f;
    public float moveSpeed = 3.5f;
    public bool useEaseInOut = true;

    [Header("Rotation Control")]
    public bool rotateBeforeMove = true;
    public float rotationSpeed = 5f;
    public float rotationThreshold = 5f; // grados

    private NavMeshAgent agent;

    private int currentIndex = 0;
    private float waitTimer;
    private bool waiting = false;
    private bool isPatrolling = false;

    private bool isRotating = false;
    private Vector3 nextTarget;

    private Vector3 origin;
    private bool goingBack = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // CLAVE
    }

    void Start()
    {
        origin = transform.position;
        agent.speed = moveSpeed;

        if (patrolPoints.Count > 0)
            StartPatrol();
    }

    void Update()
    {
        if (!isPatrolling || patrolPoints.Count == 0) return;

        if (isRotating)
        {
            HandleRotation();
            return;
        }

        HandleEase();

        if (!waiting)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                waiting = true;
                waitTimer = waitTime;

                if (useEaseInOut)
                    agent.isStopped = true;
            }
        }
        else
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                PrepareNextPoint();
                waiting = false;
            }
        }
    }

    // =========================

    void PrepareNextPoint()
    {
        nextTarget = GetNextPoint();

        if (rotateBeforeMove)
        {
            isRotating = true;
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(nextTarget);
        }
    }

    void HandleRotation()
    {
        Vector3 dir = (nextTarget - transform.position).normalized;
        dir.y = 0;

        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        float angle = Quaternion.Angle(transform.rotation, targetRot);

        if (angle <= rotationThreshold)
        {
            isRotating = false;
            agent.isStopped = false;
            agent.SetDestination(nextTarget);
        }
    }

    Vector3 GetNextPoint()
    {
        if (patrolPoints.Count == 1)
        {
            if (!goingBack)
            {
                goingBack = true;
                return patrolPoints[0];
            }
            else
            {
                goingBack = false;
                return origin;
            }
        }

        currentIndex = (currentIndex + 1) % patrolPoints.Count;
        return patrolPoints[currentIndex];
    }

    void HandleEase()
    {
        if (!useEaseInOut) return;

        float dist = agent.remainingDistance;

        if (dist < 2f)
            agent.speed = Mathf.Lerp(0.5f, moveSpeed, dist / 2f);
        else
            agent.speed = moveSpeed;
    }

    // =========================
    // PUBLIC API
    // =========================

    public void StartPatrol()
    {
        isPatrolling = true;
        currentIndex = 0;
        agent.isStopped = false;

        agent.SetDestination(patrolPoints[currentIndex]);
    }

    public void StopPatrol()
    {
        isPatrolling = false;
        agent.isStopped = true;
    }
}