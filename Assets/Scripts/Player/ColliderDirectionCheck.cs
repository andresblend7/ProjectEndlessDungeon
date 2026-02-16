using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDirectionCheck : MonoBehaviour
{
    public Direction direction;

    [SerializeField]
    public bool IsBlocked = false;

    Collider blockCollider;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("Enemy") || other.CompareTag("Limits"))
        {
            //Debug.Log("ColliderDirectionCheck: Obstacle detected in direction " + direction);
            IsBlocked = true;
            blockCollider = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("Enemy") || other.CompareTag("Limits"))
        {
            IsBlocked = false;
            blockCollider = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("Enemy") || other.CompareTag("Limits"))
        { 
            IsBlocked = true;
            blockCollider= other;
        }
    }
    private void Update()
    {
        // Si se quedó pegado el block por destroy del bloque, liberamos el blocked
        if(!blockCollider && IsBlocked)
        {
            IsBlocked = false;
        }
    }
}
