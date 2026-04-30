using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Slime_EnemyController : MonoBehaviour, IDamageable, IPooledObject
{
    public TMPro.TextMeshPro auxText;

    [Tooltip("GameObject hijo con el Collider trigger del hitbox de daño.")]
    public GameObject attackHitbox;

    [Header("Stats")]
    public EnemyStatsConfig statsConfig;
    public EnemyVFXConfig vfxConfig;

    // stats privados
    private float _currentHealth;
    private bool isKnockbackActive = false;

    public float timeToStartChase = 2f;
    public float timeToStartAttack = 1.5f;
    [Tooltip("Duración en segundos después de atacar cuando vuelve a recuperarse")]
    public float timeToRecoveryAttack = 1f;
    [Tooltip("Duración en segundos del hitbox de ataque activo.")]
    public float attackDuration = 0.3f;

    public Animator modelAnimator;
    public EnemyFlashEffect flashEffect;

    public event Action<GameObject> OnDeactivate;

    private PlayerController playerController;
    private Transform player;
    private EnemyMeleeNavMesh _navMeshController;
    void Awake()
    {
        _navMeshController = GetComponent<EnemyMeleeNavMesh>();
        auxText.text = "";

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            player = playerObj.transform;
        }


        attackHitbox.GetComponent<EnemyMeleeAttackHitBox>().OnPlayerImpact += HandlePlayerImpact;

    }



    private void Start()
    {
        _navMeshController.OnPlayerInAttackRange += HandlePlayerInAttackRange;
    }

    private void HandlePlayerInAttackRange(bool obj)
    {
        StartCoroutine(StartAttackToPlayer());
    }

    public IEnumerator StartAttackToPlayer()
    {
        _navMeshController.SetPaused(true);
        /// limpiar y activar el trigger de ataque
        modelAnimator.SetTrigger("Attack");

        yield return new WaitForSeconds(timeToStartAttack);
        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(attackDuration);
        attackHitbox.SetActive(false);
        yield return new WaitForSeconds(timeToRecoveryAttack);
        modelAnimator.ResetTrigger("Attack");
        _navMeshController.SetPaused(false);
    }

    public void OnSpawn()
    {
        statsConfig.baseHealth = CommonConfigEnemy.GetHealthByDifficulty(statsConfig, RoomTimerController.Instance.GetCurrentDifficulty());
        statsConfig.baseDamage = CommonConfigEnemy.GetDamageByDifficulty(statsConfig, RoomTimerController.Instance.GetCurrentDifficulty());
        _currentHealth = statsConfig.baseHealth;

        // por laguna razon aveces qaparecen en rojo:
        flashEffect.Flash();
        modelAnimator.ResetControllerState();
        _navMeshController.SetPaused(false);
    }

    public void EnableDisableAttackHitBox(bool enable)
    {
        attackHitbox.SetActive(enable);
    }


    public void TakeDamage(TypeOfDamage typeOfDamage, int amount, bool isCritic)
    {

        int damage = amount;

        DamageNumberSpawner.Spawn(
            transform.position + Vector3.up * vfxConfig.heightTextDamage,
            damage,
            false,
            isCrit: isCritic
        );


        if (vfxConfig.canBeNockbacked)
        {
            Vector3 dir = transform.position - player.transform.position;
            ApplyKnockback(dir, vfxConfig.knockbackForce, 0.15f);
        }

        flashEffect.Flash();

        //if (vfxConfig.haveSquashEffect)
        //{
        //    PlaySquash();
        //}



        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        if (isKnockbackActive) return;

        StartCoroutine(KnockbackRoutine(direction, force, duration));
    }

    IEnumerator KnockbackRoutine(Vector3 direction, float force, float duration)
    {
        isKnockbackActive = true;

        float timer = 0f;

        direction.y = 0f;
        direction.Normalize();

        while (timer < duration)
        {
            float t = timer / duration;

            // curva de desaceleración suave
            float currentForce = Mathf.Lerp(force, 0f, t);

            Vector3 move = direction * currentForce * Time.deltaTime;

            _navMeshController.ForceMove(move); // 🔥 clave aquí

            timer += Time.deltaTime;
            yield return null;
        }

        isKnockbackActive = false;
    }

    private void Die()
    {
        // Notificar al pool; él se encarga de desactivar y encolar el objeto
        OnDeactivate?.Invoke(gameObject);
        gameObject.SetActive(false);
        Debug.Log($"[EnemyBase] {name} ha muerto.");

    }

    private void HandlePlayerImpact(bool obj)
    {
        playerController.ProcessDamageToPlayer(new DamageToPlayer
        {
            typeOfDamage = TypeOfDamage.Melee,
            baseDamageAmount = statsConfig.baseDamage,
        });
    }
}
