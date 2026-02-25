using System.Collections;
using UnityEngine;

public class DoorsController : MonoBehaviour
{
    [Header("References")]
    public Transform door;               // Pivot object
    public GameObject mistPlane;
    public Collider pressurePlate;

    [Header("Settings")]
    public float timeToOpen = 2f;
    public float pivotRotateAngle = 90f;
    public float openSpeed = 2f;

    private bool playerOnPlate = false;
    private bool isOpen = false;
    private bool isOpening = false;

    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private float timer = 0f;

    void Start()
    {
        initialRotation = door.localRotation;
        targetRotation = initialRotation * Quaternion.Euler(0, pivotRotateAngle, 0);

        //Debug.Log("DOOR "+targetRotation.eulerAngles);

        //        Vector3 targetEuler = door.localEulerAngles;
        //targetEuler.y += pivotRotateAngle;
        //targetRotation = Quaternion.Euler(targetEuler);
    }

    void Update()
    {
        if (isOpen || isOpening) return;

        if (playerOnPlate)
        {
            timer += Time.deltaTime;

            if (timer >= timeToOpen)
            {
                StartCoroutine(OpenDoor());
            }
        }
        else
        {
            timer = 0f;
        }
    }

    private IEnumerator OpenDoor()
    {
        isOpening = true;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            door.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            yield return null;
        }

        door.localRotation = targetRotation;
        isOpen = true;

        if (mistPlane != null)
            mistPlane.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerOnPlate = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerOnPlate = false;
    }
}