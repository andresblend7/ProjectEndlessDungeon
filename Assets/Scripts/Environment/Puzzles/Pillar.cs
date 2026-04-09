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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ligthMaterial = activeLigth.GetComponent<Renderer>().material;
        ligthMaterial.EnableKeyword("_EMISSION");
        emisionColor = ligthMaterial.GetColor("_EmissionColor");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(activationType.Equals(PillarActivation.Range))
        {
            if (collision.collider.CompareTag("RangeAttackPlayer"))
            {
                isActive = true;
                AlternateLights(activeLightIntensity);
            }
            else
            {
                isActive = false;
                AlternateLights(initialLightIntensity);
            }
        }

    }


    private void OnTriggerEnter(Collider other)
    {
        switch (activationType){
            case PillarActivation.Sword:
                if (other.CompareTag("AttackPlayer"))
                {
                    isActive = true;
                    AlternateLights(activeLightIntensity);
                }else
                {
                    isActive = false;
                    AlternateLights(initialLightIntensity);
                }
            break;
            case PillarActivation.Pickaxe:
                if (other.CompareTag("ActionPlayer"))
                {
                    isActive = true;
                    AlternateLights(activeLightIntensity);
                }
                else
                {
                    isActive = false;
                    AlternateLights(initialLightIntensity);
                }
                break;
        }
       
    }

    public void AlternateLights(float intensity)
    {
        if(isActive)
        {
            // HDR: multiplicas el color base por la intensidad (en escala lineal)
            Color finalColor = emisionColor * Mathf.Pow(2f, activeLightIntensity);
            ligthMaterial.SetColor("_EmissionColor", finalColor);
        }
        else
        {
            Color finalColor = emisionColor * Mathf.Pow(1f, initialLightIntensity);
            ligthMaterial.SetColor("_EmissionColor", finalColor);
        }
    
    }
}
public enum PillarActivation
{
    Sword,
    Range,
    Pickaxe
}
