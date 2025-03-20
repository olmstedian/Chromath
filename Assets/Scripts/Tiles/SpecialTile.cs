using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Add this for List<>

// Enum to define different special tile types
public enum SpecialTileType
{
    None,
    RowClear,       // Clears entire row
    ColumnClear,    // Clears entire column
    AreaClear,      // Clears a 3x3 area around the tile
    ColorClear,     // Clears all tiles of the same color
    ValueBoost,     // Doubles value when merged
    Wildcard        // Can merge with any color
}

[RequireComponent(typeof(GameTile))]
public class SpecialTile : MonoBehaviour
{
    [SerializeField] private Sprite rowClearSprite;
    [SerializeField] private Sprite columnClearSprite;
    [SerializeField] private Sprite areaClearSprite;
    [SerializeField] private Sprite colorClearSprite;
    [SerializeField] private Sprite valueBoostSprite;
    [SerializeField] private Sprite wildcardSprite;
    
    [SerializeField] private GameObject specialEffectPrefab;
    
    private GameTile gameTile;
    private SpriteRenderer iconRenderer;
    private SpecialTileType specialType = SpecialTileType.None;
    
    private void Awake()
    {
        gameTile = GetComponent<GameTile>();
        
        // Create child object for special indicator icon if it doesn't exist
        Transform iconTransform = transform.Find("SpecialIcon");
        if (iconTransform == null)
        {
            GameObject iconObject = new GameObject("SpecialIcon");
            iconObject.transform.SetParent(transform);
            iconObject.transform.localPosition = new Vector3(0, 0, -0.05f);
            iconRenderer = iconObject.AddComponent<SpriteRenderer>();
            iconRenderer.sortingOrder = 15; // Make sure it renders on top of the tile
        }
        else
        {
            iconRenderer = iconTransform.GetComponent<SpriteRenderer>();
            if (iconRenderer == null)
            {
                iconRenderer = iconTransform.gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        // Hide icon by default
        if (iconRenderer != null)
        {
            iconRenderer.enabled = false;
        }
    }
    
    public void SetSpecialType(SpecialTileType type)
    {
        specialType = type;
        UpdateVisuals();
    }
    
    public SpecialTileType GetSpecialType()
    {
        return specialType;
    }
    
    private void UpdateVisuals()
    {
        if (iconRenderer == null) return;
        
        // Show the icon
        iconRenderer.enabled = specialType != SpecialTileType.None;
        
        // Set the appropriate sprite
        switch (specialType)
        {
            case SpecialTileType.RowClear:
                iconRenderer.sprite = rowClearSprite;
                break;
            case SpecialTileType.ColumnClear:
                iconRenderer.sprite = columnClearSprite;
                break;
            case SpecialTileType.AreaClear:
                iconRenderer.sprite = areaClearSprite;
                break;
            case SpecialTileType.ColorClear:
                iconRenderer.sprite = colorClearSprite;
                break;
            case SpecialTileType.ValueBoost:
                iconRenderer.sprite = valueBoostSprite;
                break;
            case SpecialTileType.Wildcard:
                iconRenderer.sprite = wildcardSprite;
                break;
            default:
                iconRenderer.enabled = false;
                break;
        }
    }
    
    // Activate the special effect when this tile is merged or used
    public void ActivateSpecialEffect(Vector2Int position)
    {
        switch (specialType)
        {
            case SpecialTileType.RowClear:
                StartCoroutine(ActivateRowClear(position));
                break;
            case SpecialTileType.ColumnClear:
                StartCoroutine(ActivateColumnClear(position));
                break;
            case SpecialTileType.AreaClear:
                StartCoroutine(ActivateAreaClear(position));
                break;
            case SpecialTileType.ColorClear:
                StartCoroutine(ActivateColorClear(gameTile.CurrentColor));
                break;
            case SpecialTileType.ValueBoost:
                // This is handled during merge calculation
                break;
        }
    }
    
    private IEnumerator ActivateRowClear(Vector2Int position)
    {
        TileManager tileManager = TileManager.Instance;
        if (tileManager == null) yield break;
        
        int row = position.y;
        int columns = FindObjectOfType<Board>().columns;
        
        // Visual effect before clearing
        yield return StartCoroutine(RowClearAnimation(row));
        
        // Collect all positions in this row
        List<Vector2Int> positionsToRemove = new List<Vector2Int>();
        for (int col = 0; col < columns; col++)
        {
            Vector2Int tilePos = new Vector2Int(col, row);
            if (tileManager.IsTileAt(tilePos) && tilePos != position)
            {
                positionsToRemove.Add(tilePos);
            }
        }
        
        // Remove the tiles with a slight delay for better visuals
        foreach (Vector2Int pos in positionsToRemove)
        {
            tileManager.RemoveTileAt(pos);
            yield return new WaitForSeconds(0.05f);
        }
        
        // Notify score manager of special move
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnSpecialMove(150 * positionsToRemove.Count);
        }
    }
    
    private IEnumerator RowClearAnimation(int row)
    {
        // Create a beam effect across the row
        // This would be a custom visual effect - simplified here
        
        Board board = FindObjectOfType<Board>();
        float boardWidth = board.columns * board.tileSpacing;
        float startX = -boardWidth / 2;
        float y = -board.rows * board.tileSpacing / 2 + row * board.tileSpacing;
        
        // Create beam effect GameObject
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.transform.position = new Vector3(0, y, -0.2f);
        beam.transform.localScale = new Vector3(boardWidth, 0.1f, 0.1f);
        
        // Add material/visuals here
        
        // Animate the beam
        yield return new WaitForSeconds(0.3f);
        
        // Clean up
        Destroy(beam);
    }
    
    // Similar implementations for ActivateColumnClear, ActivateAreaClear, and ActivateColorClear
    // would follow the pattern above with appropriate variations
    
    private IEnumerator ActivateColumnClear(Vector2Int position)
    {
        // Similar implementation to RowClear but vertical
        yield return null;
    }
    
    private IEnumerator ActivateAreaClear(Vector2Int position)
    {
        // Clear tiles in a 3x3 area around the position
        yield return null;
    }
    
    private IEnumerator ActivateColorClear(GameTile.TileColor color)
    {
        // Find and clear all tiles of the specified color
        yield return null;
    }
}
