using System.Collections;
using UnityEngine;

public class Trap_Floor_1 : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform spikes; // El objeto que sube y baja

    [Header("Timing")]
    [SerializeField] private float delayBeforeActivate = 1f;
    [SerializeField] private float activeTime = 2f;
    [SerializeField] private float cooldownTime = 1f;

    [Header("Movimiento")]
    [SerializeField] private float spikeUpDistance = 1f;
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 hiddenPosition;
    private Vector3 activePosition;

    private bool isActivated = false;
    private bool isBusy = false;

    private void Start()
    {
        hiddenPosition = spikes.localPosition;
        activePosition = hiddenPosition + Vector3.up * spikeUpDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isBusy)
        {
            StartCoroutine(TrapRoutine());
        }
    }

    private IEnumerator TrapRoutine()
    {
        isBusy = true;

        yield return new WaitForSeconds(delayBeforeActivate);

        yield return StartCoroutine(MoveSpikes(hiddenPosition, activePosition));

        isActivated = true;

        yield return new WaitForSeconds(activeTime);

        yield return StartCoroutine(MoveSpikes(activePosition, hiddenPosition));

        isActivated = false;

        yield return new WaitForSeconds(cooldownTime);

        isBusy = false;
    }

    private IEnumerator MoveSpikes(Vector3 from, Vector3 to)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            spikes.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        spikes.localPosition = to;

    }
}
