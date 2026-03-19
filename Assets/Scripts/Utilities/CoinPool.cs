using System.Collections.Generic;
using UnityEngine;

public class CoinPool : MonoBehaviour
{
    public static CoinPool Instance;

    [Header("Setup")]
    public GameObject coinPrefab;
    public int initialSize = 20;

    private Queue<CoinBehaviour> pool = new Queue<CoinBehaviour>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        for (int i = 0; i < initialSize; i++)
            CreateNew();
    }

    void CreateNew()
    {
        GameObject go = Instantiate(coinPrefab, transform);
        go.SetActive(false);
        pool.Enqueue(go.GetComponent<CoinBehaviour>());
    }

    public CoinBehaviour Get(Vector3 position)
    {
        if (pool.Count == 0)
            CreateNew();

        var coin = pool.Dequeue();
        coin.transform.position = position;
        coin.gameObject.SetActive(true);
        return coin;
    }

    public void Return(CoinBehaviour coin)
    {
        coin.gameObject.SetActive(false);
        pool.Enqueue(coin);
    }
}