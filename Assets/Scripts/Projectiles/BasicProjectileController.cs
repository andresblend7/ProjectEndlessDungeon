using UnityEngine;

public class BasicProjectileController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 14f;

    [Header("Lifetime")]
    [SerializeField] private float maxLifeTime = 3f;

    [Header("Impact")]
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private float impactEffectLifeTime = 1.5f;

    private bool hasCollided = false;
    private Rigidbody rb;
    private int actualRangeDamage = 0;

    private void Awake()
    {
    }

    private void Start()
    {


        // Autodestrucción si no colisiona
        Destroy(gameObject, maxLifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var damageCalculated = PlayerUtilities.Instance.GetDamageAfterCriticalCalculation(isRanged : true);

        if (hasCollided) return; // evita doble ejecución
        hasCollided = true;
        // Intentar aplicar daño
        IDamageable damageable = collision.collider.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage( TypeOfDamage.Range, damageCalculated.Damage, damageCalculated.IsCritical);
        }

        ShowImpactEffect(collision);

        // Destruir proyectil inmediatamente
        Destroy(gameObject);
    }

    private void ShowImpactEffect(Collision collision)
    {

        ContactPoint contact = collision.contacts[0];
        GameObject impact = Instantiate(
            impactEffectPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)
        );

        Destroy(impact, impactEffectLifeTime);
    }

    private void Update()
    {
        if (!hasCollided)
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        else
            rb.transform.Translate(Vector3.zero);
    }
}