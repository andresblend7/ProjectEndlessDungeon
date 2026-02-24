using UnityEngine;

public class TextMeshPro_LookAtCamera : MonoBehaviour
{
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Hace que mire a la cámara activa.
        transform.forward = cam.transform.forward;
    }
}
