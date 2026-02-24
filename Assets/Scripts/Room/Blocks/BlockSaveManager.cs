using UnityEngine;
using System.Collections.Generic;

public class BlockSaveManager : MonoBehaviour
{
    public static BlockSaveManager Instance;

    private HashSet<int> minedBlocks = new HashSet<int>();
    private const string SAVE_KEY = "MINED_BLOCKS";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterMined(int id)
    {
        if (minedBlocks.Add(id))
            Save();
    }

    public bool IsMined(int id)
    {
        return minedBlocks.Contains(id);
    }

    void Save()
    {
        string data = string.Join(",", minedBlocks);
        PlayerPrefs.SetString(SAVE_KEY, data);
        PlayerPrefs.Save();
    }

    void Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
            return;

        string data = PlayerPrefs.GetString(SAVE_KEY);
        if (string.IsNullOrEmpty(data))
            return;

        string[] ids = data.Split(',');

        minedBlocks.Clear();

        foreach (var s in ids)
        {
            if (int.TryParse(s, out int id))
                minedBlocks.Add(id);
        }
    }

    public void ClearSave()
    {
        minedBlocks.Clear();
        PlayerPrefs.DeleteKey(SAVE_KEY);
    }
}