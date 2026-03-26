using System;
using UnityEngine;

public class InteractController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string interactTag = "Interactive";
    [SerializeField] private Color emissionColor = Color.cyan;
    [SerializeField] private float emissionIntensity = 2f;


    private Renderer currentRenderer;
    private MaterialPropertyBlock propBlock;
    private InteractiveObject interactiveObject;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }
    private void Start()
    {
        // Suscribirse al evento de movimiento
        InputManager.OnActionSelectedCommand += HandleActionSelectedCommand;
    }

    private void HandleActionSelectedCommand(EnumActionType actionType)
    {
        if(interactiveObject == null) return;

        interactiveObject.Interact();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(interactTag)) return;

        Renderer rend = other.GetComponent<Renderer>();
        if (rend == null) return;

        currentRenderer = rend;
        SetEmission(currentRenderer, true);

        interactiveObject = other.GetComponent<InteractiveObject>();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(interactTag)) return;

        Renderer rend = other.GetComponent<Renderer>();
        if (rend == currentRenderer)
        {
            SetEmission(currentRenderer, false);
            currentRenderer = null;
        }

        interactiveObject = null;
    }

    void SetEmission(Renderer rend, bool enable)
    {
        rend.GetPropertyBlock(propBlock);

        if (enable)
        {
            Color finalColor = emissionColor * emissionIntensity;
            propBlock.SetColor("_EmissionColor", finalColor);
        }
        else
        {
            propBlock.SetColor("_EmissionColor", Color.black);
        }

        rend.SetPropertyBlock(propBlock);
    }
}
