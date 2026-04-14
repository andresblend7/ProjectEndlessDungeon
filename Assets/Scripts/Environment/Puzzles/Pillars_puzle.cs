using System;
using System.Collections;
using System.Linq.Expressions;
using UnityEngine;

public class Pillars_puzle : MonoBehaviour
{
    [Header("Ray Settings")]
    public LineRenderer ray;
    public float drawDuration = 1.5f;   // tiempo en recorrer todos los puntos
    public float widthMin = 0.05f;
    public float widthMax = 0.2f;
    public float widthPulseDuration = 0.5f;
    private Vector3[] positions;
    private float totalLength;

    [Header("Sounds Settings")]
    public AudioClip successPuzzleSound;

    [Header("Chest Reward")]
    public ChestController chestReward;


    public bool isPuzzleCompleted = false;
    public bool swordPillarActive = false;
    public bool rangePillarActive = false;
    public bool pickaxePillarActive = false;
    public Action OnPuzzleCompleted;
    public Action OnPillarFailed;


    void Start()
    {
        // Guardamos las posiciones originales del inspector
        positions = new Vector3[ray.positionCount];
        ray.GetPositions(positions);

        ray.positionCount = 0;

        // Calcular longitud total del path
        totalLength = 0f;
        for (int i = 0; i < positions.Length - 1; i++)
        {
            totalLength += Vector3.Distance(positions[i], positions[i + 1]);
        }
        chestReward.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CheckPuzzleCompletion()
    {
        if (swordPillarActive && rangePillarActive && pickaxePillarActive)
        {
            AudioSource.PlayClipAtPoint(successPuzzleSound, Camera.main.transform.position);

            isPuzzleCompleted = true;
            OnPuzzleCompleted.Invoke();
            // Aquí puedes agregar cualquier lógica adicional que quieras ejecutar cuando el puzzle se complete
            Debug.Log("¡Puzzle completado!");
           
            Invoke("DropChestReward", 1.5f);
            //StartCoroutine(AnimateLaser());
        }
    }

    public void DropChestReward()
    {
        chestReward.gameObject.SetActive(true);
        chestReward.PlayDrop();
    }


    private IEnumerator AnimateLaser()
    {
        float time = 0f;

        while (time < drawDuration)
        {
            time += Time.deltaTime;
            float t = time / drawDuration;

            float targetLength = totalLength * t;

            float accumulated = 0f;
            int currentIndex = 0;

            // Encontrar en qué segmento estamos
            for (int i = 0; i < positions.Length - 1; i++)
            {
                float segmentLength = Vector3.Distance(positions[i], positions[i + 1]);

                if (accumulated + segmentLength >= targetLength)
                {
                    currentIndex = i;
                    break;
                }

                accumulated += segmentLength;
            }

            // Construir puntos visibles
            ray.positionCount = currentIndex + 2;

            for (int i = 0; i <= currentIndex; i++)
            {
                ray.SetPosition(i, positions[i]);
            }

            // Interpolación dentro del segmento actual
            float segmentDist = targetLength - accumulated;
            float segmentLengthCurrent = Vector3.Distance(
                positions[currentIndex],
                positions[currentIndex + 1]
            );

            float segmentT = segmentDist / segmentLengthCurrent;

            Vector3 interpolatedPoint = Vector3.Lerp(
                positions[currentIndex],
                positions[currentIndex + 1],
                segmentT
            );

            ray.SetPosition(currentIndex + 1, interpolatedPoint);     

            yield return null;
        }

        // Completar al final
        ray.positionCount = positions.Length;
        ray.SetPositions(positions);

 
        yield return StartCoroutine(WidthPulse());
    }

    private IEnumerator WidthPulse()
    {
        float time = 0f;

        while (time < widthPulseDuration)
        {
            time += Time.deltaTime;
            float t = time / widthPulseDuration;

            float width = Mathf.Lerp(widthMin, widthMax, t);
            ray.startWidth = width;
            ray.endWidth = width;

            yield return null;
        }

        ray.startWidth = widthMax;
        ray.endWidth = widthMax;
    }

    public void InActivePillar(PillarActivation pillarType)
    {
        switch (pillarType)
        {
            case PillarActivation.Sword:
                swordPillarActive = false;
                break;
            case PillarActivation.Range:
                rangePillarActive = false;
                break;
            case PillarActivation.Pickaxe:
                pickaxePillarActive = false;
                break;
        }

        // Evento de que un pilar falló para que los demás se desactiven
        OnPillarFailed.Invoke();
    }

    public void ActivePillar(PillarActivation pillarType)
    {
        switch (pillarType)
        {
            case PillarActivation.Sword:
                swordPillarActive = true;
                break;
            case PillarActivation.Range:
                rangePillarActive = true;
                break;
            case PillarActivation.Pickaxe:
                pickaxePillarActive = true;
                break;
        }
        CheckPuzzleCompletion();
    }

}
