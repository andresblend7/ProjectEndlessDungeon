using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class RoomTimerController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI debugDifficult;

    #region Singleton

    public static RoomTimerController Instance { get; private set; }

    private void Awake()
    {
        // Singleton seguro
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Configuración

    [Header("Configuración de Dificultad")]

    [Tooltip("Tiempo en segundos durante el cual la dificultad permanece en 0 al inicio")]
    [SerializeField] private float initialDuration = 10f;

    [Tooltip("Curva que define cómo crece la dificultad en función del tiempo normalizado (0 a 1)")]
    [SerializeField] private AnimationCurve dificultCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Tooltip("Factor máximo que escala la dificultad final")]
    [SerializeField] public float maxDificultfactor = 10f;

    [Tooltip("Tiempo total esperado de una run para normalizar la curva")]
    [SerializeField] private float maxTimeReference = 600f;

    #endregion

    #region Estado Interno

    private float actualTime = 0f;
    private bool isPaused = true;
    public bool isStarted = false;

    private float currentDifficult = 0f;

    #endregion

    #region Eventos

    [Header("Eventos")]

    [Tooltip("Evento invocado cuando cambia la dificultad (envía el nuevo valor)")]
    public UnityEvent<float> OnDifficultyChanged;

    #endregion

    private void Start()
    {
        StartRun();
        // Corutina para actualizar el texto del timer cada segundo (optimización)  
        StartCoroutine(UpdateTimerText());
    }

    private IEnumerator UpdateTimerText()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1); // Esperar un frame para asegurar que timerText esté asignado
            if (isPaused)
            {
                timerText.text = "00:00";
                yield break;
            }
            timerText.text = TimeSpan.FromSeconds(actualTime).ToString(@"mm\:ss");
            debugDifficult.text = $"{currentDifficult:F2}";
        }

    }

    #region Control de Run

    /// <summary>
    /// Inicia la run desde cero
    /// </summary>
    public void StartRun()
    {
        actualTime = 0f;
        currentDifficult = 0f;
        isPaused = false;
        isStarted = true;

        OnDifficultyChanged?.Invoke(currentDifficult);
    }

    /// <summary>
    /// Pausa la run
    /// </summary>
    public void PauseRun()
    {
        isPaused = true;
    }

    /// <summary>
    /// Reanuda la run
    /// </summary>
    public void RestartRun()
    {
        isPaused = false;
    }

    /// <summary>
    /// Reinicia completamente la run
    /// </summary>
    public void ResetRun()
    {
        actualTime = 0f;
        currentDifficult = 0f;
        isPaused = true;

        OnDifficultyChanged?.Invoke(currentDifficult);
    }

    #endregion


    private void Update()
    {
        // Evitar cálculos innecesarios
        if (isPaused) return;

        // Avanza el tiempo de la run
        actualTime += Time.deltaTime;

        // Calcula nueva dificultad
        float newDifficult = CalculateDificulty(actualTime);

        // Solo notifica si cambia significativamente (optimización)
        if (!Mathf.Approximately(newDifficult, currentDifficult))
        {
            currentDifficult = newDifficult;
            OnDifficultyChanged?.Invoke(currentDifficult);
        }

    }


    #region Cálculo de Dificultad

    /// <summary>
    /// Calcula la dificultad basada en el tiempo actual
    /// </summary>
    /// <param name="_time">Tiempo transcurrido</param>
    /// <returns>Dificultad resultante</returns>
    private float CalculateDificulty(float _time)
    {
        // Mantener dificultad en 0 al inicio
        if (_time < initialDuration)
            return 0f;

        // Tiempo ajustado (después del delay inicial)
        float adjustTime = _time - initialDuration;

        // Normalización (0 a 1)
        float normalizedTime = Mathf.Clamp01(adjustTime / maxTimeReference);

        // Evaluación de la curva
        float finalCurve = dificultCurve.Evaluate(normalizedTime);

        // Escalado final
        return finalCurve * maxDificultfactor;
    }

    #endregion
}