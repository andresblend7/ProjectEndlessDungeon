using System.Collections.Generic;
using UnityEngine;

public class ItemBagPool : MonoBehaviour
{
    public static ItemBagPool Instance;

    public LootBag prefab;
    public int initialSize = 30;

    Queue<LootBag> pool = new Queue<LootBag>();

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < initialSize; i++)
        {
            Create();
        }
    }

    LootBag Create()
    {
        var obj = Instantiate(prefab, transform);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    public LootBag Get(List<ItemBagResource> data, Vector3 position)
    {
        if (pool.Count == 0)
            Create();

        LootBag obj = pool.Dequeue();
        obj.transform.position = position;
        obj.gameObject.SetActive(true);
        obj.SetResources(data);
        obj.Play();
        return obj;
    }

    public void Return(LootBag obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
