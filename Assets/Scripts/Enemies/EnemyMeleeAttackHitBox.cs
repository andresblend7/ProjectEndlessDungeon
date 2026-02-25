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

    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("OnTriggerEnter: " + other.gameObject.name);
    //    if (other.CompareTag("Player"))
    //    {
    //        _baseLogic.DetectAttackCollisionToPlayer();
    //    }
    //}

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerEnter: " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            if (!playerHitted)
                _baseLogic.DetectAttackCollisionToPlayer();

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

    //private void OnCollisionStay(Collision collision)
    //{
    //    Debug.Log("OnTriggerEnter: " + collision.gameObject.name);

    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        _baseLogic.DetectAttackCollisionToPlayer();
    //    }
    //}

    //private void OnCollisionEnter(Collision collision)
    //{
    //    Debug.Log("OnTriggerEnter: " + collision.gameObject.name);

    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        _baseLogic.DetectAttackCollisionToPlayer();
    //    }
    //}
}
