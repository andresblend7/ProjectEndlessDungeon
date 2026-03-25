using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EffectsUiController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PlayerController playerController;

    public RawImage damageEffectImage;
    public float damageEffectDuration = 0.5f;

    void Awake()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        playerController.OnPlayerTakeDamage += HandlePlayerTakeDamage;
    }

    private void HandlePlayerTakeDamage(int obj)
    {
        StartCoroutine(ShowDamageEffect());
    }

    private IEnumerator ShowDamageEffect()
    {
        // Hacer visible la imagen del efecto de daño
        damageEffectImage.gameObject.SetActive(true);

        // Esperar un breve momento para mostrar el efecto
        yield return new WaitForSeconds(damageEffectDuration);

        // Ocultar la imagen del efecto de daño
        damageEffectImage.gameObject.SetActive(false);
    }
}
