using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerUtilities : MonoBehaviour
{
    private Direction lookDirection = Direction.Forward;
    [SerializeField]
    private EnumActualToolSelected actualToolSelected = EnumActualToolSelected.Pickaxe;
    [SerializeField]
    private PlayerInGameData actualStats;

    public static PlayerUtilities Instance { get; private set; }


    #region Eventos
    // Evento para notificar movimiento
    public delegate void DamageToPlayerEvent();
    public static event DamageToPlayerEvent OnDamageToPlayerEvent;
    #endregion

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }
    private void Start()
    {
        actualStats = SaveSystem.Load();
        actualStats.MaxHealth = 20;
        actualStats.ActualHealth = actualStats.MaxHealth;
        actualStats.ActualToolDamage = 3;        
        //DEV
        SaveSystem.Save(actualStats); 
    }

    public  Vector3 GetActualPosition()
    {
        return transform.position;
    }
    public Direction GetActualLookDirection()
    {
        return lookDirection;
    }

    public void SetActualToolSelected(EnumActualToolSelected toolToSelect)
    {
        actualToolSelected = toolToSelect;
    }

    public EnumActualToolSelected GetActualToolSelected()
    {
        return actualToolSelected;
    }

    #region PLAYER STATS

    public PlayerInGameData GetActualStats()
    {
        return actualStats;
    }
    public int GetActualHealth()
    {
        return actualStats.ActualHealth;
    }

    public int GetActualToolDamage()
    {
        return actualStats.ActualToolDamage;
    }

    public void ApplyDamageToPlayer(int damage)
    {
        actualStats.ActualHealth -= damage;
        if (actualStats.ActualHealth < 0)
            actualStats.ActualHealth = 0;
        // Notificar a los suscriptores que el jugador ha recibido daño
        OnDamageToPlayerEvent?.Invoke();
        SaveSystem.Save(actualStats);
    }

    #endregion

}

public enum Direction { Forward, Backward, Left, Right }
public enum EnumActualToolSelected { None, Pickaxe, Melee, Ranged }

public static class SaveSystem
{
    private static string path = Application.persistentDataPath + "/savedata.json";

    public static void Save(PlayerInGameData data)
    {
        string json = JsonUtility.ToJson(data, true); // 'true' para que sea legible
        File.WriteAllText(path, json);
        Debug.Log("Juego Guardado en: " + path);
    }

    public static PlayerInGameData Load()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerInGameData>(json);
        }
        return new PlayerInGameData(); // Retorna stats vacíos si no hay archivo
    }

    
}

