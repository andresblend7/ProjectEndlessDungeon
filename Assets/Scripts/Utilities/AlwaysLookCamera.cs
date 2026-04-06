using UnityEngine;

public class AlwaysLookCamera : MonoBehaviour
{
    private Camera cam;
    public float fixedXAngle = 45f; // Ángulo fijo en X

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        Vector3 direction = cam.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion yRotation = Quaternion.LookRotation(-direction);

        // Combinar: rotación Y + inclinación fija en X
        transform.rotation = yRotation * Quaternion.Euler(fixedXAngle, 0f, 0f);
    }
}
