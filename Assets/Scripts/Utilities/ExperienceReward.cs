using UnityEngine;
using UnityEngine.Events;

public class ExperienceReward : MonoBehaviour
{
    [SerializeField] private int experienceAmount = 10;
    [SerializeField] private bool rewardOnDisable = true;  // también al desactivar?
    [SerializeField] private bool rewardOnDestroy = true;  // al destruir?


    // Opcional: evento por si quieres hacer algo extra además de dar experiencia
    public UnityEvent OnExperienceGranted;

    private static bool isQuitting = false;

    void OnApplicationQuit()
    {
        isQuitting = true;
    }


    private void OnDisable()
    {
        if (rewardOnDisable)
            GrantExperience();
    }

    private void OnDestroy()
    {
        if (rewardOnDestroy)
            GrantExperience();
    }
    public void GrantExperiencePublic()
    {
        GrantExperience();
    }

    private void GrantExperience()
    {
        // Evitar que se ejecuten las acciones de drop al cerrar la aplicación
        if (isQuitting) return;
        if(FloatingTextPool.Instance == null || PlayerExpManager.Instance == null) return;  

        // Aquí llamas a tu sistema de experiencia global
        FloatingTextPool.Instance.SpawnText($"+{experienceAmount} XP", FloatingTextType.Experience);
        PlayerExpManager.Instance.AddExperience(experienceAmount);

        OnExperienceGranted?.Invoke();
    }
}