using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitStatsOfRoom();

    }

    private void InitStatsOfRoom()
    {
        actualStats = SaveSystem.Load();

        this.SetStatsToInitRoom();
        Debug.Log($"PlayerUtilities Start: Actual Health = {actualStats.ActualHealth}, Melee Damage = {actualStats.ActualMeleeDamage}, Range Damage = {actualStats.ActualRangeDamage}");

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

    public void SetStatsToInitRoom()
    {
        actualStats.MaxHealth = 20;
        actualStats.ActualHealth = actualStats.MaxHealth;
        actualStats.ActualToolDamage = 1;
        actualStats.ActualMeleeDamage = 4;
        actualStats.ActualRangeDamage = 3;
        actualStats.CriticalChance = 0.15f;
        actualStats.CriticalDamageMultiplier = 1.5f;
    }
    public PlayerInGameData GetActualStats()
    {
        return actualStats;
    }
    public int GetActualMeleeDamage()
    {
        return actualStats.ActualMeleeDamage;
    }
    public float GetActualCriticalChance()
    {
        return actualStats.CriticalChance;
    }

    public PlayerDamageCalculated  GetDamageAfterCriticalCalculation(bool isRanged = false)
    {
        int baseDamage = !isRanged ? actualStats.ActualMeleeDamage : actualStats.ActualRangeDamage;
        bool wasCritic = false;
        if (Random.value < actualStats.CriticalChance)
        {
            wasCritic = true;
            baseDamage = Mathf.RoundToInt(baseDamage * actualStats.CriticalDamageMultiplier); // Daño crítico
        }
        return new PlayerDamageCalculated { Damage = baseDamage, IsCritical = wasCritic };
    }


    public int GetActualRangeDamage()
    {
        return actualStats.ActualRangeDamage;
    }
    public int GetActualHealth()
    {
        return actualStats.ActualHealth;
    }

    public int GetActualToolDamage()
    {
        return actualStats.ActualToolDamage;
    }


    public void RegisterDamageToPlayer(int damage)
    {
        actualStats.ActualHealth -= damage;
        if (actualStats.ActualHealth < 0)
            actualStats.ActualHealth = 0;
        // Notificar a los suscriptores que el jugador ha recibido daño
        OnDamageToPlayerEvent?.Invoke();
        SaveSystem.Save(actualStats);
    }

    public int CalculateDrop(ResourceDropTable table)
    {
        float currentChance = table.dropChance;
        int count = 0;

        // primer drop (minCountDrop) = n cantidad
        int dropNumber = 1;
        float fRoll = Random.Range(0f, 1f);
        if (fRoll <= currentChance)
        {
            count = table.minCountDrop;
            currentChance -= table.dropDecrementalChance;
            dropNumber = 2;
        }
        else
        {
            dropNumber = table.maxAditionalUnitDrop;
            currentChance = 0;
        }

        // Segundo intento en adelante
        int remainingAttempts = table.maxAditionalUnitDrop;
        for (int i = dropNumber; i < remainingAttempts; i++)
        {
            float roll = Random.Range(0f, 1f);

            //Debug.Log($"Drop roll: {roll} | Current Chance: {currentChance} | Count: {count}");
            if (roll <= currentChance)
            {
                count++;
                currentChance -= table.dropDecrementalChance;

                if (currentChance <= 0f)
                    break;
            }
            else
            {
                break;
            }
        }

        return count;
    }

    /// <summary>
    /// Cálculo de drop para items, similar al de recursos pero con la posibilidad de que el primer drop sea 0 (minCountDrop) y con un máximo de drops adicionales (maxAditionalUnitDrop) que se intentan calcular con la misma lógica de probabilidad decreciente. El método devuelve la cantidad total de items a dropear según la tabla proporcionada.
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public int CalculateDropOfLootingBags(ItemDropTable table)
    {
        float currentChance = table.dropChance;
        int count = 0;

        // primer drop (minCountDrop) = n cantidad
        int dropNumber = 1;
        float fRoll = Random.Range(0f, 1f);
        if (fRoll <= currentChance)
        {
            count = table.minCountDrop;
            currentChance -= table.dropDecrementalChance;
            dropNumber = 2;
        }
        else
        {
            dropNumber = table.maxAditionalUnitDrop;
            currentChance = 0;
        }

        // Segundo intento en adelante
        int remainingAttempts = table.maxAditionalUnitDrop;
        for (int i = dropNumber; i < remainingAttempts; i++)
        {
            float roll = Random.Range(0f, 1f);

            //Debug.Log($"Drop roll: {roll} | Current Chance: {currentChance} | Count: {count}");
            if (roll <= currentChance)
            {
                count++;
                currentChance -= table.dropDecrementalChance;

                if (currentChance <= 0f)
                    break;
            }
            else
            {
                break;
            }
        }

        return count;
    }
    #endregion

}

public enum Direction { Forward, Backward, Left, Right }
public enum EnumActualToolSelected { None, Pickaxe, Melee, Ranged }

public enum TypeOfDamage { Melee, Range, Trap, TickOverTime }
public enum TypeOfTickDamage { Poison, Burn, Bleed, electro }
public class DamageToPlayer
{
    public TypeOfDamage typeOfDamage;
    public int baseDamageAmount;
    public bool isCriticalHit; 
    public float duration; // Solo relevante para daño en el tiempo
    public float tickInterval;
    public TypeOfTickDamage typeOfTickDamage; 
}
public static class SaveSystem
{
    private static string path = Application.persistentDataPath + "/savedata.json";

    public static void Save(PlayerInGameData data)
    {
        string json = JsonUtility.ToJson(data, true); // 'true' para que sea legible
        File.WriteAllText(path, json);
        //Debug.Log("Juego Guardado en: " + path);
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



