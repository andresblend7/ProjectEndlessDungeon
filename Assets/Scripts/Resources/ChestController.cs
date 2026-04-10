using System.Collections;
using TMPro.Examples;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform basePart;
    public Transform lidPart;

    [Header("Caída")]
    public float gravity = 25f;


    // Camera
    private CameraController cameraController;
    private Animator animator;

    private bool isPlaying = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        cameraController = Camera.main.GetComponent<CameraController>();
     
    }

    public void PlayDrop()
    {
        if (isPlaying) return;
        StartCoroutine(DropRoutine());
    }

    private IEnumerator DropRoutine()
    {
        isPlaying = true;

        Vector3 startPos = transform.position;
        float velocity = 0f;

        Debug.Log("Iniciando caída del cofre..."+transform.position.y);
        Debug.Log(basePart.parent.name);
        // -------- CAÍDA CON ACELERACIÓN --------
        while (transform.position.y > 0f)
        {
            velocity += gravity * Time.deltaTime;
            transform.position -= new Vector3(0, velocity * Time.deltaTime, 0);

            if (transform.position.y <= 0f)
            {
                transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
                break;
            }

            yield return null;
        }
        cameraController.ShakeCamera(0.18f, 0.1f);
        animator.SetTrigger("Impact");


        isPlaying = false;
    }
 
}
