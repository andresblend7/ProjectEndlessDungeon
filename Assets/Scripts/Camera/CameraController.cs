using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class CameraController : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  INSPECTOR PARAMS
    // ─────────────────────────────────────────

    [Header("Objetivo")]
    [Tooltip("Transform del jugador (o cualquier objetivo a seguir)")]
    public Transform target;
    public bool followPlayer = true;
    private Coroutine moveCoroutine;


    [Header("Posición")]
    [Tooltip("Offset respecto al objetivo. Y = altura de la cámara sobre el suelo.")]
    public Vector3 offset = new Vector3(0f, 12f, 0f);

    [Tooltip("Rotar la cámara para mirar hacia abajo (top-down). Desactiva si ya la rotaste manualmente.")]
    public bool lookDown = true;

    [Header("Suavizado (SmoothDamp)")]
    [Tooltip("Tiempo para alcanzar el objetivo. Valores bajos = más pegado. Rango recomendado: 0.05 – 0.5")]
    [Range(0.01f, 1f)]
    public float smoothTime = 0.15f;

    [Tooltip("Velocidad máxima de movimiento de la cámara. 0 = sin límite.")]
    public float maxSpeed = 50f;

    [Header("Dead Zone (zona muerta)")]
    [Tooltip("La cámara no se moverá mientras el jugador esté dentro de este radio.")]
    public float deadZoneRadius = 0f;

    [Header("Límites del Nivel")]
    public bool useBounds = false;
    public Vector2 minBounds = new Vector2(-50f, -50f); // X, Z
    public Vector2 maxBounds = new Vector2(50f, 50f);   // X, Z

    [Header("Look Ahead (anticipación)")]
    [Tooltip("La cámara se adelanta en la dirección de movimiento del jugador.")]
    public bool useLookAhead = false;
    [Range(0f, 5f)]
    public float lookAheadDistance = 2f;
    [Range(0.01f, 1f)]
    public float lookAheadSmoothTime = 0.3f;


    [Header("Shake Damge")]
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.2f;
    private Vector3 originalPosition;
    private bool isShaking = false;

    // ─────────────────────────────────────────
    //  PRIVATE STATE
    // ─────────────────────────────────────────

    private Vector3 _velocity = Vector3.zero;
    private Vector3 _lookAheadOffset = Vector3.zero;
    private Vector3 _lookAheadVelocity = Vector3.zero;
    private Vector3 _previousTargetPos;


    // ----------------- Timer CONTROLLER REFERENCE
    private RoomTimerController roomTimerController;

    private void Awake()
    {
        roomTimerController = FindFirstObjectByType<RoomTimerController>();
    }

    private void OnEnable()
    {
        PlayerUtilities.OnDamageToPlayerEvent += HandlePlayerDamaged;
    }

    private void HandlePlayerDamaged()
    {
        if (!isShaking && this != null)
        {
            StartCoroutine(ShakeCoroutine());
        }
    }

    public void ShakeCamera(float customMagnitude = 0, float customDuration = 0)
    {

        var previousMagnitude = shakeMagnitude;
        var previousDuration = shakeDuration;

        if (!isShaking && this != null)
        {
            if (customMagnitude > 0)
                shakeMagnitude = customMagnitude;
            if (customDuration > 0)
                shakeDuration = customDuration;
            StartCoroutine(ShakeCoroutine());
        }

        shakeMagnitude = previousMagnitude;
        shakeDuration = previousDuration;
    }

    private void OnDisable()
    {
        PlayerUtilities.OnDamageToPlayerEvent -= HandlePlayerDamaged;
    }

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("[TopDownCameraFollow] No se asignó un target. Asigna el Transform del jugador en el Inspector.");
            return;
        }

        // Snap inicial (sin suavizado) para evitar que la cámara "vuele" al inicio
        transform.position = target.position + offset;
        _previousTargetPos = target.position;

        if (lookDown)
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);


    }

    IEnumerator ShakeCoroutine()
    {
        // Guardamos la posición inicial de la cámara
        originalPosition = transform.localPosition;
        isShaking = true;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            // Generamos movimiento aleatorio en X e Y
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            // Aplicamos el desplazamiento
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsedTime += Time.deltaTime;

            // Esperamos un frame
            yield return null;
        }

        // Restauramos la posición original
        transform.localPosition = originalPosition;
        isShaking = false;
    }

    void LateUpdate()
    {
        if (!followPlayer) return;

        Vector3 targetPos = target.position;

        // ── Dead Zone ──────────────────────────────────
        Vector3 delta = targetPos - (transform.position - offset);
        if (delta.magnitude < deadZoneRadius) return;

        // ── Look Ahead ────────────────────────────────
        if (useLookAhead)
        {
            Vector3 moveDir = (targetPos - _previousTargetPos) / Time.deltaTime;
            moveDir.y = 0f; // ignoramos el eje vertical
            Vector3 targetLookAhead = moveDir.normalized * Mathf.Min(moveDir.magnitude, lookAheadDistance);
            _lookAheadOffset = Vector3.SmoothDamp(_lookAheadOffset, targetLookAhead, ref _lookAheadVelocity, lookAheadSmoothTime);
        }

        // ── Desired Position ──────────────────────────
        Vector3 desiredPos = targetPos + offset + _lookAheadOffset;

        // ── Bounds ────────────────────────────────────
        if (useBounds)
        {
            desiredPos.x = Mathf.Clamp(desiredPos.x, minBounds.x, maxBounds.x);
            desiredPos.z = Mathf.Clamp(desiredPos.z, minBounds.y, maxBounds.y);
        }

        // ── SmoothDamp ────────────────────────────────
        float speed = maxSpeed > 0f ? maxSpeed : Mathf.Infinity;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _velocity, smoothTime, speed);

        _previousTargetPos = targetPos;
    }


    public void MoveCameraTo(Vector3 newPosition, float duration = 1f, bool pauseTimer = true)
    {

        if (pauseTimer)
            roomTimerController.PauseTimer();

        followPlayer = false; // Desactivamos el seguimiento para mover la cámara a una posición fija

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveCameraSmooth(newPosition, duration));
    }

    private IEnumerator MoveCameraSmooth(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
    }

    // ─────────────────────────────────────────
    //  GIZMOS (visualización en Scene view)
    // ─────────────────────────────────────────

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Dead zone
        if (deadZoneRadius > 0f)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(target.position, deadZoneRadius);
        }

        // Bounds
        if (useBounds)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, target.position.y, (minBounds.y + maxBounds.y) / 2f);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, 0.1f, maxBounds.y - minBounds.y);
            Gizmos.DrawWireCube(center, size);
        }
    }
#endif
}
