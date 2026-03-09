using UnityEngine;

public class SlimeBossCameraPositioner : MonoBehaviour
{
    public Transform cameraPositionToBoss;
    public bool cameraPositionerActive = false;
    private void Awake()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!cameraPositionerActive)
            {
                Camera.main.GetComponent<CameraController>().MoveCameraTo(cameraPositionToBoss.position);
                cameraPositionerActive = true;
            }
        }
    }


}
