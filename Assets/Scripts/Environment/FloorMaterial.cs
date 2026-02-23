using UnityEngine;

public class FloorMaterial : MonoBehaviour
{ // Expose tiling properties to the Inspector
    private Vector2 tilingScale = new Vector2(1.0f, 1.0f);

    private Renderer objectRenderer;

    void Start()
    {
        // Get the Renderer component attached to the GameObject
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null && objectRenderer.material != null)
        {

            tilingScale.x = gameObject.transform.localScale.x; // You can adjust this based on your needs
            tilingScale.y = gameObject.transform.localScale.z; 
            // Apply the initial tiling scale
            ApplyTiling();
        }
        else
        {
            Debug.LogWarning("Renderer or Material not found on this GameObject."+gameObject.name);
        }
    }

    // You can call this method to update tiling dynamically
    public void ApplyTiling()
    {
        // Set the main texture scale (tiling) on the material
        // "_MainTex" is the default property name for the main texture in most standard shaders
        objectRenderer.material.mainTextureScale = tilingScale;

        // You can also set the offset similarly:
        // objectRenderer.material.mainTextureOffset = new Vector2(xOffset, yOffset);
    }
}
