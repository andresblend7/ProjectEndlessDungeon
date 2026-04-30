using System;
using UnityEngine;

public class EnemyMeleeAttackHitBox : MonoBehaviour
{
    public bool playerHitted = false;

    public event Action<bool> OnPlayerImpact;


    private void OnEnable()
    {
    }



    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("OnTriggerEnter: " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            if (!playerHitted)
                OnPlayerImpact.Invoke(true);

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
