using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class StatsController : MonoBehaviour
{

    public TMPro.TextMeshProUGUI healthText;
    public TMPro.TextMeshProUGUI txtCoinsCounter;

    public PlayerUtilities playerUtilities;
    private PlayerInGameData playerInGameData;
    void Awake()
    {
        if(playerUtilities == null)
        {
            playerUtilities = PlayerUtilities.Instance;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("1");
        ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
        Debug.Log("2");

        LoadResources();

        Debug.Log("3");


        if (playerUtilities == null)
        {
            Debug.Log("3.1");

            Debug.Log("OLE = PlayerUtilities reference not set in StatsController, trying to find it in the scene...");
            playerUtilities = PlayerUtilities.Instance;
        }

        Debug.Log("4");

        playerInGameData = playerUtilities.GetActualStats();

        Debug.Log("5");
        if(playerInGameData == null)
        {
            Debug.LogError("PlayerInGameData is null after fetching from PlayerUtilities!");
        }

        Debug.Log($"Player actual health: {playerInGameData.ActualHealth}");
        SubscribeToPlayerUtilitiesEvents();
        Debug.Log("6");

        ShowUiStats();
    }

    #region ------------------- COINS -------------------

    private void LoadResources()
    {
        Debug.Log("LoadResources 1");

        UpdateCoins(ResourceManager.Instance.Get(ResourceType.Coin));
    }

    private void UpdateCoins(int value)
    {
        Debug.Log("UpdateCoins "+ value);

        txtCoinsCounter.text= value.ToString();

        Debug.Log("UpdateCoins 2" + value);

    }

    private void OnResourceChanged(ResourceType changedType, int value)
    {
        if (changedType == ResourceType.Coin)
            UpdateCoins(value);
    }

    #endregion ------------------------------------

    private void SubscribeToPlayerUtilitiesEvents()
    {
        Debug.Log("SubscribeToPlayerUtilitiesEvents 1");

        PlayerUtilities.OnDamageToPlayerEvent += OnPlayerDamageTakenEvent;
        Debug.Log("SubscribeToPlayerUtilitiesEvents 2");

    }

    public void ShowUiStats()
    {
        Debug.Log("ShowUiStats");

        healthText.text = $"{playerInGameData.ActualHealth} / {playerInGameData.MaxHealth}";
        Debug.Log("ShowUiStats 2");


    }

    private void OnPlayerDamageTakenEvent()
    {
        playerInGameData = playerUtilities.GetActualStats();
        healthText.text = $"{playerInGameData.ActualHealth} / {playerInGameData.MaxHealth}";
    }

    // Update is called once per frame
    void Update()
    {

    }
}
