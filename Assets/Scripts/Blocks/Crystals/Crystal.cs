using UnityEngine;

public class Crystal : MonoBehaviour
{
    public EnumCrystalType Type;
    public int lifePoints = 10;
    private int currentHP;

    [Header("Effectss")]
    private HitShake hitShakeEffect;
    private Vector3 initialScale;

    /*----------------------------------------------*/
    [Header("Drops")]
    public ResourceDropTable[] dropTable;
    private CoinSpawner coinSpawner;
    /* ----------------------------------------------*/



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = lifePoints;
        hitShakeEffect = GetComponent<HitShake>();
        initialScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeHit()
    {

        //Debug.Log("Current HP " + currentHP);

        hitShakeEffect.Shake();

        int damage = PlayerUtilities.Instance.GetActualToolDamage();
        currentHP = currentHP - damage;
        var actualPercentHP = currentHP * 100 / lifePoints;

        var idxDamage = 4;
        if (actualPercentHP >= 80)
            idxDamage = 4;
        else if (actualPercentHP >= 60)
            idxDamage = 3;
        else if (actualPercentHP >= 40)
            idxDamage = 2;
        else if (actualPercentHP >= 20)
            idxDamage = 1;
        else if (actualPercentHP >= 0)
            idxDamage = 0;


        //Debug.Log($" currentHP{currentHP}| lifePoints {lifePoints}| ActualPercenthP: {actualPercentHP}| idxDanage: {idxDamage}");



        if (currentHP <= 0)
        {
            //TODO: Calcular drops

            // Destruir el objeto
            Destroy(gameObject);
        }


    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Block collided with " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("ActionPlayer"))
        {
            TakeHit();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ActionPlayer"))
        {

            TakeHit();


        }
    }
}

public enum EnumCrystalType
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple
}