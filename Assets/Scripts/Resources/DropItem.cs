using UnityEngine;

public class DropItem : MonoBehaviour
{
    [Header("Movimiento flotante")]
    [Tooltip("Velocidad del movimiento arriba/abajo")]
    public float floatSpeed = 1.5f;
    [Tooltip("Altura máxima del flotado (desde la posición inicial)")]
    public float floatHeight = 0.3f;

    [Header("Rotación")]
    [Tooltip("Velocidad de rotación (grados por segundo)")]
    public float rotationSpeed = 45f;
    [Tooltip("Eje de rotación (normalmente Vector3.up)")]
    public Vector3 rotationAxis = Vector3.up;

    [Header("Tiempo de vida")]
    [Tooltip("Segundos hasta que empiece a parpadear")]
    public float lifetime = 10f;
    [Tooltip("Duración del parpadeo antes de destruirse")]
    public float blinkDuration = 2f;
    [Tooltip("Velocidad del parpadeo (intervalo en segundos entre cambios de visibilidad)")]
    public float blinkInterval = 0.2f;

    [Header("Opcional: cambio de color al parpadear")]
    //[Tooltip("Color que tomará durante el parpadeo (si se asigna material)")]
    //public Color blinkColor = Color.red;
    [Tooltip("Si es true, restaura el color original al destruir (si tiene material)")]
    public bool restoreOriginalColor = true;

    // Variables privadas
    private Vector3 startPosition;
    private Renderer objectRenderer;
    private Material originalMaterial;
    private Color originalColor;
    private bool isBlinking = false;
    private float blinkTimer = 0f;
    private bool visible = true;

    void Start()
    {
        // Guardar posición inicial para el flotado
        startPosition = transform.position;

        // Obtener el renderer (SpriteRenderer, MeshRenderer, etc.)
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
            originalColor = originalMaterial.color;
        }

        // Programar el inicio del parpadeo después de 'lifetime' segundos
        Invoke("StartBlinking", lifetime);
        // Programar la destrucción total después de lifetime + blinkDuration
        Destroy(gameObject, lifetime + blinkDuration);
    }

    void Update()
    {
        // Movimiento flotante (seno suave)
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Rotación lenta
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);

        // Si está parpadeando, manejar el toggle de visibilidad
        if (isBlinking)
        {
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                ToggleVisibility();
            }
        }
    }

    void StartBlinking()
    {
        if (isBlinking) return;
        isBlinking = true;
        blinkTimer = 0f;

        // Cambiar el color si el objeto tiene material
        //if (objectRenderer != null && blinkColor != null)
        //{
        //    objectRenderer.material.color = blinkColor;
        //}
    }

    void ToggleVisibility()
    {
        visible = !visible;
        if (objectRenderer != null)
        {
            objectRenderer.enabled = visible;
        }
        else
        {
            // Fallback: activar/desactivar el GameObject hijo del mesh? 
            // Simplemente desactivamos el propio renderer.
            Debug.LogWarning("DropItem: No se encontró Renderer en " + gameObject.name);
        }
    }

    // Opcional: restaurar color original si se desactiva antes de destruir (por si lo reciclas)
    void OnDestroy()
    {
        if (restoreOriginalColor && objectRenderer != null && originalMaterial != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }
}