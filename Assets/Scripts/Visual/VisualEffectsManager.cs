using UnityEngine;

public class VisualEffectsManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private Camera mainCamera;
    
    [Header("Visual Settings")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.15f);
    
    // Using fields in animation methods
    private float tileSelectScale = 1.2f;
    private float tileScaleSpeed = 5f;
    
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Set background color
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = backgroundColor;
        }
    }
    
    // Apply pulsing effect to a specific tile
    public void PulseTile(GameObject tile)
    {
        if (tile == null) return;
        
        Renderer tileRenderer = tile.GetComponent<Renderer>();
        if (tileRenderer != null && tileRenderer.material != null)
        {
            // Clone the material to avoid affecting all tiles
            Material instanceMaterial = new Material(tileRenderer.material);
            instanceMaterial.SetFloat("_GlowIntensity", 0.8f);
            tileRenderer.material = instanceMaterial;
            
            // Reset after a delay
            StartCoroutine(ResetMaterialAfterDelay(tileRenderer, 0.5f));
        }
    }
    
    // Helper method to reset material after delay
    private System.Collections.IEnumerator ResetMaterialAfterDelay(Renderer renderer, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (renderer != null && tileMaterial != null)
        {
            renderer.material = tileMaterial;
        }
    }
    
    // New method that uses the scale values
    public void AnimateTileSelection(GameObject tile)
    {
        if (tile == null) return;
        
        StartCoroutine(ScaleTile(tile));
    }
    
    private System.Collections.IEnumerator ScaleTile(GameObject tile)
    {
        Vector3 originalScale = tile.transform.localScale;
        Vector3 targetScale = originalScale * tileSelectScale;
        
        float elapsed = 0f;
        float duration = 1f / tileScaleSpeed; // Convert speed to duration
        
        // Scale up
        while (elapsed < duration)
        {
            tile.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        elapsed = 0f;
        
        // Scale down
        while (elapsed < duration)
        {
            tile.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final scale is exact
        tile.transform.localScale = originalScale;
    }
}
