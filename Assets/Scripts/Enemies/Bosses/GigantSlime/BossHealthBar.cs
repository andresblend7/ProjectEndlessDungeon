using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public Image healthFront;
    public Image healthBack;

    public float lagDelay = 0.25f;
    public float lagSpeed = 1.5f;

    Coroutine lagRoutine;

    public void SetHealth(float percent)
    {
        // la barra real baja inmediatamente
        healthFront.fillAmount = percent;

        if (lagRoutine != null)
            StopCoroutine(lagRoutine);

        lagRoutine = StartCoroutine(AnimateLag(percent));
    }

    IEnumerator AnimateLag(float target)
    {
        yield return new WaitForSeconds(lagDelay);

        while (healthBack.fillAmount > target)
        {
            healthBack.fillAmount = Mathf.MoveTowards(
                healthBack.fillAmount,
                target,
                lagSpeed * Time.deltaTime
            );

            yield return null;
        }

        healthBack.fillAmount = target;
    }
}