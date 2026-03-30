using System.Collections;
using UnityEngine;

public class AcidProjectile : MonoBehaviour
{
    [Header("Impacto")]
    public GameObject acidPoolPrefab;
    private AcidSplash acidSplashScript;

    [Header("Daño")]
    public int baseDamageAmount = 4;

    [Header("Configuración")]
    public LayerMask floorLayer;
    public float lifeTime = 10f;

    Rigidbody rb;

    private void Awake()
    {
        acidSplashScript = acidPoolPrefab != null ? acidPoolPrefab.GetComponent<AcidSplash>() : null;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        AlignWithVelocity();
    }

    void AlignWithVelocity()
    {
        if (rb == null) return;

        Vector3 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & floorLayer) != 0)
        {
            ContactPoint contact = collision.contacts[0];

            if (acidPoolPrefab != null)
            {
                Quaternion randomY = Quaternion.Euler(
                    0,
                    Random.Range(0f, 360f),
                    0
                );

                Instantiate(
                    acidPoolPrefab,
                    contact.point + contact.normal * 0.02f,
                    randomY
                );
            }

            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(HitEnemy(collision.gameObject.GetComponent<PlayerController>()));
        }

    }
    public IEnumerator HitEnemy(PlayerController playerController)
    {
        playerController.ProcessDamageToPlayer(new DamageToPlayer
        {
            typeOfDamage = TypeOfDamage.Range,
            baseDamageAmount = baseDamageAmount,
            isCriticalHit = true
        });

        yield return new WaitForSeconds(0.3f);

        playerController.ProcessDamageToPlayer(new DamageToPlayer
        {
            typeOfDamage = TypeOfDamage.TickOverTime,
            baseDamageAmount = acidSplashScript.damagePerTick,
            tickInterval = acidSplashScript.tickInterval,
            duration = acidSplashScript.damageDuration,
            typeOfTickDamage = TypeOfTickDamage.Poison
        });
        Destroy(gameObject);
    }
}