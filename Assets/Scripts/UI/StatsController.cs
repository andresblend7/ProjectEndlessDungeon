using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsController : MonoBehaviour
{

    public TMPro.TextMeshProUGUI healthText;

    public PlayerUtilities playerUtilities;
    private PlayerInGameData playerInGameData;
    void Awake()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
        playerInGameData = playerUtilities.GetActualStats();
        SubscribeToPlayerUtilitiesEvents();
        ShowUiStats();
    }

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
