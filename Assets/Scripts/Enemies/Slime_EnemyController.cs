using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Slime_EnemyController : MonoBehaviour
{
    private MeleeEnemyBasicLogic _logic;
    public TMPro.TextMeshPro auxText;
    private bool enemyHasBeenDetected = false;

    public float timeToStartChase = 2f;
    public float timeToStartAttack = 1.5f;
    [Tooltip("Duración en segundos después de atacar cuando vuelve a recuperarse")]
    public float timeToRecoveryAttack = 1f;
    [Tooltip("Duración en segundos del hitbox de ataque activo.")]
    public float attackDuration = 0.3f;



    void Awake()
    {
        _logic = GetComponent<MeleeEnemyBasicLogic>();
        auxText.text = "";

    }

    private void Start()
    {
        _logic.OnPlayerDetected += HandlePlayerDetected;
        _logic.OnPlayerInAttackRange += HandlePlayerInAttackRange;

    }

    private void HandlePlayerInAttackRange(bool obj)
    {
        StartCoroutine(StartAttackToPlayer());
    }

    public IEnumerator StartAttackToPlayer()
    {
        _logic.LookAtPlayer();
        _logic.CanChasePlayer(false);
        yield return new WaitForSeconds(timeToStartAttack);
        _logic.EnableDisableAttackHitBox(true);
        yield return new WaitForSeconds(attackDuration);
        _logic.EnableDisableAttackHitBox(false);
        yield return new WaitForSeconds(timeToRecoveryAttack);
        _logic.CanChasePlayer(true);
    }

    private void HandlePlayerDetected()
    {
        if (!enemyHasBeenDetected)
        {
            enemyHasBeenDetected = true;
            StartCoroutine(StartChaseToPlayer());
            //Debug.Log("Player in attack range");
        }
    }

    public IEnumerator StartChaseToPlayer()
    {   
        _logic.LookAtPlayer();
        auxText.text = "EY!";
        yield return new WaitForSeconds(timeToStartChase);
        auxText.text = "";
        _logic.CanChasePlayer(true);
    }


}
