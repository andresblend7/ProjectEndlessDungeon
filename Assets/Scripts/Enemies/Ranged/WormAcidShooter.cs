using UnityEngine;

public class WormAcidShooter : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Transform shootPoint;
    public GameObject projectilePrefab;

    [Header("Detección")]
    public Vector3 detectionBox = new Vector3(8f, 3f, 10f); // ancho, alto, profundidad

    [Header("Trayectoria")]
    public float arcHeight = 3f;

    [Header("Disparo")]
    public float shootCooldown = 3f;
    public float shootAngleThreshold = 5f;

    [Header("Rotación")]
    public float rotationSpeed = 180f;

    float cooldownTimer;


    void Update()
    {
        if (player == null) return;

        if (!IsPlayerInsideDetectionBox())
            return;
    

        RotateTowardsPlayer();

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f && IsFacingPlayer())
        {
            Shoot();
            cooldownTimer = shootCooldown;
        }
    }

    bool IsPlayerInsideDetectionBox()
    {
        Vector3 localPos = transform.InverseTransformPoint(player.position);

        return
            Mathf.Abs(localPos.x) <= detectionBox.x * 0.5f &&
            Mathf.Abs(localPos.y) <= detectionBox.y * 0.5f &&
            Mathf.Abs(localPos.z) <= detectionBox.z * 0.5f;
    }


    void RotateTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    bool IsFacingPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        return angle < shootAngleThreshold;
    }

    void Shoot()
    {
        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        Rigidbody rb = proj.GetComponent<Rigidbody>();

        Vector3 target = player.position + Random.insideUnitSphere * 0.5f;
        Vector3 velocity = CalculateVelocity(shootPoint.position, target, arcHeight);

        rb.linearVelocity = velocity;
    }

    Vector3 CalculateVelocity(Vector3 start, Vector3 target, float height)
    {
        float gravity = Physics.gravity.y;

        Vector3 displacementY = Vector3.up * (target.y - start.y);
        Vector3 displacementXZ = new Vector3(target.x - start.x, 0, target.z - start.z);

        float timeUp = Mathf.Sqrt(-2 * height / gravity);
        float timeDown = Mathf.Sqrt(2 * (displacementY.y - height) / gravity);

        float totalTime = timeUp + timeDown;

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
        Vector3 velocityXZ = displacementXZ / totalTime;

        return velocityXZ + velocityY;
    }

     void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Matrix4x4 matrix = Matrix4x4.TRS(
            transform.position,
            transform.rotation,
            Vector3.one
        );

        Gizmos.matrix = matrix;

        Gizmos.DrawWireCube(
            Vector3.zero,
            detectionBox
        );
    }
}