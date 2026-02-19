using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Slime_EnemyController : MeleeEnemyBasicLogic
{
    [Header("── Orc Settings ───────────────────")]
    [Tooltip("Animator del modelo del orco.")]
    public Animator animator;

    [Tooltip("Prefab de moneda instanciado al morir.")]
    public GameObject coinPrefab;

    [Tooltip("Clip de audio cuando detecta al jugador.")]
    public AudioClip alertSound;

    private AudioSource _audio;

    // Hashes de parámetros del Animator (más eficiente que strings)
    private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int AnimAttack = Animator.StringToHash("Attack");
    private static readonly int AnimDie = Animator.StringToHash("Die");
    private static readonly int AnimTakeDamage = Animator.StringToHash("TakeDamage");

    protected override void Awake()
    {
        base.Awake(); // MUY IMPORTANTE: siempre llama base.Awake()

        _audio = GetComponent<AudioSource>();

        // Puedes sobreescribir stats aquí o simplemente setearlos en el Inspector del prefab
        // maxHealth   = 120f;
        // damage      = 20f;
        // coinDrop    = 5;
    }

    protected override void Update()
    {
        base.Update(); // mantiene toda la lógica de detección y ataque

        // Sincroniza animación de movimiento
        if (animator != null)
        {
            bool moving = _canMove && HasDetectedPlayer && !PlayerIsInRangeToAttack() && !IsDead;
            animator.SetBool(AnimIsMoving, moving);
        }
    }

    // ── Hooks sobreescritos ─────────────────────────────────────

    protected override void OnPlayerDetected()
    {
        // Grito de alerta al ver al jugador
        if (_audio != null && alertSound != null)
            _audio.PlayOneShot(alertSound);

        Debug.Log($"[OrcEnemy] ¡{name} detectó al jugador!");
    }

    protected override void OnAttackStart()
    {
        animator?.SetTrigger(AnimAttack);
    }

    protected override void OnDamageTaken(float effectiveDamage, bool wasCritical)
    {
        animator?.SetTrigger(AnimTakeDamage);

        string msg = wasCritical ? $"¡CRÍTICO! -{effectiveDamage}" : $"-{effectiveDamage}";
        Debug.Log($"[OrcEnemy] {name} recibió daño: {msg} | HP: {CurrentHealth}/{MaxHealth}");

        // Aquí puedes instanciar un canvas de damage number, llamar a un sistema de partículas, etc.
    }

    protected override void OnDeath()
    {
        // Animación de muerte
        animator?.SetTrigger(AnimDie);

        // Drop de monedas
        for (int i = 0; i < coinDrop; i++)
        {
            if (coinPrefab != null)
            {
                Vector3 scatter = transform.position + new Vector3(
                    Random.Range(-0.5f, 0.5f), 0.1f, Random.Range(-0.5f, 0.5f)
                );
                Instantiate(coinPrefab, scatter, Quaternion.identity);
            }
        }

        // Destruye después de que termine la animación de muerte
        Destroy(gameObject, 1.5f);
    }

    // ── Métodos públicos propios del Orc ───────────────────────

    /// <summary>
    /// Ejemplo: el Orc puede entrar en modo Berserk si su HP baja del 30%.
    /// Llama a EnableDisableMovement() y ajusta stats en tiempo real.
    /// </summary>
    public void ActivateBerserk()
    {
        moveSpeed *= 1.5f;
        damage *= 1.3f;
        attackCooldown = Mathf.Max(0.3f, attackCooldown * 0.5f);
        Debug.Log($"[OrcEnemy] {name} entró en BERSERK.");
    }
}
