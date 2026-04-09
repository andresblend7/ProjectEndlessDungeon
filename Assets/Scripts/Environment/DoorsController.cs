using System.Collections;
using TMPro;
using UnityEngine;

public class DoorsController : MonoBehaviour
{
    [Header("Identificación")]
    [Tooltip("ID único para guardar estado")]
    public string doorID;

    [Tooltip("Si es true, recuerda si ya fue abierta")]
    public bool persistState = true;

    [Header("Requisitos de apertura")]
    public int requiredCoins = 0;
    public int requiredKeys = 0;
    public int requiredSouls = 0;

    [Header("References")]
    public Transform door1;
    public Transform door2;
    public GameObject mistPlane;

    [Header("PressurePlate Settings")]
    public GameObject pressurePlate;
    public float pressDepth = 0.2f;
    public float pressSpeed = 5f;
    public float rotateSpeed = 360f;
    private float pressAmount = 0f;

    private Vector3 pressurePlateinitialPosition;
    private Quaternion pressurePlateinitialRotation;
    private Quaternion pressurePlateTargetRotation;

    [Header("Requirements Sign")]
    public GameObject sign;
    public TextMeshPro requirementsText;
    public float timeToOpen = 2f;
    public float pivotRotateAngle = 90f;
    public float openSpeed = 2f;

    [Header("Shake Feedback")]
    public float shakeDuration = 0.3f;
    public float shakeStrength = 5f; // grados
    public float shakeSpeed = 40f;
    private bool isShaking = false;
    public float timeBetweenFailedAttempts = 1f;

    private bool playerOnPlate = false;
    private bool isOpen = false;
    private bool isOpening = false;

    private Quaternion initialDoor1Rotation;
    private Quaternion initialDoor2Rotation;
    private Quaternion door1TargetRotation;
    private Quaternion door2TargetRotation;
    private float timer = 0f;

    //*----------------------------------------------*/
    [Header("Sounds")]
    public AudioClip openDoorSound;
    public AudioClip cantOpenDoorSound;

    private string SaveKey => "DOOR_" + doorID;

    void Start()
    {
        pressurePlateinitialPosition = pressurePlate.transform.localPosition;
        pressurePlateinitialRotation = pressurePlate.transform.localRotation;
        pressurePlateTargetRotation = pressurePlateinitialRotation * Quaternion.Euler(0, 180f, 0);

        initialDoor1Rotation = door1.localRotation;
        door1TargetRotation = initialDoor1Rotation * Quaternion.Euler(0, pivotRotateAngle, 0);

        initialDoor2Rotation = door2.localRotation;
        door2TargetRotation = initialDoor2Rotation * Quaternion.Euler(0, -pivotRotateAngle, 0);

        // ---- CARGAR ESTADO ----
        if (persistState && PlayerPrefs.GetInt(SaveKey, 0) == 1)
        {
            SetDoorOpenInstant();
        }

        var finalText = "";
        if (persistState)
        {
            if (requiredCoins > 0)
                finalText += $"💰 x {requiredCoins}";
            if (requiredKeys > 0)
                finalText += $"\n 🔑 x {requiredKeys}";
            if (requiredSouls > 0)
                finalText += $"\n 👻 x {requiredSouls}";
        }

        requirementsText.text = finalText;
    }

    void Update()
    {
        float targetPress = playerOnPlate ? 1f : 0f;
        pressAmount = Mathf.MoveTowards(pressAmount, targetPress, pressSpeed * Time.deltaTime);

        Vector3 targetPos = pressurePlateinitialPosition + Vector3.down * pressDepth * pressAmount;
        pressurePlate.transform.localPosition = targetPos;

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
                if (CanOpenDoor())
                {
                    StartCoroutine(OpenDoor());
                }
                else
                {
                    if (!isShaking)
                        StartCoroutine(ShakeDoors());
                }

            }
        }
        else
        {
            timer = 0f;
        }
    }

    private bool CanOpenDoor()
    {
        var coins = ResourceManager.Instance.Get(ResourceType.Coin);
        var keys = ResourceManager.Instance.Get(ResourceType.Key);
        var souls = ResourceManager.Instance.Get(ResourceType.Soul);   

        var canOpenDoor = true;

        if (coins < requiredCoins) canOpenDoor = false;
        if (keys < requiredKeys)    canOpenDoor = false;
        if (souls < requiredSouls) canOpenDoor = false;

        if (!canOpenDoor)
        {
            Debug.LogWarning($"No tienes suficientes recursos para abrir la puerta. {SaveKey}");
            return false;
        }

        return true;
    }

    private IEnumerator OpenDoor()
    {
        isOpening = true;

        float t = 0f;

        AudioSource.PlayClipAtPoint(openDoorSound, transform.position);


        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            door1.localRotation = Quaternion.Slerp(initialDoor1Rotation, door1TargetRotation, t);
            door2.localRotation = Quaternion.Slerp(initialDoor2Rotation, door2TargetRotation, t);
            yield return null;
        }

        door1.localRotation = door1TargetRotation;
        door2.localRotation = door2TargetRotation;

        isOpen = true;

        if (mistPlane != null)
            mistPlane.SetActive(false);

        sign.SetActive(false);

        // ---- GUARDAR ESTADO ----
        if (persistState)
        {
            PlayerPrefs.SetInt(SaveKey, 1);
            PlayerPrefs.Save();
        }
    }

    private void SetDoorOpenInstant()
    {
        door1.localRotation = door1TargetRotation;
        door2.localRotation = door2TargetRotation;

        isOpen = true;

        if (mistPlane != null)
            mistPlane.SetActive(false);

        sign.SetActive(false);

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

    private IEnumerator ShakeDoors()
    {
        AudioSource.PlayClipAtPoint(cantOpenDoorSound, transform.position);

        isShaking = true;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float angle = Mathf.Sin(elapsed * shakeSpeed) * shakeStrength;

            door1.localRotation = initialDoor1Rotation * Quaternion.Euler(0, angle, 0);
            door2.localRotation = initialDoor2Rotation * Quaternion.Euler(0, -angle, 0);

            yield return null;
        }

        // Restaurar rotación original
        door1.localRotation = initialDoor1Rotation;
        door2.localRotation = initialDoor2Rotation;

        yield return new WaitForSeconds(timeBetweenFailedAttempts);
        isShaking = false;
    }
}