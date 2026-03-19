using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceSaveData
{
    public List<ResourceEntry> resources = new List<ResourceEntry>();
}

[Serializable]
public class ResourceEntry
{
    public ResourceType type;
    public int amount;
}

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    public event Action<ResourceType, int> OnResourceChanged;

    const string SAVE_KEY = "GAME_RESOURCES";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        InitializeResources();
        Load();
    }

    void InitializeResources()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (!resources.ContainsKey(type))
                resources.Add(type, 0);
        }
    }

    public int Get(ResourceType type)
    {
        return resources[type];
    }

    public void Add(ResourceType type, int amount)
    {
        resources[type] += amount;

        OnResourceChanged?.Invoke(type, resources[type]);

        Save();
    }

    public bool Remove(ResourceType type, int amount)
    {
        if (resources[type] < amount)
            return false;

        resources[type] -= amount;

        OnResourceChanged?.Invoke(type, resources[type]);

        Save();

        return true;
    }

    public void Set(ResourceType type, int amount)
    {
        resources[type] = amount;

        OnResourceChanged?.Invoke(type, resources[type]);

        Save();
    }

    void Save()
    {
        ResourceSaveData data = new ResourceSaveData();

        foreach (var pair in resources)
        {
            data.resources.Add(new ResourceEntry
            {
                type = pair.Key,
                amount = pair.Value
            });
        }

        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    void Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
            return;

        string json = PlayerPrefs.GetString(SAVE_KEY);

        ResourceSaveData data = JsonUtility.FromJson<ResourceSaveData>(json);

        foreach (var entry in data.resources)
        {
            resources[entry.type] = entry.amount;
        }
    }
}

[System.Serializable]
public class ResourceDropTable
{
    public ResourceType resourceType;
    [Range(0f, 1f)]
    public float dropChance; // porcentaje de probabilidad de que caiga el primer item
    [Range(0f, 1f)]
    public float dropDecrementalChance; // porcentaje de decremento para cada item adicional
    public int minCountDrop;
    [Tooltip("Cantidad máxima de items que pueden caer, EXCLUYENDO el primer item")]
    /// <summary>
    /// Cantidad máxima de items que pueden caer, EXCLUYENDO el primer item
    /// </summary>
    public int maxAditionalUnitDrop;
}


public enum ResourceType
{
    Soul,
    Coin,
    // Minerals
    Iron,
    Gold,
    Copper
}