using System.Collections;
using UnityEngine;

public class EnemyFlashEffect : MonoBehaviour
{

    [Header("Flash Settings")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.08f;
    public float flashIntensity = 2f;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private Color[] originalColors;
    private Color[] originalEmission;

    private Coroutine flashCoroutine;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        mpb = new MaterialPropertyBlock();

        originalColors = new Color[renderers.Length];
        originalEmission = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].sharedMaterial;

            if (mat.HasProperty(BaseColorID))
                originalColors[i] = mat.GetColor(BaseColorID);

            if (mat.HasProperty(EmissionColorID))
                originalEmission[i] = mat.GetColor(EmissionColorID);
        }
    }

    public void Flash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        // FLASH BLANCO
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].GetPropertyBlock(mpb);

            mpb.SetColor(BaseColorID, flashColor);

            // emission blanco SOLO durante flash
            if (renderers[i].sharedMaterial.HasProperty(EmissionColorID))
                mpb.SetColor(EmissionColorID, flashColor * flashIntensity);

            renderers[i].SetPropertyBlock(mpb);
        }

        yield return new WaitForSeconds(flashDuration);

        // RESTORE limpio (sin emission)
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].GetPropertyBlock(mpb);

            mpb.SetColor(BaseColorID, originalColors[i]);

            mpb.SetColor(EmissionColorID, Color.black);

            renderers[i].SetPropertyBlock(mpb);
        }

        flashCoroutine = null;
    }
}
