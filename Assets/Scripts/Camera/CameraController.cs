using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraController : MonoBehaviour
{

    [Header("Auto Scroll")]
    [SerializeField] private float scrollSpeed = 5f;

    [Header("Player Tracking")]
    [SerializeField] private Transform player;
    [SerializeField] private float catchUpSpeed = 8f;

    [Header("Viewport Thresholds (0 = abajo, 1 = arriba en pantalla)")]
    [SerializeField][Range(0f, 1f)] private float frontThreshold = 0.65f;
    [SerializeField][Range(0f, 1f)] private float softZoneStart = 0.50f;  // zona donde empieza el ajuste suave
    [SerializeField][Range(0f, 1f)] private float dangerThreshold = 0.15f;

    [Header("Events")]
    public UnityEvent onPlayerInDanger;   // jugador cerca del borde trasero
    public UnityEvent onPlayerSafe;       // jugador volvió a zona segura

    private Camera cam;
    private bool isInDanger = false;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        // 1 — Auto scroll constante en Z
        transform.position += Vector3.forward * scrollSpeed * Time.deltaTime;

        // 2 — Posición del jugador en viewport (Y = profundidad en top-down)
        Vector3 viewportPos = cam.WorldToViewportPoint(player.position);

        // 3 — Jugador llega al frente → ajuste suave y progresivo
        if (viewportPos.y > softZoneStart)
        {
            // 0 cuando está en softZoneStart, 1 cuando llega a frontThreshold o más
            float t = Mathf.InverseLerp(softZoneStart, frontThreshold, viewportPos.y);

            // Suavizar la curva para que la entrada sea gradual
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            float worldHalfHeight = GetViewportWorldHalfHeight();
            float targetZ = player.position.z - (frontThreshold * worldHalfHeight * 2f) + worldHalfHeight;

            Vector3 pos = transform.position;
            pos.z = Mathf.Lerp(pos.z, targetZ, smoothT * catchUpSpeed * Time.deltaTime);
            transform.position = pos;
        }

        // 4 — Jugador se queda atrás → danger
        bool playerBehind = viewportPos.y < dangerThreshold;

        if (playerBehind && !isInDanger)
        {
            isInDanger = true;
            onPlayerInDanger?.Invoke();
        }
        else if (!playerBehind && isInDanger)
        {
            isInDanger = false;
            onPlayerSafe?.Invoke();
        }
    }

    // Semialtura del frustum a la altura de la cámara (ortográfica o perspectiva)
    private float GetViewportWorldHalfHeight()
    {
        if (cam.orthographic)
        {
            return cam.orthographicSize;
        }
        else
        {
            float dist = Mathf.Abs(transform.position.y - player.position.y);
            return dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }
    }

    private void OnDrawGizmos()
    {
        if (cam == null) cam = GetComponent<Camera>();

        DrawThresholdLine(frontThreshold, Color.cyan);
        DrawThresholdLine(softZoneStart, Color.yellow);  // zona de suavizado
        DrawThresholdLine(dangerThreshold, Color.red);
    }

    private void DrawThresholdLine(float viewportY, Color color)
    {
        Vector3 left = cam.ViewportToWorldPoint(new Vector3(0f, viewportY, cam.nearClipPlane + 0.1f));
        Vector3 right = cam.ViewportToWorldPoint(new Vector3(1f, viewportY, cam.nearClipPlane + 0.1f));
        Gizmos.color = color;
        Gizmos.DrawLine(left, right);
    }
}
