using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialMenu_UIController : MonoBehaviour
{
    public TMPro.TextMeshProUGUI txt_coins;
    public TMPro.TextMeshProUGUI txt_Iron;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
        LoadResources();
    }

    private void LoadResources()
    {
        UpdateCoins(ResourceManager.Instance.Get(ResourceType.Coin));
        UpdateIron(ResourceManager.Instance.Get(ResourceType.Iron));
    }

    void OnResourceChanged(ResourceType changedType, int value)
    {
        if (changedType == ResourceType.Coin)
            UpdateCoins(value);
    }

    void UpdateCoins(int value)
    {
        txt_coins.text ="COINS: "+ value.ToString();
    }
    void UpdateIron(int value)
    {
        txt_Iron.text = "Iron: " + value.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }


    // Call this method to load the next scene by name
    public void GoToMine()
    {
        var sceneName = "Gameplay_1";
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs have been reset.");
    }

    public void AddCoin()
    {
        ResourceManager.Instance.Add(ResourceType.Coin, 10);
    }

}
