using System;
using System.Collections;
using UnityEngine;

public class AttackCollisionController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float hitStopDuration = 0.1f; // Duración del hit stop en segundos
    public float hitStopTimeScale = 0.1f; // Escala de tiempo durante el hit stop


    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnTriggerEnter: " + other.gameObject.name);

       
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            var damageCalculated = PlayerUtilities.Instance.GetDamageAfterCriticalCalculation();
            damageable.TakeDamage(TypeOfDamage.Melee, damageCalculated.Damage, damageCalculated.IsCritical);
            ITimeFreezable timeFreezable = other.GetComponent<ITimeFreezable>();
            if (timeFreezable != null) {
                StartCoroutine(HitStopRoutine());
            }
        }
    }

  

    private IEnumerator HitStopRoutine()
    {
        float originalTimeScale = Time.timeScale;

        Time.timeScale = hitStopTimeScale;

        yield return new WaitForSecondsRealtime(hitStopDuration);

        Time.timeScale = originalTimeScale;
    }
}
