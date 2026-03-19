using System.Collections;
using UnityEngine;

public  class CoinSpawner : MonoBehaviour
{
    public float spawnDelay = 0.04f;
    public float spread = 0.4f;




    public void SpawnCoins(int amount, Vector3 origin)
    {
        StartCoroutine(SpawnRoutine(amount, origin));
    }

    IEnumerator SpawnRoutine(int amount, Vector3 origin)
    {
        var playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        Debug.Log($"Spawning {amount} coins at {origin} with spread {spread} and delay {spawnDelay}");

        for (int i = 0; i < amount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-spread, spread),
                0,
                Random.Range(-spread, spread)
            );

            var coin = CoinPool.Instance.Get(origin + offset);
            coin.Play(playerTransform);

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
