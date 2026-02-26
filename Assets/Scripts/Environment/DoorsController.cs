using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DoorsController : MonoBehaviour
{
    [Header("References")]
    public Transform door;               // Pivot object
    public GameObject mistPlane;

    [Header("PressurePlate Settings")]
    public GameObject pressurePlate;
    public float pressDepth = 0.2f;        // cuánto baja
    public float pressSpeed = 5f;          // velocidad de bajar/subir
    public float rotateSpeed = 360f;       // grados por segundo mientras está presionada
    private float pressAmount = 0f;        // 0 = arriba, 1 = abajo

    private Vector3 pressurePlateinitialPosition;
    private Quaternion pressurePlateinitialRotation;
    private Quaternion pressurePlateTargetRotation;



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
        pressurePlateinitialPosition = pressurePlate.transform.localPosition;
        pressurePlateinitialRotation = pressurePlate.transform.localRotation;
        pressurePlateTargetRotation = pressurePlateinitialRotation * Quaternion.Euler(0, 180f, 0);

        initialRotation = door.localRotation;
        targetRotation = initialRotation * Quaternion.Euler(0, pivotRotateAngle, 0);


        //Debug.Log("DOOR "+targetRotation.eulerAngles);

        //        Vector3 targetEuler = door.localEulerAngles;
        //targetEuler.y += pivotRotateAngle;
        //targetRotation = Quaternion.Euler(targetEuler);
    }

    void Update()
    {
        float targetPress = playerOnPlate ? 1f : 0f;
        pressAmount = Mathf.MoveTowards(pressAmount, targetPress, pressSpeed * Time.deltaTime);
        // Movimiento vertical
        Vector3 targetPos = pressurePlateinitialPosition + Vector3.down * pressDepth * pressAmount;
        pressurePlate.transform.localPosition = targetPos;


        // Rotación controlada tipo tornillo
        if (playerOnPlate)
        {
            pressurePlate.transform.localRotation = Quaternion.RotateTowards(
                pressurePlate.transform.localRotation,
                pressurePlateTargetRotation,
                rotateSpeed * Time.deltaTime
            );
        }
        else
        {
            pressurePlate.transform.localRotation = Quaternion.RotateTowards(
                pressurePlate.transform.localRotation,
                pressurePlateinitialRotation,
                rotateSpeed * Time.deltaTime
            );
        }


        // ---- LÓGICA DE PUERTA ----
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