using System.Collections;
using UnityEngine;

public class GigantSlime : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 500;
    public int currentHealth;
    public int damage = 20;

    [Header("References")]
    public GameObject hitJumpMarker;
    public EnemySqashEfect enemySqashEfect;
    public EnemyFlashEffect enemyFlashEffect;
    public float markerWarningTime = 0.8f;
    //Player    
    private Transform player;
    private PlayerController playerController;


    [Header("Behaviour Jump Attack Settings")]
    public int jumpsPerAttack = 3;
    public float timeBetweenJumbs = 0.5f;
    public float jumpHeight = 6f;
    public float jumpDuration = 1.2f;
    public float landingRadius = 2f;
    private float HeightBase = 0f;

    private bool isPerformingAction;

    //STATES
    [SerializeField]
    private bool lookAtPlayer = true;

    private void Awake()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj.transform;
        playerController = playerObj.GetComponent<PlayerController>();
        HeightBase = transform.position.y;
    }

    private void Start()
    {
        currentHealth = maxHealth;
     
    }


    private void Update()
    {
        if (lookAtPlayer){
            //Mantener el mismo nivel de altura para mirar al jugador
            Vector3 targetPosition = player.position;
            targetPosition.y = transform.position.y;
            transform.LookAt(targetPosition);
        }
    }

    void DecideBehaviour()
    {
        if (isPerformingAction)
            return;

        float healthPercent = (float)currentHealth / maxHealth;

        if (healthPercent > 0.75f)
        {
            StartCoroutine(SlimeJumpAttack());
        }
        else if (healthPercent > 0.50f)
        {
            StartCoroutine(SlimeJumpAttack());
        }
        else if (healthPercent > 0.25f)
        {
            StartCoroutine(SlimeJumpAttack());
        }
        else
        {
            StartCoroutine(SlimeJumpAttack());
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    IEnumerator SlimeJumpAttack()
    {
        isPerformingAction = true;

        for (int i = 0; i < jumpsPerAttack; i++)
        {
            yield return StartCoroutine(JumpTowardsPlayer());
            yield return new WaitForSeconds(timeBetweenJumbs);
        }

        isPerformingAction = false;
    }

    IEnumerator JumpTowardsPlayer()
    {
        Vector3 start = transform.position;
        Vector3 target = new Vector3(player.position.x, HeightBase, player.position.z);

        // Crear indicador en el suelo
        GameObject markerObj = Instantiate(hitJumpMarker, start, Quaternion.identity);
        HitJumpMarker marker = markerObj.GetComponent<HitJumpMarker>();

        marker.boss = transform;
        marker.maxHeight = jumpHeight;

        float timer = 0f;
        lookAtPlayer = false;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;

            float progress = timer / jumpDuration;

            Vector3 horizontal = Vector3.Lerp(start, target, progress);

            float height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;

            transform.position = horizontal + Vector3.up * height;

            yield return null;
        }

        transform.position = target;

        // Destruir marcador al aterrizar
        Destroy(markerObj);
        StartCoroutine(LookAtOverTime(player, timeBetweenJumbs));
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            playerController.ProcessDamageToPlayer(new DamageToPlayer
            {
                baseDamageAmount = damage,
                typeOfDamage = TypeOfDamage.Melee
            });
        }
    }

    public void TakeDamage(TypeOfDamage typeOfDamage, int amount)
    {
        if (enemyFlashEffect != null)
        {
            enemyFlashEffect.Flash();
        }
        if (enemySqashEfect != null)
        {
            enemySqashEfect.PlaySquash();
        }
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        DecideBehaviour();
    }


    public IEnumerator LookAtOverTime(Transform target, float duration)
    {
        // 1. Guardar rotación inicial y calcular rotación objetivo
        Quaternion startRotation = transform.rotation;

        Vector3 targetPosition = target.position;
        targetPosition.y = transform.position.y; // Solo horizontal
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

        // 2. Calcular el ángulo total a rotar
        float totalAngle = Quaternion.Angle(startRotation, targetRotation);

        // 3. Si no hay ángulo que rotar, salir
        if (totalAngle <= 0.01f)
            yield break;

        // 4. Calcular velocidad angular necesaria (grados por segundo)
        float angularSpeed = totalAngle / duration;

        // 5. Temporizador
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 6. Rotar hacia el objetivo con la velocidad calculada
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                angularSpeed * Time.deltaTime
            );

            yield return null;
        }

        // 7. Asegurar rotación final exacta
        transform.rotation = targetRotation;
        lookAtPlayer = true;

    }
}
