using UnityEngine;

public class DropCoinsEnemy : MonoBehaviour
{

    [SerializeField] private int experienceAmount = 10;
    [SerializeField] private bool rewardOnDisable = true;  // también al desactivar?
    [SerializeField] private bool rewardOnDestroy = true;  // al destruir?
    [Header("Configurar únicamente monedas aquí:")]
    public ResourceDropTable coinsDropTable;

    private CoinSpawner coinSpawner;

    private void Awake()
    {
        coinSpawner = FindFirstObjectByType<CoinSpawner>();
    }


    private void OnDisable()
    {
        if (rewardOnDisable)
            DropCoin();
    }

    private void OnDestroy()
    {
        if (rewardOnDestroy)
            DropCoin();
    }
    public void DropCoinsPublic()
    {
        DropCoin();
    }

    private void DropCoin()
    {  
        var drops= PlayerUtilities.Instance.CalculateDrop(coinsDropTable);
        coinSpawner.SpawnCoins(drops , transform.position);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
}
