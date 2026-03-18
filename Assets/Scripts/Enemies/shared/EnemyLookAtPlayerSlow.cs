using UnityEngine;

public class EnemyLookAtPlayerSlow : MonoBehaviour
{
    private Transform player;
    public float rotationSpeed = 90f; // grados por segundo

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

    }
    public void RotateTowardsPlayer()
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
}
