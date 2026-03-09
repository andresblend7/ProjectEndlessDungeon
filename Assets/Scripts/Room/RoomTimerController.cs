using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomTimerController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;

    [Header("Time Settings")]
    public float startTime = 180f;
    public string sceneToLoad = "InitialMenu";

    [Header("Danger Settings")]
    public float dangerThreshold = 30f; // cuando quedan 30 segundos
    public float pulseSpeed = 6f;
    public float pulseAmount = 0.15f;

    [Header("Portal Settings")]
    private PortalController portalController;

    private float currentTime;
    private bool isRunning = true;

    private Color normalColor = Color.white;
    private Color dangerColor = Color.red;

    private Vector3 originalScale;

    private bool portalSpawned = false;

    private void Awake()
    {
        portalController = GetComponent<PortalController>();
    }


    void Start()
    {
        currentTime = startTime;
        originalScale = timerText.transform.localScale;
        UpdateUI();
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            UpdateUI();
            EndGame();
            return;
        }

        UpdateUI();
        HandleDangerEffects();
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void HandleDangerEffects()
    {
        if (currentTime <= dangerThreshold)
        {
            float dangerPercent = 1f - (currentTime / dangerThreshold);

            // Cambio gradual a rojo
            timerText.color = Color.Lerp(normalColor, dangerColor, dangerPercent);

            // Pulso suave
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount * dangerPercent;
            timerText.transform.localScale = originalScale * pulse;

            if (!portalSpawned)
            {
                // Aquí podrías llamar a un método para generar el portal
                portalController.SpawnPortal();
                portalSpawned = true;
            }
        }
        else
        {
            timerText.color = normalColor;
            timerText.transform.localScale = originalScale;
        }
    }

    void EndGame()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void AddTime(float amount)
    {
        currentTime += amount;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        isRunning = true;
    }
}
