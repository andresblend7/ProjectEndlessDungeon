using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPoolController : MonoBehaviour
{
    public Block[] blockPrefabs;
    public int countPerBlock = 40;

    Dictionary<EnumBlockType, Queue<Block>> pools = new();

    void Awake()
    {
        foreach (var block in blockPrefabs)
        {
            var queue = new Queue<Block>();

            for (int i = 0; i < countPerBlock; i++)
            {
                var blockInstance = Instantiate(block);
                blockInstance.gameObject.SetActive(false);
                queue.Enqueue(blockInstance);
            }

            pools.Add(block.Type, queue);
        }
    }

    public Block Get(EnumBlockType type)
    {
        var pool = pools[type];

        if (pool.Count == 0)
        {
            var prefab = GetPrefab(type);
            var blockInstance = Instantiate(prefab);
            blockInstance.gameObject.SetActive(false);
            pool.Enqueue(blockInstance);
        }

        var block = pool.Dequeue();
        //block.gameObject.SetActive(true);
        return block;
    }

    public void Return(Block block)
    {
        block.gameObject.SetActive(false);
        pools[block.Type].Enqueue(block);
    }

    Block GetPrefab(EnumBlockType type)
    {
        foreach (var block in blockPrefabs)
            if (block.Type == type)
                return block;

        return null;
    }
}
