using JetBrains.Annotations;
using System;
using UnityEngine;

public static class CommonConfigEnemy 
{
   public static int GetHealthByDifficulty(EnemyStatsConfig config, float difficultyLevel)
    {
        return Mathf.RoundToInt(config.baseHealth * (1 + config.healthFactorByDifficulty * difficultyLevel));
    }
    public static int GetDamageByDifficulty(EnemyStatsConfig config, float difficultyLevel)
    {
        return Mathf.RoundToInt(config.baseDamage * (1 + config.damageFactorByDifficulty * difficultyLevel));
    }
}

[Serializable]
public class EnemyStatsConfig
{
    [Header("Health")]
    [Tooltip("Vida base del enemigo con la que spawnea en dificultad 0")]
    public int baseHealth = 1;
    [Tooltip("Vida máxima del enemigo en máxima dificultad")]
    public int maxHealth = 10;
    [Tooltip("Factor de aumento de vida por cada nivel de dificultad. Por ejemplo, si es 0.1, la vida aumentará un 10% por cada nivel de dificultad.")]
    [Range(0f, 1f)]
    public float healthFactorByDifficulty = 1f;

    [Header("Damage")]
    public int baseDamage = 1;
    public int maxDamage = 10;
    [Range(0f, 1f)]
    public float damageFactorByDifficulty = 1f;
    [Range(0f, 1f)]
    public float CriticalHitChance = 0.1f;
}



[Serializable]
public class EnemyVFXConfig
{
    [Tooltip("Altura del texto al recibir daño")]
    public float heightTextDamage = 1.5f;
    public bool canBeNockbacked = true;
    public float knockbackForce = 5f;
    public bool haveSquashEffect = true;
    public bool canBeStunned = true;
}

[Serializable]
public class EnemyAudioConfig
{
    public AudioClip[] audioClipsDamage;
    public AudioClip[] audioClipsDeath;
    public AudioClip[] audioClipsAttack;
}


