using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Pillar : MonoBehaviour
{
    public PillarActivation activationType;
    public GameObject activeLigth;
    public GameObject crystal;

    [Header("Ligth Settings")]
    public float initialLightIntensity = 1;
    public float activeLightIntensity = 8;
    private bool isActive = false;
    private Material ligthMaterial;
    private Color emisionColor;
    private Color endEmisionColor;
    private Color initialEmisionColor;
    private Color maxEmisionColor;
    public Pillars_puzle puzzleManager;

    [Header("Sounds Settings")]
    public AudioClip errorSound;
    public AudioClip correctSound;

    private bool puzzleCompleted = false;

    void Awake()
    {
        puzzleManager.OnPillarFailed += () =>
        {
            isActive = false;
            AlternateLights(false);
        };
        puzzleManager.OnPuzzleCompleted += () =>
        {
            puzzleCompleted = true;
            StartCoroutine(BlindEffect());
        };
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ligthMaterial = activeLigth.GetComponent<Renderer>().material;
        ligthMaterial.EnableKeyword("_EMISSION");
        emisionColor = ligthMaterial.GetColor("_EmissionColor");

        endEmisionColor = emisionColor * Mathf.Pow(2f, activeLightIntensity);
        initialEmisionColor = emisionColor * Mathf.Pow(1f, initialLightIntensity);
        maxEmisionColor = emisionColor * Mathf.Pow(2f, 8.5f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator BlindEffect()
    {

        for (int i = 0; i < 6; i++)
        {
            ligthMaterial.SetColor("_EmissionColor", maxEmisionColor);
            yield return new WaitForSecondsRealtime(0.3f);
            ligthMaterial.SetColor("_EmissionColor", initialEmisionColor);
            yield return new WaitForSecondsRealtime(0.3f);

        }

        AlternateLights(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (puzzleCompleted) return;


        if (collision.collider.CompareTag("RangeAttackPlayer"))
        {
            if (activationType.Equals(PillarActivation.Range))
            {
                isActive = true;
            }
            else
            {
                isActive = false;
            }
            AlternateLights();

        }

    }


    private void OnTriggerEnter(Collider other)
    {
        if (puzzleCompleted) return;

        switch (activationType)
        {
            case PillarActivation.Sword:
                if (other.CompareTag("AttackPlayer"))
                {
                    isActive = true;
                    AlternateLights();
                }
                else if (other.CompareTag("AttackPlayer") || other.CompareTag("ActionPlayer"))
                {
                    isActive = false;
                    AlternateLights();
                }
                break;
            case PillarActivation.Pickaxe:
                if (other.CompareTag("ActionPlayer"))
                {
                    isActive = true;
                    AlternateLights();
                }
                else if (other.CompareTag("AttackPlayer") || other.CompareTag("RangeAttackPlayer"))
                {
                    isActive = false;
                    AlternateLights();
                }
                break;
            case PillarActivation.Range:
                if (other.CompareTag("RangeAttackPlayer"))
                {
                    isActive = true;
                    AlternateLights();
                }
                else if (other.CompareTag("AttackPlayer") || other.CompareTag("ActionPlayer"))
                {
                    isActive = false;
                    AlternateLights();
                }
                break;
        }

    }

    public void AlternateLights(bool invokeParentEvent = true)
    {
        if (isActive)
        {
            AudioSource.PlayClipAtPoint(correctSound, transform.position);
            ligthMaterial.SetColor("_EmissionColor", endEmisionColor);
            if (invokeParentEvent)
                puzzleManager.ActivePillar(activationType);

        }
        else
        {
            AudioSource.PlayClipAtPoint(errorSound, transform.position);
            ligthMaterial.SetColor("_EmissionColor", initialEmisionColor);
            if (invokeParentEvent)
                puzzleManager.InActivePillar(activationType);
        }

    }
}
public enum PillarActivation
{
    Sword,
    Range,
    Pickaxe
}
