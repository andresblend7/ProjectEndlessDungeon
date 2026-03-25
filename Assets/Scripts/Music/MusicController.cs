using System.Collections;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [Header("Configuración de Audio")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float maxVolume = 0.5f;
    [SerializeField] private float fadeDuration = 2.0f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = false; // Lo manejaremos por código para el fade
    }

    void Start()
    {
        if (musicClip != null)
        {
            StartCoroutine(PlayMusicWithLoops());
        }
    }

    IEnumerator PlayMusicWithLoops()
    {
        while (true)
        {
            // 1. Iniciar reproducción con volumen en 0
            audioSource.volume = 0;
            audioSource.Play();

            // 2. Fade In
            yield return StartCoroutine(Fade(0, maxVolume, fadeDuration));

            // 3. Esperar hasta que falte el tiempo del fade para terminar
            float waitTime = musicClip.length - (fadeDuration * 2);

            // Seguridad por si la canción es más corta que los fades
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }

            // 4. Fade Out
            yield return StartCoroutine(Fade(maxVolume, 0, fadeDuration));

            // 5. Pequeña pausa opcional o reiniciar inmediatamente
            audioSource.Stop();
        }
    }

    IEnumerator Fade(float startVolume, float endVolume, float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, timer / duration);
            yield return null;
        }
        audioSource.volume = endVolume;
    }
}
