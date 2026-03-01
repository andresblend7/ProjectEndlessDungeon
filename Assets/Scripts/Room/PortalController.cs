using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private int maxAttempts = 30;
    [SerializeField] private float raycastHeight = 10f;
    [SerializeField] private float portalHalfExtents = 0.5f;

    [Header("Layers")]
    [SerializeField] private LayerMask floorLayer;
    [Tooltip("Capas que el portal debe evitar al generar. Si un collider pertenece a una de estas capas, el portal no se generará allí.")]
    [SerializeField] private LayerMask collisionLayers;

    [Header("Ignore Tags")]
    [SerializeField] private List<string> ignoreTags;

    public void SpawnPortal()
    {

        if (portalPrefab == null || player == null)
        {
            Debug.LogError("[PortalSpawner] Falta asignar portalPrefab o player.");
            return;
        }

        for (int i = 0; i < maxAttempts; i++)
        {


            Vector3 randomPoint = GetRandomPointAroundPlayer();

            // Raycast hacia abajo
            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, raycastHeight * 2f, floorLayer))
            {
                Debug.DrawLine(randomPoint, hit.point, Color.purple, 2f);

                Vector3 spawnPosition = hit.point;

                if (IsPositionValid(spawnPosition))
                {
                    Instantiate(portalPrefab, new Vector3(spawnPosition.x, 0.1f, spawnPosition.z), Quaternion.identity);
                    Debug.Log("[PortalSpawner] Portal generado correctamente en intento #" + (i + 1));
                    return;
                }else
                    {
                    Debug.Log("[PortalSpawner] Posición no válida para el portal en intento #" + spawnPosition);
                }
            }
        }

        Debug.LogWarning(
            "[PortalSpawner] ⚠ NO se pudo generar el portal después de " + maxAttempts +
            " intentos. Posiblemente no hay espacio libre suficiente en el radio de " +
            spawnRadius + " metros alrededor del jugador."
        );
    }

    private Vector3 GetRandomPointAroundPlayer()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomPoint = new Vector3(
            player.position.x + randomCircle.x,
            player.position.y + raycastHeight,
            player.position.z + randomCircle.y
        );

        return randomPoint;
    }

    private bool IsPositionValid(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapBox(
            position,
            Vector3.one * portalHalfExtents,
            Quaternion.identity,
            collisionLayers
        );

        foreach (Collider col in colliders)
        {
            if (ignoreTags.Contains(col.tag))
                continue;

            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, spawnRadius);
    }
}
