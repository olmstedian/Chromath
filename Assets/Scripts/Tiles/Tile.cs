using UnityEngine;
using TMPro;
using System.Collections;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer tileRenderer;
    [SerializeField] private TextMeshPro valueText;
    [SerializeField] private GameObject specialEffectPrefab;
    [SerializeField] private float moveSpeed = 8f;
    
    private TileData data;
    private Vector3 targetPosition;
    private bool isMoving = false;
    
    // Color mappings
    [SerializeField] private Color[] tileColors;
    
    public TileData Data => data;
    
    public void Initialize(TileData tileData, Vector2 position)
    {
        data = tileData;
        transform.position = position;
        targetPosition = position;
        UpdateVisuals();
    }
    
    public void UpdateVisuals()
    {
        // Update the number text
        valueText.text = data.value.ToString();
        
        // Update the color based on tile's color property
        tileRenderer.color = tileColors[(int)data.color];
        
        // Add special tile visual indicators if needed
        if (data.isSpecial)
        {
            // Add visual distinction for special tiles
            UpdateSpecialTileVisuals();
        }
    }
    
    private void UpdateSpecialTileVisuals()
    {
        // Different visual effects based on special tile type
        switch (data.specialType)
        {
            case SpecialTileType.Wildcard:
                // Add wildcard visuals (e.g., star icon or outline)
                break;
            case SpecialTileType.Jumper:
                // Add jumper visuals (e.g., lightning bolt icon)
                break;
            case SpecialTileType.Converter:
                // Add converter visuals
                break;
            case SpecialTileType.Multiplier:
                // Add multiplier visuals (e.g., x2 icon)
                break;
            case SpecialTileType.Bomb:
                // Add bomb visuals
                break;
            case SpecialTileType.Rainbow:
                // Add rainbow visuals (e.g., gradient)
                break;
        }
        
        // Instantiate special effect if needed
        if (specialEffectPrefab != null)
        {
            Instantiate(specialEffectPrefab, transform.position, Quaternion.identity, transform);
        }
    }
    
    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
    }
    
    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPosition, 
                moveSpeed * Time.deltaTime
            );
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }
    
    public IEnumerator MergeWithTile(Tile otherTile)
    {
        // Move to the other tile's position
        MoveTo(otherTile.transform.position);
        
        // Wait until the move is complete
        while (isMoving)
        {
            yield return null;
        }
        
        // Update the other tile's data with merged values
        otherTile.Data.MergeWith(data);
        otherTile.UpdateVisuals();
        
        // Play merge effect
        PlayMergeEffect(otherTile.transform.position);
        
        // Destroy this tile
        Destroy(gameObject);
    }
    
    private void PlayMergeEffect(Vector3 position)
    {
        // Create a simple merge effect (can be enhanced later)
        GameObject effect = new GameObject("MergeEffect");
        effect.transform.position = position;
        
        // Add particle system or animation
        // For now, we'll just create a simple scale animation
        var scaleEffect = effect.AddComponent<ScaleEffect>();
        scaleEffect.Initialize(0.5f, 1f);
        
        Destroy(effect, 0.5f);
    }
}

// Simple scale effect class for merge animation
public class ScaleEffect : MonoBehaviour
{
    private float duration;
    private float elapsed = 0;
    private Vector3 startScale;
    private Vector3 targetScale;
    
    public void Initialize(float duration, float maxScale)
    {
        this.duration = duration;
        startScale = Vector3.one * 0.8f;
        targetScale = Vector3.one * maxScale;
        transform.localScale = startScale;
    }
    
    private void Update()
    {
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);
        
        transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
        
        if (progress >= 1)
        {
            Destroy(this);
        }
    }
}