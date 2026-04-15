using System.Collections;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public float spawnDelay = 0.04f;
    public float spread = 0.4f;

    [Header("Chest Settings")]
    public float chestUpForce = 16f;
    public float zAxysMaxValue = -0.7f;
    public float chestHorizontalRandom = 1.1f;



    public void SpawnCoinsFromChest(int amount, Vector3 origin)
    {
        StartCoroutine(SpawnCoinsFromChestRoutine(amount, origin));
    }

    private IEnumerator SpawnCoinsFromChestRoutine(int amount, Vector3 origin)
    {
    

        for (int i = 0; i < amount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-spread, spread),
                0,
                Random.Range(-spread, spread)
            );

            var coin = CoinPool.Instance.Get(origin + offset);

            var velocity = new Vector3(
                    Random.Range(-chestHorizontalRandom, chestHorizontalRandom),
                    chestUpForce,
                    zAxysMaxValue
                );

            coin.DropAndWait(origin, velocity);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    public void SpawnCoins(int amount, Vector3 origin, bool goToPlayer = true)
    {
        StartCoroutine(SpawnRoutine(amount, origin, goToPlayer));
    }

    IEnumerator SpawnRoutine(int amount, Vector3 origin, bool goToPlayer)
    {
        var playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        //Debug.Log($"Spawning {amount} coins at {origin} with spread {spread} and delay {spawnDelay}");

        for (int i = 0; i < amount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-spread, spread),
                0,
                Random.Range(-spread, spread)
            );

            var coin = CoinPool.Instance.Get(origin + offset);
            if (goToPlayer)
                coin.GoToPlayer(playerTransform);
            else
                coin.DropAndWait(origin, null);

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
