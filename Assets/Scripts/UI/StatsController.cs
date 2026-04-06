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
        ResourceManager.Instance.OnResourceChanged += OnResourceChanged;

        LoadResources();



        if (playerUtilities == null)
        {

            Debug.Log("OLE = PlayerUtilities reference not set in StatsController, trying to find it in the scene...");
            playerUtilities = PlayerUtilities.Instance;
        }


        playerInGameData = playerUtilities.GetActualStats();

        if(playerInGameData == null)
        {
            Debug.LogError("PlayerInGameData is null after fetching from PlayerUtilities!");
        }

        Debug.Log($"Player actual health: {playerInGameData.ActualHealth}");
        SubscribeToPlayerUtilitiesEvents();

        ShowUiStats();
    }

    #region ------------------- COINS -------------------

    private void LoadResources()
    {

        UpdateCoins(ResourceManager.Instance.Get(ResourceType.Coin));
    }

    private void UpdateCoins(int value)
    {

        txtCoinsCounter.text= value.ToString();


    }

    private void OnResourceChanged(ResourceType changedType, int value)
    {
        if (changedType == ResourceType.Coin)
            UpdateCoins(value);
    }

    #endregion ------------------------------------

    private void SubscribeToPlayerUtilitiesEvents()
    {

        PlayerUtilities.OnDamageToPlayerEvent += OnPlayerDamageTakenEvent;

    }

    public void ShowUiStats()
    {

        healthText.text = $"{playerInGameData.ActualHealth} / {playerInGameData.MaxHealth}";


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
