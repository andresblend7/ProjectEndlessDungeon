using UnityEngine;
using System.Collections.Generic;

public class DamageNumberPool : MonoBehaviour
{
    public static DamageNumberPool Instance;

    public DamageNumber prefab;
    public int initialSize = 30;

    Queue<DamageNumber> pool = new Queue<DamageNumber>();

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < initialSize; i++)
        {
            Create();
        }
    }

    DamageNumber Create()
    {
        DamageNumber obj = Instantiate(prefab, transform);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    public DamageNumber Get(Vector3 position)
    {
        if (pool.Count == 0)
            Create();

        DamageNumber obj = pool.Dequeue();
        obj.transform.position = position;
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(DamageNumber obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}