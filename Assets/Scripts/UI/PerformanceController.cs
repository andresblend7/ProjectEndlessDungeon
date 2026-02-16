using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PerformanceController : MonoBehaviour
{


    [Header("FPS")]
    // Para limitar la tasa de FPS
    public int targetFrameRate = 60;

    public TMP_Text fpsText;
    public float refreshRate = 0.5f;

    float timer;
    int frames;

    void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;

        if (timer >= refreshRate)
        {
            float fps = frames / timer;
            fpsText.text = Mathf.RoundToInt(fps) + " FPS";
            frames = 0;
            timer = 0;
        }
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
    }

 
}
