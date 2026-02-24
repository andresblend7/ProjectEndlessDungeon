using UnityEngine;

public static class DamageNumberSpawner
{
    public static void Spawn(Vector3 position, int amount, bool isHeal, bool isCrit = false)
    {
        var number = DamageNumberPool.Instance.Get(position);
        number.Show(amount, isHeal, isCrit);
    }
}