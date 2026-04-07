using System.Collections.Generic;
using UnityEngine;

public class ItemBagPool : MonoBehaviour
{
    public static ItemBagPool Instance;

    public DropItem prefab;
    public int initialSize = 30;

    Queue<DropItem> pool = new Queue<DropItem>();

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < initialSize; i++)
        {
            Create();
        }
    }

    DropItem Create()
    {
        var obj = Instantiate(prefab, transform);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    public DropItem Get(List<ItemBag> data, Vector3 position)
    {
        if (pool.Count == 0)
            Create();

        DropItem obj = pool.Dequeue();
        obj.transform.position = position;
        obj.gameObject.SetActive(true);
        obj.Play();
        return obj;
    }

    public void Return(DropItem obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
