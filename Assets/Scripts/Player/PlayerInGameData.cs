using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInGameData 
{
    public int HighScore;
    public int ActualScore;
    public int MaxHealth;
    public int ActualHealth;
    public float CriticalChance;
    public float CriticalDamageMultiplier;
    public int ActualMeleeDamage;
    public int ActualToolDamage;
    public int ActualRangeDamage;
}

public class PlayerDamageCalculated
{
    public int Damage;
    public bool IsCritical;
}
