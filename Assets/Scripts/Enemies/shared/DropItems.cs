using System;
using System.Collections.Generic;
using UnityEngine;

public class DropItems : MonoBehaviour
{
    [Header("Items to drop")]
    public ItemDropTable[] itemDropList;

    [SerializeField] private bool rewardOnDisable = true;  // también al desactivar?
    [SerializeField] private bool rewardOnDestroy = true;  // al destruir?

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    private void OnDisable()
    {
        if (rewardOnDisable)
            DropItemBag();
    }

    private void OnDestroy()
    {
        if (rewardOnDestroy)
            DropItemBag();
    }
    public void DropCoinsPublic()
    {
        DropItemBag();
    }

    public void DropItemBag()
    {
        //Debug.Log("Dropping items...");

        if (PlayerUtilities.Instance == null || ItemBagPool.Instance ==null) return;

        var itemBags = new List<ItemBag>();

        foreach (var item in itemDropList)
        {
            var dropCount = PlayerUtilities.Instance.CalculateItemDrop(item);

            if (dropCount > 0)
            {
                itemBags.Add(new ItemBag { item = item.item, count = dropCount });
            }
        }

        if (itemBags.Count > 0)
        {
            var lootBag = ItemBagPool.Instance.Get(itemBags, transform.position);
        }

    }
}

[Serializable]
public class ItemDropTable
{
    public ItemDrop item;
    [Range(0f, 1f)]
    public float dropChance; // porcentaje de probabilidad de que caiga el primer item
    [Range(0f, 1f)]
    public float dropDecrementalChance; // porcentaje de decremento para cada item adicional
    public int minCountDrop;
    [Tooltip("Cantidad máxima de items que pueden caer, EXCLUYENDO el primer item")]
    public int maxAditionalUnitDrop;
}

public class ItemBag
{
    public ItemDrop item;
    public int count;
}

//!TODO: Mover el enum a un script separado para mantener el código organizado
public enum ItemDrop
{
    Coin,
    Slime,
    AcidBag
}
