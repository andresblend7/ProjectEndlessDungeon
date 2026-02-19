using System.Collections;
using UnityEngine;

public class MeleeEnemyBasicLogic : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════
    //  INSPECTOR — MOVIMIENTO
    // ═══════════════════════════════════════════════════════════

    [Header("── Movimiento ──────────────────────────────────────")]
    [Tooltip("Velocidad de desplazamiento en el plano XZ.")]
    public float moveSpeed = 3.5f;

    [Tooltip("Segundos de espera tras detectar al jugador antes de perseguirlo.")]
    public float chaseDelay = 0.5f;

    [Tooltip("Altura desde el pivot del enemigo donde se origina la visión (centro del cuerpo). " +
             "Ajústala al centro de tu modelo, ej: 0.9 para una cápsula de altura 1.8.")]
    public float eyeHeight = 0.9f;

    // ═══════════════════════════════════════════════════════════
    //  INSPECTOR — DETECCIÓN
    // ═══════════════════════════════════════════════════════════

    [Header("── Detección ───────────────────────────────────────")]
    [Tooltip("Radio máximo (en el plano XZ) al que el enemigo puede ver al jugador.")]
    public float detectionRange = 8f;

    [Tooltip("Si es true, el enemigo recuerda la última posición del jugador al perderlo de vista " +
             "y continúa persiguiéndolo. Si es false, se detiene en cuanto pierde línea de visión.")]
    public bool rememberPlayerPosition = true;

    // Tags que bloquean la visión — no en Inspector, son fijos por diseño
    private readonly string[] _visionBlockerTags = { "Obstacle", "Limits", "Enemy" };

    // ═══════════════════════════════════════════════════════════
    //  INSPECTOR — ATAQUE
    // ═══════════════════════════════════════════════════════════

    [Header("── Ataque ──────────────────────────────────────────")]
    [Tooltip("Distancia XZ a la que puede golpear (ignora diferencia de altura).")]
    public float attackRange = 1.2f;

    [Tooltip("Duración en segundos del hitbox activo.")]
    public float attackDuration = 0.3f;

    [Tooltip("Tiempo de espera entre ataques consecutivos.")]
    public float attackCooldown = 1.2f;

    [Tooltip("GameObject hijo con el Collider trigger del hitbox de daño.")]
    public GameObject attackHitbox;

    // ═══════════════════════════════════════════════════════════
    //  INSPECTOR — STATS
    // ═══════════════════════════════════════════════════════════

    [Header("── Stats Base ──────────────────────────────────────")]
    public float maxHealth = 80f;
    public float defense = 5f;
    public float damage = 15f;
    [Range(0f, 1f)]
    public float criticalChance = 0.1f;
    public int coinDrop = 3;

    // ═══════════════════════════════════════════════════════════
    //  INSPECTOR — CONTEXT STEERING
    // ═══════════════════════════════════════════════════════════

    [Header("── Evasión de obstáculos (Context Steering) ────────")]
    [Tooltip("Número de rayos del abanico. Más rayos = más preciso pero más costoso. Recomendado: 8-16.")]
    public int steeringRays = 12;

    [Tooltip("Ángulo total del abanico en grados. 360 = omnidireccional.")]
    [Range(90f, 360f)]
    public float steeringAngle = 240f;

    [Tooltip("Longitud de cada rayo. Debe ser mayor que el radio del collider del enemigo.")]
    public float steeringRayLength = 1.6f;

    [Tooltip("Peso de la evasión sobre la dirección directa al jugador.\n" +
             "0 = ignora obstáculos, 1 = máxima evasión.")]
    [Range(0f, 1f)]
    public float steeringWeight = 0.75f;

    // ═══════════════════════════════════════════════════════════
    //  PRIVATE / PROTECTED STATE
    // ═══════════════════════════════════════════════════════════

    protected float _currentHealth;
    protected bool _canMove = true;
    protected bool _isDead = false;
    protected Transform _player;
    protected Rigidbody _rb;

    private bool _playerDetected = false;
    private bool _chaseStarted = false;
    private bool _isAttacking = false;
    private float _attackCooldownTimer = 0f;
    private LayerMask _obstacleLayerMask;

    // ═══════════════════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // ── Rigidbody 3D top-down ──────────────────────────────
        // NO congelamos Y: la gravedad debe funcionar para que el enemigo
        // se mantenga pegado al suelo. Solo congelamos rotación para que
        // el enemigo no se vuelque al chocar con obstáculos.
        _rb.freezeRotation = true;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        // ── LayerMask para raycast ─────────────────────────────
        // Excluimos Player e Ignore Raycast. Los rayos de steering y visión
        // solo deben colisionar con geometría del mundo (obstáculos, suelo, etc).
        _obstacleLayerMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

        // ── Estado inicial ─────────────────────────────────────
        _currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
        else
            Debug.LogWarning($"[{name}] No se encontró un GameObject con tag 'Player'.");

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    protected virtual void Update()
    {
        if (_isDead || _player == null) return;

        _attackCooldownTimer -= Time.deltaTime;

        DetectPlayer();
        HandleAttack();
    }

    protected virtual void FixedUpdate()
    {
        if (_isDead || _player == null || !_canMove || !_chaseStarted) return;
        if (PlayerIsInRangeToAttack()) { StopHorizontalMovement(); return; }

        MoveTowardsPlayer();
    }

    // ═══════════════════════════════════════════════════════════
    //  DETECCIÓN DE JUGADOR
    // ═══════════════════════════════════════════════════════════

    private void DetectPlayer()
    {
        // Distancia horizontal (XZ) — ignoramos diferencia de altura
        float distXZ = HorizontalDistance(transform.position, _player.position);

        if (distXZ > detectionRange)
        {
            if (!rememberPlayerPosition)
            {
                _playerDetected = false;
                _chaseStarted = false;
            }
            return;
        }

        if (HasLineOfSight())
        {
            if (!_playerDetected)
            {
                _playerDetected = true;
                StartCoroutine(ChaseAfterDelay());
            }
        }
        else if (!rememberPlayerPosition)
        {
            // Perdió línea de visión y no recuerda al jugador → se detiene
            _chaseStarted = false;
        }
    }

    /// <summary>
    /// Raycast desde el pecho del enemigo hacia el pecho del jugador.
    /// Retorna true si no hay ningún objeto bloqueante entre ambos.
    /// </summary>
    private bool HasLineOfSight()
    {
        // Usamos eyeHeight para salir desde el centro del cuerpo, no desde los pies.
        // Esto evita que el rayo golpee el suelo o las paredes en la base del enemigo.
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 playerChest = _player.position + Vector3.up * eyeHeight;
        Vector3 direction = (playerChest - origin).normalized;
        float distance = Vector3.Distance(origin, playerChest);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, _obstacleLayerMask))
            return !IsBlockerTag(hit.collider.tag); // golpeó algo: ¿es bloqueante?

        return true; // camino libre
    }

    private IEnumerator ChaseAfterDelay()
    {
        OnPlayerDetected();
        yield return new WaitForSeconds(chaseDelay);
        _chaseStarted = true;
        OnChaseStarted();
    }

    // ═══════════════════════════════════════════════════════════
    //  MOVIMIENTO 3D (plano XZ) + CONTEXT STEERING
    // ═══════════════════════════════════════════════════════════

    private void MoveTowardsPlayer()
    {
        // Dirección al jugador APLANADA al plano XZ (Y = 0)
        // Esto es crítico en 3D: sin aplanar, el enemigo intentaría moverse
        // hacia arriba/abajo si hay diferencia de altura con el jugador.
        Vector3 toPlayer = _player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.001f) return;
        Vector3 desiredDir = toPlayer.normalized;

        // Context steering para rodear obstáculos dinámicamente
        Vector3 steerDir = ComputeSteeringDirection(desiredDir);

        // Aplicamos velocidad horizontal preservando la Y actual (gravedad/caída)
        float gravityY = _rb.velocity.y;
        Vector3 newVelocity = steerDir * moveSpeed;
        newVelocity.y = gravityY;            // ← preserva gravedad
        _rb.velocity = newVelocity;

        // Rotación suave hacia la dirección de movimiento (solo eje Y)
        if (steerDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(
                new Vector3(steerDir.x, 0f, steerDir.z), Vector3.up
            );
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.fixedDeltaTime);
        }
    }

    private void StopHorizontalMovement()
    {
        // Detiene XZ pero preserva Y (gravedad) para no "flotar"
        _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
    }

    /// <summary>
    /// Context Steering en el plano XZ.
    ///
    /// Lanza N rayos en abanico alrededor de la dirección deseada.
    /// Cada rayo libre suma puntos según cuánto se alinea con el objetivo.
    /// Los rayos bloqueados reciben penalización fuerte.
    /// Devuelve la dirección horizontal más libre y más alineada con el jugador.
    /// </summary>
    private Vector3 ComputeSteeringDirection(Vector3 desiredDirXZ)
    {
        Vector3 bestDir = desiredDirXZ;
        float bestScore = float.MinValue;

        float halfAngle = steeringAngle * 0.5f;
        float step = steeringRays > 1 ? steeringAngle / (steeringRays - 1) : 0f;

        // El origen de los rayos de steering está al nivel del pecho
        Vector3 rayOrigin = transform.position + Vector3.up * eyeHeight;

        for (int i = 0; i < steeringRays; i++)
        {
            float angle = -halfAngle + step * i;

            // Rotación en el plano XZ (eje Y mundial)
            Vector3 rayDir = Quaternion.AngleAxis(angle, Vector3.up) * desiredDirXZ;

            bool blocked = Physics.Raycast(rayOrigin, rayDir, steeringRayLength, _obstacleLayerMask);

            // Score: cuánto se alinea con el destino; penalizado si está bloqueado
            float dot = Vector3.Dot(rayDir, desiredDirXZ);
            float score = blocked ? (dot - 3f) : dot;

            if (score > bestScore)
            {
                bestScore = score;
                bestDir = blocked ? Vector3.zero : rayDir;
            }
        }

        // Si todos los rayos están bloqueados, intenta ir directo (mejor que quedarse quieto)
        if (bestDir.sqrMagnitude < 0.01f)
            bestDir = desiredDirXZ;

        // Interpola entre dirección directa y mejor dirección libre
        Vector3 finalDir = Vector3.Lerp(desiredDirXZ, bestDir, steeringWeight);
        finalDir.y = 0f; // garantiza que siempre sea horizontal

        return finalDir.sqrMagnitude > 0.001f ? finalDir.normalized : desiredDirXZ;
    }

    // ═══════════════════════════════════════════════════════════
    //  ATAQUE
    // ═══════════════════════════════════════════════════════════

    private void HandleAttack()
    {
        if (!_chaseStarted || _isAttacking) return;
        if (_attackCooldownTimer > 0f) return;
        if (!PlayerIsInRangeToAttack()) return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        StopHorizontalMovement();
        FacePlayer(); // mira al jugador antes de atacar

        Attack();     // activa el hitbox (virtual → subclases pueden sobreescribir)

        yield return new WaitForSeconds(attackDuration);

        if (attackHitbox != null) attackHitbox.SetActive(false);
        OnAttackFinished();

        _attackCooldownTimer = attackCooldown;
        _isAttacking = false;
    }

    /// <summary>Gira instantáneamente hacia el jugador en el eje Y antes de atacar.</summary>
    private void FacePlayer()
    {
        Vector3 toPlayer = _player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.LookRotation(toPlayer, Vector3.up);
    }

    // ═══════════════════════════════════════════════════════════
    //  MUERTE
    // ═══════════════════════════════════════════════════════════

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        StopHorizontalMovement();
        if (attackHitbox != null) attackHitbox.SetActive(false);

        OnDeath();
    }

    // ═══════════════════════════════════════════════════════════
    //  MÉTODOS PÚBLICOS EXPUESTOS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// True si el jugador está dentro del rango de ataque.
    /// Usa distancia XZ para no depender de la diferencia de altura entre ambos.
    /// </summary>
    public virtual bool PlayerIsInRangeToAttack()
    {
        if (_player == null) return false;
        return HorizontalDistance(transform.position, _player.position) <= attackRange;
    }

    /// <summary>
    /// Activa el hitbox del ataque.
    /// Sobreescribe en subclases para ataques personalizados:
    /// proyectiles, área, combo, splash, etc.
    /// </summary>
    public virtual void Attack()
    {
        OnAttackStart();
        if (attackHitbox != null)
            attackHitbox.SetActive(true);
    }

    /// <summary>
    /// Habilita o deshabilita el movimiento del enemigo.
    /// Útil para: knockback, stun, cutscenes, congelación por habilidad del jugador.
    /// </summary>
    public virtual void EnableDisableMovement(bool enable)
    {
        _canMove = enable;
        if (!enable) StopHorizontalMovement();
    }

    /// <summary>
    /// Aplica daño al enemigo con la fórmula base.
    /// Daño efectivo = max(1, rawDamage - defense)
    /// Crítico: rawDamage × 1.5 (ignora defensa).
    /// </summary>
    public virtual void TakeDamage(float rawDamage, bool isCritical = false)
    {
        if (_isDead) return;

        float effective = isCritical
            ? rawDamage * 1.5f
            : Mathf.Max(1f, rawDamage - defense);

        _currentHealth -= effective;
        OnDamageTaken(effective, isCritical);

        if (_currentHealth <= 0f) Die();
    }

    // ═══════════════════════════════════════════════════════════
    //  HOOKS VIRTUALES — sobreescribe en subclases
    // ═══════════════════════════════════════════════════════════

    /// <summary>Llamado cuando el enemigo ve al jugador por primera vez (antes del chaseDelay).</summary>
    protected virtual void OnPlayerDetected() { }

    /// <summary>Llamado cuando el enemigo empieza a correr (después del chaseDelay).</summary>
    protected virtual void OnChaseStarted() { }

    /// <summary>Llamado justo antes de activar el hitbox.</summary>
    protected virtual void OnAttackStart() { }

    /// <summary>Llamado cuando el hitbox se desactiva y termina el ataque.</summary>
    protected virtual void OnAttackFinished() { }

    /// <summary>Llamado cada vez que el enemigo recibe daño. effectiveDamage ya tiene defensa aplicada.</summary>
    protected virtual void OnDamageTaken(float effectiveDamage, bool wasCritical) { }

    /// <summary>
    /// Llamado cuando HP llega a 0.
    /// Sobreescribe para: animación de muerte, drop de loot, object pooling, efectos...
    /// </summary>
    protected virtual void OnDeath()
    {
        Destroy(gameObject, 0.05f);
    }

    // ═══════════════════════════════════════════════════════════
    //  PROPIEDADES DE SOLO LECTURA
    // ═══════════════════════════════════════════════════════════

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercent => _currentHealth / maxHealth;
    public bool IsDead => _isDead;
    public bool HasDetectedPlayer => _playerDetected;
    public bool IsChasing => _chaseStarted;

    // ═══════════════════════════════════════════════════════════
    //  UTILIDADES PRIVADAS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Distancia en el plano XZ, ignorando diferencia de altura.</summary>
    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private bool IsBlockerTag(string tag)
    {
        foreach (string t in _visionBlockerTags)
            if (t == tag) return true;
        return false;
    }

    // ═══════════════════════════════════════════════════════════
    //  GIZMOS (solo en Editor)
    // ═══════════════════════════════════════════════════════════

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

        // Rango de detección (amarillo semitransparente)
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque (rojo semitransparente)
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Punto de origen de visión (blanco)
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(eyePos, 0.06f);

        // Los rayos de steering solo se muestran en Play Mode
        if (!Application.isPlaying || _player == null) return;

        // Dirección al jugador aplanada en XZ
        Vector3 toPlayerXZ = new Vector3(
            _player.position.x - transform.position.x,
            0f,
            _player.position.z - transform.position.z
        ).normalized;

        float halfAngle = steeringAngle * 0.5f;
        float step = steeringRays > 1 ? steeringAngle / (steeringRays - 1) : 0f;

        for (int i = 0; i < steeringRays; i++)
        {
            float angle = -halfAngle + step * i;
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * toPlayerXZ;
            bool blocked = Physics.Raycast(eyePos, dir, steeringRayLength, _obstacleLayerMask);

            // Cian = libre, Rojo = bloqueado
            Gizmos.color = blocked ? Color.red : Color.cyan;
            Gizmos.DrawRay(eyePos, dir * steeringRayLength);
        }

        // Línea de visión al jugador: verde = libre, roja = bloqueada
        Gizmos.color = HasLineOfSight() ? Color.green : Color.red;
        Gizmos.DrawLine(eyePos, _player.position + Vector3.up * eyeHeight);
    }

   
#endif

}
