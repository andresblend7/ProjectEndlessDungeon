using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float duration = 0.1f;
    public float strength = 0.05f;
    private GameObject model;

    Vector3 originalLocalPos;
    float timer;

    //void Awake()
    //{
    //}

    private void OnEnable()
    {
        model = transform.GetChild(0).gameObject; // Asume que el modelo es el primer hijo
        originalLocalPos = model.transform.localPosition;
    }

    public void Shake()
    {
        timer = duration;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            float damper = timer / duration;

            Vector3 offset = Random.insideUnitSphere * strength * damper;
            model.transform.localPosition = originalLocalPos + offset;
        }
        else
        {
            model.transform.localPosition = originalLocalPos;
        }
    }
}
