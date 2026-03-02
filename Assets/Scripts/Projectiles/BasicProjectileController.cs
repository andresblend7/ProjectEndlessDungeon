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


        if (hasCollided) return; // evita doble ejecución
        hasCollided = true;

        Debug.Log($"Projectile collided with {collision.gameObject.name} at {collision.contacts[0].point}");
        // Instanciar efecto en punto de impacto
        if (impactEffectPrefab != null)
        {
            ContactPoint contact = collision.contacts[0];
            GameObject impact = Instantiate(
                impactEffectPrefab,
                contact.point,
                Quaternion.LookRotation(contact.normal)
            );

            Destroy(impact, impactEffectLifeTime);
        }

        // Destruir proyectil inmediatamente
        Destroy(gameObject);
    }
    private void Update()
    {
        if(!hasCollided)
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        else
            rb.transform.Translate(Vector3.zero);
    }
}