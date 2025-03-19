using UnityEngine;

[RequireComponent(typeof(Tile))]
public class TileVisualController : MonoBehaviour
{
    [Header("Visual Properties")]
    [SerializeField] private Material selectedMaterial; // Only keeping selected material
    [SerializeField] private bool enableHoverScale = false;
    [SerializeField] private float hoverScaleMultiplier = 1.1f;
    
    private Tile tileComponent;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private bool isHovered = false;
    private Material originalMaterial; // Store the original material for restoring later
    
    private void Awake()
    {
        tileComponent = GetComponent<Tile>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        // Store the original material
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }
    }
    
    private void Start()
    {
        // No need to apply default material on start anymore
    }
    
    private void OnMouseEnter()
    {
        isHovered = true;
        
        // Only scale if enabled
        if (enableHoverScale)
        {
            transform.localScale = originalScale * hoverScaleMultiplier;
        }
        
        // Apply selected material if available
        if (spriteRenderer != null && selectedMaterial != null)
        {
            spriteRenderer.material = selectedMaterial;
        }
    }
    
    private void OnMouseExit()
    {
        isHovered = false;
        
        // Only reset scale if we're using hover scaling
        if (enableHoverScale)
        {
            transform.localScale = originalScale;
        }
        
        // Revert to original material
        if (spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
        }
    }
    
    // Apply special visual effect when this tile moves
    public void ApplyMoveEffect()
    {
        if (spriteRenderer != null && selectedMaterial != null)
        {
            // Create a temporary material that glows
            Material glowMaterial = new Material(selectedMaterial);
            glowMaterial.SetFloat("_GlowIntensity", 1.0f);
            spriteRenderer.material = glowMaterial;
            
            // Schedule to restore the original material
            Invoke("RestoreDefaultMaterial", 0.5f);
        }
    }
    
    private void RestoreDefaultMaterial()
    {
        if (spriteRenderer != null)
        {
            // Use selected material if hovered, otherwise use original
            spriteRenderer.material = isHovered ? selectedMaterial : originalMaterial;
        }
    }
}
