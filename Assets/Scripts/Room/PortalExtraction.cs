using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalExtraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float requiredStayTime = 3f;
    [SerializeField] private string sceneToLoad = "SafeZone";

    private float stayTimer = 0f;
    private bool playerInside = false;

    private void Update()
    {
        if (!playerInside) return;

        stayTimer += Time.deltaTime;

        if (stayTimer >= requiredStayTime)
        {
            ExtractPlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        stayTimer = 0f;

        Debug.Log("[PortalExtraction] Player entró al portal. Iniciando extracción...");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        stayTimer = 0f;

        Debug.Log("[PortalExtraction] Player salió del portal. Extracción cancelada.");
    }

    private void ExtractPlayer()
    {
        Debug.Log("[PortalExtraction] Extracción completada. Cargando escena...");
        SceneManager.LoadScene(sceneToLoad);
    }
}