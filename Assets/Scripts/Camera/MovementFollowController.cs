using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementFollowController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera cam;

    [Header("Movement")]
    public float baseSpeed = 3f;
    public float catchUpSpeed = 6f;

    [Header("Threshold")]
    [Range(0.3f, 0.7f)]
    public float viewportTriggerZ = 0.55f; // punto donde empieza a seguir al jugador

    [Header("Smoothing")]
    public float smooth = 6f;

    private Vector3 velocity;

    void LateUpdate()
    {
        Vector3 camPos = transform.position;

        // Movimiento base hacia adelante (Z)
        camPos.z += baseSpeed * Time.deltaTime;

        // Posición del jugador en viewport (0..1)
        float playerViewportX = cam.WorldToViewportPoint(player.position).z;

        // Si el jugador pasa el umbral, la cámara acelera
        if (playerViewportX > viewportTriggerZ)
        {
            float extra = (playerViewportX - viewportTriggerZ) / (1f - viewportTriggerZ);
            camPos.z += catchUpSpeed * extra * Time.deltaTime;
        }

        transform.position = Vector3.SmoothDamp(transform.position, camPos, ref velocity, 1f / smooth);
    }
}
