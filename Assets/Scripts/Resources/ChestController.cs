using System.Collections;
using TMPro.Examples;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform basePart;
    public Transform lidPart;
    private CoinSpawner coinSpawner;

    [Header("Loot")]
    public int coinsToDrop = 10;

    [Header("Caída")]
    public float gravity = 25f;
    public float finalHeight = 0f;

    [Header("Sounds")]
    public AudioClip impactSound;
    public AudioClip openSound;


    // Camera
    private CameraController cameraController;
    private Animator animator;

    private bool isPlaying = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        cameraController = Camera.main.GetComponent<CameraController>();
        coinSpawner = FindFirstObjectByType<CoinSpawner>();
    }

    public void PlayDrop()
    {
        if (isPlaying) return;
        StartCoroutine(DropRoutine());
    }

    public void OpenLid()
    {
        AudioSource.PlayClipAtPoint(openSound, Camera.main.transform.position);
        animator.SetTrigger("Open");
        StartCoroutine(DropCoinsInTime());
    }

    private IEnumerator DropRoutine()
    {
        isPlaying = true;

        Vector3 startPos = transform.position;
        float velocity = 0f;

        Debug.Log("Iniciando caída del cofre..."+transform.position.y);
        Debug.Log(basePart.parent.name);
        // -------- CAÍDA CON ACELERACIÓN --------
        while (transform.position.y > finalHeight)
        {
            velocity += gravity * Time.deltaTime;
            transform.position -= new Vector3(0, velocity * Time.deltaTime, 0);

            if (transform.position.y <= finalHeight)
            {
                transform.position = new Vector3(transform.position.x, finalHeight, transform.position.z);
                break;
            }

            yield return null;
        }
        AudioSource.PlayClipAtPoint(impactSound, Camera.main.transform.position);
        cameraController.ShakeCamera(0.18f, 0.1f);
        animator.SetTrigger("Impact");
        isPlaying = false;

        yield return new WaitForSecondsRealtime(1.21f);
        OpenLid();

    }
    
    private IEnumerator DropCoinsInTime()
    {
        if (coinSpawner != null)
        {
            coinSpawner.SpawnCoinsFromChest(coinsToDrop, transform.position);
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }
}
