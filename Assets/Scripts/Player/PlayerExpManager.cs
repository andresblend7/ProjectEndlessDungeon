using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class PlayerExpManager : MonoBehaviour
{
    public static PlayerExpManager Instance { get; private set; }

    [Header("Level Config")]
    public int[] xpPerLevel = new int[21];

    [Header("UI")]
    public Image xpBar;
    public TMPro.TextMeshProUGUI levelText;

    [Header("Animation")]
    public float lerpSpeed = 5f;

    [Header("Debug")]
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private int currentXp = 0;

    public event Action<int> OnLevelUp;

    private const string LEVEL_KEY = "PLAYER_LEVEL";
    private const string XP_KEY = "PLAYER_XP";

    private float targetFill = 0f;
    private Coroutine lerpRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        ForceUpdateUI();
    }

    // =========================
    // PUBLIC API
    // =========================

    public void AddExperience(int amount)
    {
        if (currentLevel >= xpPerLevel.Length - 1)
            return;

        currentXp += amount;

        while (currentLevel < xpPerLevel.Length &&
               currentXp >= xpPerLevel[currentLevel])
        {
            currentXp -= xpPerLevel[currentLevel];
            LevelUp();
        }

        UpdateXPBarSmooth();
        UpdateLevelText();
        Save();
    }

    public void ResetXpBar()
    {
        if (xpBar != null)
        {
            xpBar.fillAmount = 0f;
            targetFill = 0f;
        }
    }

    // =========================
    // INTERNAL
    // =========================

    void LevelUp()
    {
        currentLevel++;

        // 🔥 Floating Text LEVEL UP
        FloatingTextPool.Instance.SpawnText("LEVEL UP!", FloatingTextType.Event);

        OnLevelUp?.Invoke(currentLevel);

        ResetXpBar();
    }

    void UpdateXPBarSmooth()
    {
        if (xpBar == null || currentLevel >= xpPerLevel.Length)
            return;

        float requiredXp = xpPerLevel[currentLevel];
        targetFill = requiredXp > 0 ? (float)currentXp / requiredXp : 0f;

        if (lerpRoutine != null)
            StopCoroutine(lerpRoutine);

        lerpRoutine = StartCoroutine(LerpXPBar());
    }

    IEnumerator LerpXPBar()
    {
        while (Mathf.Abs(xpBar.fillAmount - targetFill) > 0.001f)
        {
            xpBar.fillAmount = Mathf.Lerp(xpBar.fillAmount, targetFill, Time.deltaTime * lerpSpeed);
            yield return null;
        }

        xpBar.fillAmount = targetFill;
    }

    void UpdateLevelText()
    {
        if (levelText != null)
            levelText.text = $"Lv. {currentLevel}";
    }

    void ForceUpdateUI()
    {
        UpdateLevelText();

        if (xpBar != null && currentLevel < xpPerLevel.Length)
        {
            float requiredXp = xpPerLevel[currentLevel];
            xpBar.fillAmount = requiredXp > 0 ? (float)currentXp / requiredXp : 0f;
        }
    }

    void Save()
    {
        PlayerPrefs.SetInt(LEVEL_KEY, currentLevel);
        PlayerPrefs.SetInt(XP_KEY, currentXp);
        PlayerPrefs.Save();
    }

    void Load()
    {
        currentLevel = PlayerPrefs.GetInt(LEVEL_KEY, 0);
        currentXp = PlayerPrefs.GetInt(XP_KEY, 0);
    }
}