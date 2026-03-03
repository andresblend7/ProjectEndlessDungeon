using UnityEngine;

public class EnemyMeleeAttackHitBox : MonoBehaviour
{
    private MeleeEnemyBasicLogic _baseLogic;
    public bool playerHitted = false;

    private void Awake()
    {
        _baseLogic = GetComponentInParent<MeleeEnemyBasicLogic>();
    }

    private void OnEnable()
    {
    }



    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("OnTriggerEnter: " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            if (!playerHitted)
                _baseLogic.ApplyDamageToPlayer();

            playerHitted = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHitted = false;
        }
    }
    private void OnDisable()
    {
            playerHitted = false;
    }

  
}
