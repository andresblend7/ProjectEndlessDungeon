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

        // Hace que mire a la cámara
        transform.forward = cam.transform.forward;
    }
}
