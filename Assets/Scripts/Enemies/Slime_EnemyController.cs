using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime_EnemyController : MonoBehaviour, IMeleeEnemyBehaviour
{
    private MeleeEnemyBasicLogic baseLogic;
    [SerializeField]
    private bool isAttacking = false;


    public float timeToStartAttack = 0.3f;
    // Start is called before the first frame update
    void Start()
    {
        InitReferences();
    }

    // Update is called once per frame
    void Update()
    {
            //realizar el ataque
            if (baseLogic.IsPlayerInAttackRange() && !isAttacking && !baseLogic.IsMoving())
            {
            Debug.Log("Slime_EnemyController: Player in attack range, starting attack!");
            StartCoroutine(SlimeAttack());

            }
    }

    public void Tick()
    {
        throw new System.NotImplementedException();
    }

    public void InitReferences()
    {
        baseLogic = GetComponent<MeleeEnemyBasicLogic>();
    }

    public void MakeAttack()
    {
        baseLogic.Attack(3);
    }

    public void SubscribeReceiveDamage()
    {
        throw new System.NotImplementedException();
    }

    public void SubscribeDeath()
    {
        throw new System.NotImplementedException();
    }
    public IEnumerator SlimeAttack()
    {
        isAttacking = true;
        yield return new WaitForSecondsRealtime(timeToStartAttack);
        if (!baseLogic.IsPlayerInAttackRange())
        {
            isAttacking = false;
            Debug.Log("Slime_EnemyController: Cancelling attack!");
            yield break;
        }
        baseLogic.EnableDisableMovement(false);
        //Debug.Log("Slime_EnemyController: Starting attack animation!");
        yield return new WaitForSecondsRealtime(1.0f);
        MakeAttack();
        yield return new WaitForSecondsRealtime(baseLogic.attackDuration);
        isAttacking = false;
        baseLogic.EnableDisableMovement(true);

    }
}
