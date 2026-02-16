using System.Collections;
using UnityEngine;

public class MeleeEnemyBasicLogic : MonoBehaviour
{

    [Header("Grid")]
    public float laneSize = 1f;
    public float moveSpeed = 2.5f;

    [Header("Detection")]
    public int detectionRangeZ = 8;
    public int detectionRangeX = 3;

    [Header("Attack")]
    public float attackRange = 1.01f;
    public float attackDuration = 0.3f;

    [Header("ColisionBounds")]
    public GameObject collisionMeleeAttack;

    [Header("stats")]
    public int health = 3;

    Transform player;

    [SerializeField]
    bool isMoving;
    [SerializeField]
    bool canMove = true;
    [SerializeField]
    bool enemyIsInrangedAttack = true;
    Vector3 targetPos;
    Vector3 lastGridPos;

    [SerializeField]
    private bool touchedByRayCast = false;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        targetPos = transform.position;
        lastGridPos = transform.position;

        collisionMeleeAttack.SetActive(false);

        Debug.Log("MeleeEnemyBasicLogic: Enemy initialized and ready to detect the player.");
    }

    void Update()
    {
        // Seguridad: nunca compartir casilla con jugador
        if (IsSameTileAsPlayer())
        {
            transform.position = lastGridPos;
            targetPos = lastGridPos;
            isMoving = false;
            return;
        }

        if (isMoving)
        {
            MoveStep();
            return;
        }

        if (!IsPlayerInRange())
            return;

        if (canMove)
            TryMoveTowardPlayer();

        if (IsPlayerInAttackRange() && canMove)
            transform.LookAt(player.position);
    }

    bool IsSameTileAsPlayer()
    {
        return Vector3.Distance(transform.position, player.position) < laneSize * 0.5f;
    }

    public bool IsPlayerInRange()
    {
        float dz = Mathf.Abs(player.position.z - transform.position.z);
        float dx = Mathf.Abs(player.position.x - transform.position.x);


        var atRange = dz <= detectionRangeZ && dx <= detectionRangeX;

        if (atRange && !touchedByRayCast)
        {
            touchedByRayCast = HasLineOfSightToPlayer();
            return false;
        }  

        return atRange;
    }

    bool HasLineOfSightToPlayer()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        Vector3 target = player.position;
        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);

        // 1. Dibujar el rayo en la escena para depurar (solo en modo Editor)
        Debug.DrawRay(origin, dir * dist, Color.red);


        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
        {
            if (hit.transform == player)
                return true;

            if (hit.transform.CompareTag("Obstacle") || hit.transform.CompareTag("Limits"))
                return false;
        }

        return false;
    }

    void TryMoveTowardPlayer()
    {
        lastGridPos = transform.position;

        Vector3 e = transform.position;
        Vector3 p = player.position;

        float dx = p.x - e.x;
        float dz = p.z - e.z;

        // Si ya está a 1 tile → no avanzar
        if (Mathf.Abs(dx) < 0.1f && Mathf.Abs(dz) <= attackRange) return;
        if (Mathf.Abs(dz) < 0.1f && Mathf.Abs(dx) <= attackRange) return;

        Vector3 primaryDir;
        Vector3 secondaryDir;

        if (Mathf.Abs(dx) > Mathf.Abs(dz))
        {
            primaryDir = dx > 0 ? Vector3.right : Vector3.left;
            secondaryDir = dz > 0 ? Vector3.forward : Vector3.back;
        }
        else
        {
            primaryDir = dz > 0 ? Vector3.forward : Vector3.back;
            secondaryDir = dx > 0 ? Vector3.right : Vector3.left;
        }

        // Primero intenta ir por el eje principal
        if (CanMove(primaryDir))
        {
            Move(primaryDir);
            return;
        }

        // Si está bloqueado, intenta eje secundario
        if (CanMove(secondaryDir))
        {
            Move(secondaryDir);
            return;
        }

        // Si ambos están bloqueados → no moverse
    }

    bool CanMove(Vector3 dir)
    {
        Vector3 target = transform.position + dir * laneSize;

        float radius = laneSize * 0.4f;

        Collider[] hits = Physics.OverlapSphere(target, radius);

        foreach (var h in hits)
        {
            if (h.CompareTag("Obstacle") || h.CompareTag("Limits") || (h.CompareTag("Enemy")))
                return false;
        }

        return true;
    }

    void Move(Vector3 dir)
    {
        targetPos = transform.position + dir * laneSize;
        isMoving = true;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    void MoveStep()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            transform.position = targetPos;
            isMoving = false;
        }
    }


    public virtual void Attack(int damage)
    {
        StartCoroutine(ActiveAttackHitBox());
        //Debug.Log("Ejecutando acción: " + actionSelected);
        PlayerUtilities.Instance.ApplyDamageToPlayer(damage);
        //Debug.Log("MeleeEnemyBasicLogic: Attacking the player!");
    }
    private IEnumerator ActiveAttackHitBox()
    {
        //yield return new WaitForSeconds(0.15f);
        collisionMeleeAttack.SetActive(true);
        yield return new WaitForSeconds(attackDuration); // Duración del hitbox activo
        collisionMeleeAttack.SetActive(false);

    }


    #region mensajes de los enemigos HACIA la lógica básica
    public void EnableDisableMovement(bool enable)
    {
        this.canMove = enable;
    }
    #endregion

    #region Consulta de los enemigos a la lógica básica
    public bool IsMoving()
    {
        return isMoving;
    }

    /// <summary>
    //´TODO: Comprobar si en diagonal se activa
    /// </summary>
    /// <returns></returns>
    public bool IsPlayerInAttackRange()
    {


        Vector3 p = player.position;
        Vector3 e = transform.position;

        // Ignoramos la altura (eje Y)
        float dx = Mathf.Abs(p.x - e.x);
        float dz = Mathf.Abs(p.z - e.z);

        if (dx > 0.96 && dx<0.99999999)
            dx = 1;


        if (dz > 0.96 && dz < 0.99999999)
            dz = 1;

        bool esAdyacente = (dx == 1 && dz == 0) || (dx == 0 && dz == 1);

        return esAdyacente;
    }

    #endregion


    #region colisiones  
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("AttackPlayer"))
        {
            Debug.Log("AAtaque detectado");
            TakeDamage();
        }
    }
    private void TakeDamage()
    {
        health--;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
        // Tu lógica de daño aquí
    }
    #endregion

}
