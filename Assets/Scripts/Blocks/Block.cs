using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Rendering;

public class Block : MonoBehaviour
{

    public EnumBlockType Type;

    public Texture[] crackStages; // cada uno representa 20% de daño
    [Tooltip("Puntos de vida del bloque, el daño depende de la tool equipada por el usuario")]
    public int lifePoints = 5;

    private int currentHP;
    public GameObject crackPlaneTop, crackPlaneFront;

    [Header("Effectss")]
    private HitShake hitShakeEffect;

    //*----------------------------------------------*/
    [Header("Sounds")]
    public AudioClip hitSound;
    public AudioClip destroySound;

    /*----------------------------------------------*/
    [Header("Drops")]
    public ResourceDropTable[] dropTable;
    private CoinSpawner coinSpawner;
    private ExperienceReward experienceReward;
    /* ----------------------------------------------*/

    [Header("Animación de Destrucción")]
    [Tooltip("Tiempo que tarda en encogerse (segundos)")]
    [SerializeField] private float shrinkDuration = 0.3f;

    [Tooltip("Escala final antes de destruirse (0 = desaparece completamente)")]
    [SerializeField] private float targetScale = 0f;

    [Tooltip("Curva de animación para el encogimiento")]
    [SerializeField] private AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("¿Rotar mientras se encoge?")]
    [SerializeField] private bool rotateWhileShrinking = false;

    [Tooltip("Velocidad de rotación (grados por segundo)")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 360, 0);
    private Vector3 initialScale;
    private bool isDestroying = false;

    /*----------------------------------------------*/
    [Header("Movimiento hacia el Jugador")]
    [Tooltip("¿Mover hacia el jugador mientras se encoge?")]
    [SerializeField] private bool moveTowardsPlayer = true;

    private Transform playerTransform;

    [Tooltip("Tag del jugador (si no se asigna manualmente)")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Curva de movimiento hacia el jugador")]
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("Offset de posición final respecto al jugador")]
    [SerializeField] private Vector3 playerOffset = Vector3.zero;

    private int uniqueID;

    void Awake()
    {
        uniqueID = GenerateID();
        coinSpawner = FindFirstObjectByType<CoinSpawner>();
        experienceReward = GetComponent<ExperienceReward>();
    }
    private void Start()
    {
        hitShakeEffect = GetComponent<HitShake>();
        initialScale = transform.localScale;
        currentHP = lifePoints;

        //registrar el bloque en el BlockSaveManager para manejar su estado de minado
        if (BlockSaveManager.Instance.IsMined(uniqueID))
        {
            gameObject.SetActive(false);
        }

        if(experienceReward == null)
        { 
            Debug.LogWarning("ExperienceReward component not found on the block.");
        }
    }

    int GenerateID()
    {
        Vector3 pos = transform.position;

        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.y);
        int z = Mathf.RoundToInt(pos.z);

        // hash determinístico ultra simple
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + x;
            hash = hash * 31 + y;
            hash = hash * 31 + z;
            return hash;
        }
    }

    private void Update()
    {

    }

    public void TakeHit()
    {


        AudioSource.PlayClipAtPoint(hitSound, transform.position);

        crackPlaneTop.SetActive(true);
        crackPlaneFront.SetActive(true);

        //Debug.Log("Current HP " + currentHP);

        Renderer rendererTop = crackPlaneTop.GetComponent<Renderer>();
        Renderer rendererFront = crackPlaneFront.GetComponent<Renderer>();

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

        //Renderer renderer = crackPlaneTop.GetComponent<Renderer>();
        if (rendererTop != null)
        {
            rendererTop.material.SetTexture("_BaseMap", crackStages[idxDamage]);
            rendererFront.material.SetTexture("_BaseMap", crackStages[idxDamage]);
            hitShakeEffect.Shake();
            // renderer.material.mainTexture = newBaseMapTexture; // Use .mainTexture for built-in render pipeline
        }

        if (currentHP <= 0)
        {
            crackPlaneTop.SetActive(false);
            crackPlaneFront.SetActive(false);
            foreach (var table in dropTable)
            {
                int dropCount = PlayerUtilities.Instance.CalculateDrop(table);
                if (dropCount > 0)
                {
                    if(table.resourceType == ResourceType.Coin && coinSpawner != null)
                    {
                        coinSpawner.SpawnCoins(dropCount, transform.position);
                    }

                    ResourceManager.Instance.Add(table.resourceType, dropCount);
                    Debug.Log($"Dropped {dropCount} of {table.resourceType}");
                }
            }

            experienceReward.GrantExperiencePublic();
            StartCoroutine(ShrinkAndDestroy());
        }


    }

    private IEnumerator ShrinkAndDestroy()
    {
        isDestroying = true;
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = initialScale * targetScale;
        Vector3 startPosition = transform.position;
        playerTransform = GameObject.FindGameObjectWithTag(playerTag)?.transform;

        AudioSource.PlayClipAtPoint(destroySound, playerTransform.position);

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / shrinkDuration;

            // Aplicar la curva de animación para escala
            float curveValue = shrinkCurve.Evaluate(progress);

            // Interpolar la escala
            transform.localScale = Vector3.Lerp(startScale, endScale, curveValue);

            // Mover hacia el jugador si está habilitado
            if (moveTowardsPlayer && playerTransform != null)
            {
                //Descativar la collision para evitar problemas de física durante el movimiento
                Collider collider = GetComponent<Collider>();
                if (collider != null)
                    collider.enabled = false;

                float movementValue = movementCurve.Evaluate(progress);
                Vector3 targetPosition = playerTransform.position + playerOffset;
                transform.position = Vector3.Lerp(startPosition, targetPosition, movementValue);
            }

            // Rotar si está habilitado
            if (rotateWhileShrinking)
            {
                transform.Rotate(rotationSpeed * Time.deltaTime);
            }

            yield return null;
        }

        // Asegurar que llegue a la escala final
        transform.localScale = endScale;

        BlockSaveManager.Instance.RegisterMined(uniqueID);

        // Destruir el objeto
        Destroy(gameObject);
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
    private void OnEnable()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;
    }
}

public enum EnumBlockType
{
    None,
    Stone,
    Gold,
    Iron,
    Diamond
}
