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
        Debug.Log($"Activating special effect of type {specialType} at position {position}");
        
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
                // Enhanced value boost
                StartCoroutine(ActivateValueBoost());
                break;
                
            case SpecialTileType.Wildcard:
                // Wildcard can match with any color - this is handled differently
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
        GameObject beamEffect = CreateBeamEffect(true, row);
        
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
        
        // Wait for effect to complete
        yield return new WaitForSeconds(0.3f);
        
        // Remove the beam effect
        if (beamEffect != null)
            Destroy(beamEffect);
        
        // Create a sparkle effect at each position to be removed
        foreach (Vector2Int pos in positionsToRemove)
        {
            // Create a sparkle effect at this position
            CreateSparkleEffect(tileManager.GetWorldPositionFromGrid(pos));
            
            // Wait a tiny bit for cascading visual
            yield return new WaitForSeconds(0.05f);
            
            // Remove the tile
            tileManager.RemoveTileAt(pos);
        }
        
        // Notify score manager of special move
        if (ScoreManager.Instance != null)
        {
            int pointValue = 150 * positionsToRemove.Count;
            ScoreManager.Instance.OnSpecialMove(pointValue);
            
            // Show floating score text
            ShowFloatingScoreText(transform.position, pointValue);
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
        TileManager tileManager = TileManager.Instance;
        if (tileManager == null) yield break;
        
        int column = position.x;
        int rows = FindObjectOfType<Board>().rows;
        
        // Visual effect before clearing
        GameObject beamEffect = CreateBeamEffect(false, column);
        
        // Collect all positions in this column
        List<Vector2Int> positionsToRemove = new List<Vector2Int>();
        for (int row = 0; row < rows; row++)
        {
            Vector2Int tilePos = new Vector2Int(column, row);
            if (tileManager.IsTileAt(tilePos) && tilePos != position)
            {
                positionsToRemove.Add(tilePos);
            }
        }
        
        // Wait for effect to complete
        yield return new WaitForSeconds(0.3f);
        
        // Remove the beam effect
        if (beamEffect != null)
            Destroy(beamEffect);
        
        // Create a sparkle effect at each position to be removed
        foreach (Vector2Int pos in positionsToRemove)
        {
            // Create a sparkle effect at this position
            CreateSparkleEffect(tileManager.GetWorldPositionFromGrid(pos));
            
            // Wait a tiny bit for cascading visual
            yield return new WaitForSeconds(0.05f);
            
            // Remove the tile
            tileManager.RemoveTileAt(pos);
        }
        
        // Notify score manager of special move
        if (ScoreManager.Instance != null)
        {
            int pointValue = 150 * positionsToRemove.Count;
            ScoreManager.Instance.OnSpecialMove(pointValue);
            
            // Show floating score text
            ShowFloatingScoreText(transform.position, pointValue);
        }
    }
    
    private IEnumerator ActivateAreaClear(Vector2Int position)
    {
        TileManager tileManager = TileManager.Instance;
        if (tileManager == null) yield break;
        
        // Get world position for effects
        Vector3 worldPos = tileManager.GetWorldPositionFromGrid(position);
        
        // Create expanding circle effect
        GameObject circleEffect = CreateExpandingCircleEffect(worldPos);
        
        // Collect all positions in 3x3 area
        List<Vector2Int> positionsToRemove = new List<Vector2Int>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int tilePos = new Vector2Int(position.x + dx, position.y + dy);
                
                // Skip the center position (where the special tile is)
                if (dx == 0 && dy == 0) continue;
                
                // Make sure the position is on the board
                if (tilePos.x >= 0 && tilePos.x < FindObjectOfType<Board>().columns &&
                    tilePos.y >= 0 && tilePos.y < FindObjectOfType<Board>().rows)
                {
                    if (tileManager.IsTileAt(tilePos))
                    {
                        positionsToRemove.Add(tilePos);
                    }
                }
            }
        }
        
        // Wait for effect to complete
        yield return new WaitForSeconds(0.5f);
        
        // Remove the circle effect
        if (circleEffect != null)
            Destroy(circleEffect);
        
        // Create a sparkle effect at each position to be removed
        foreach (Vector2Int pos in positionsToRemove)
        {
            // Create a sparkle effect at this position
            CreateSparkleEffect(tileManager.GetWorldPositionFromGrid(pos));
            
            // Wait a tiny bit for cascading visual
            yield return new WaitForSeconds(0.03f);
            
            // Remove the tile
            tileManager.RemoveTileAt(pos);
        }
        
        // Notify score manager of special move
        if (ScoreManager.Instance != null)
        {
            int pointValue = 200 * positionsToRemove.Count; // Higher value for area clear
            ScoreManager.Instance.OnSpecialMove(pointValue);
            
            // Show floating score text
            ShowFloatingScoreText(transform.position, pointValue);
        }
    }
    
    private IEnumerator ActivateColorClear(GameTile.TileColor color)
    {
        TileManager tileManager = TileManager.Instance;
        if (tileManager == null) yield break;
        
        // Create color flash effect
        GameObject flashEffect = CreateColorFlashEffect(color);
        
        // Find all tiles of the given color
        List<Vector2Int> positionsToRemove = new List<Vector2Int>();
        
        // Get all tiles on the board
        List<Vector2Int> allPositions = tileManager.GetAllTilePositions();
        
        foreach (Vector2Int pos in allPositions)
        {
            GameObject tile = tileManager.GetTileAt(pos);
            if (tile != null)
            {
                GameTile gameTile = tile.GetComponent<GameTile>();
                if (gameTile != null && gameTile.CurrentColor == color)
                {
                    // Don't remove the special tile itself
                    if (gameTile.gameObject != this.gameObject)
                    {
                        positionsToRemove.Add(pos);
                    }
                }
            }
        }
        
        // Wait for effect to complete
        yield return new WaitForSeconds(0.3f);
        
        // Remove the flash effect
        if (flashEffect != null)
            Destroy(flashEffect);
        
        // Create a sparkle effect at each position to be removed
        foreach (Vector2Int pos in positionsToRemove)
        {
            // Create a sparkle effect at this position
            CreateSparkleEffect(tileManager.GetWorldPositionFromGrid(pos));
            
            // Wait a tiny bit for cascading visual
            yield return new WaitForSeconds(0.02f);
            
            // Remove the tile
            tileManager.RemoveTileAt(pos);
        }
        
        // Notify score manager of special move
        if (ScoreManager.Instance != null)
        {
            int pointValue = 250 * positionsToRemove.Count; // Highest value for color clear
            ScoreManager.Instance.OnSpecialMove(pointValue);
            
            // Show floating score text
            ShowFloatingScoreText(transform.position, pointValue);
        }
    }
    
    private IEnumerator ActivateValueBoost()
    {
        // Apply bonus to player when this tile is used in a match
        if (gameTile != null)
        {
            // Double the tile's value when merged
            int boostedValue = gameTile.TileValue * 2;
            gameTile.SetValue(boostedValue);
            
            // Create a visual effect to show the boost
            GameObject boostEffect = CreateBoostEffect(transform.position);
            
            // Show floating text indicating the boost
            ShowFloatingScoreText(transform.position, boostedValue, "x2");
            
            yield return new WaitForSeconds(0.5f);
            
            // Remove boost effect
            if (boostEffect != null)
                Destroy(boostEffect);
        }
    }
    
    // Helper methods to create visual effects
    
    private GameObject CreateBeamEffect(bool isHorizontal, int index)
    {
        Board board = FindObjectOfType<Board>();
        if (board == null) return null;
        
        float boardWidth = board.columns * board.tileSpacing;
        float boardHeight = board.rows * board.tileSpacing;
        
        // Create beam effect GameObject
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        if (isHorizontal)
        {
            // Horizontal beam (row clear)
            float startX = -boardWidth / 2;
            float y = -boardHeight / 2 + index * board.tileSpacing;
            
            beam.transform.position = new Vector3(0, y, -0.2f);
            beam.transform.localScale = new Vector3(boardWidth, 0.15f, 0.1f);
            
            // Set color
            MeshRenderer renderer = beam.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.6f, 0.2f, 0.7f); // Orange, semi-transparent
            }
        }
        else
        {
            // Vertical beam (column clear)
            float x = -boardWidth / 2 + index * board.tileSpacing;
            float startY = -boardHeight / 2;
            
            beam.transform.position = new Vector3(x, 0, -0.2f);
            beam.transform.localScale = new Vector3(0.15f, boardHeight, 0.1f);
            
            // Set color
            MeshRenderer renderer = beam.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.2f, 0.6f, 1f, 0.7f); // Light blue, semi-transparent
            }
        }
        
        return beam;
    }
    
    private GameObject CreateExpandingCircleEffect(Vector3 position)
    {
        // Create a quad for the circle
        GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Quad);
        circle.transform.position = new Vector3(position.x, position.y, position.z - 0.1f);
        
        // Set color
        MeshRenderer renderer = circle.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 0.2f, 0.8f, 0.7f); // Pink, semi-transparent
        }
        
        // Start with small scale
        circle.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
        
        // Animate scale
        StartCoroutine(AnimateCircleScale(circle));
        
        return circle;
    }
    
    private IEnumerator AnimateCircleScale(GameObject circle)
    {
        if (circle == null) yield break;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        // Target scale for 3x3 area
        Vector3 targetScale = new Vector3(3f, 3f, 1f);
        
        while (elapsed < duration)
        {
            if (circle == null) yield break;
            
            float t = elapsed / duration;
            circle.transform.localScale = Vector3.Lerp(new Vector3(0.1f, 0.1f, 1f), targetScale, t);
            
            // Also fade out as it expands
            MeshRenderer renderer = circle.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                renderer.material.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0.7f, 0f, t));
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private GameObject CreateColorFlashEffect(GameTile.TileColor color)
    {
        // Create a full-screen quad for the flash
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Quad);
        
        // Set position in front of everything
        flash.transform.position = new Vector3(0, 0, -5f);
        
        // Set scale to cover the screen
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float height = 2f * mainCamera.orthographicSize;
            float width = height * mainCamera.aspect;
            flash.transform.localScale = new Vector3(width, height, 1f);
        }
        else
        {
            flash.transform.localScale = new Vector3(20f, 20f, 1f);
        }
        
        // Set color based on the tile color
        MeshRenderer renderer = flash.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Color flashColor = GetColorForTileColor(color);
            flashColor.a = 0.3f; // Semi-transparent
            renderer.material.color = flashColor;
        }
        
        // Animate flash fade out
        StartCoroutine(AnimateFlashFade(flash));
        
        return flash;
    }
    
    private Color GetColorForTileColor(GameTile.TileColor tileColor)
    {
        switch (tileColor)
        {
            case GameTile.TileColor.Red:
                return new Color(1f, 0.2f, 0.2f);
            case GameTile.TileColor.Green:
                return new Color(0.2f, 1f, 0.2f);
            case GameTile.TileColor.Blue:
                return new Color(0.2f, 0.5f, 1f);
            case GameTile.TileColor.Yellow:
                return new Color(1f, 0.92f, 0.2f);
            default:
                return Color.white;
        }
    }
    
    private IEnumerator AnimateFlashFade(GameObject flash)
    {
        if (flash == null) yield break;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        MeshRenderer renderer = flash.GetComponent<MeshRenderer>();
        if (renderer == null) yield break;
        
        Color startColor = renderer.material.color;
        
        while (elapsed < duration)
        {
            if (flash == null) yield break;
            
            float t = elapsed / duration;
            Color color = renderer.material.color;
            renderer.material.color = new Color(color.r, color.g, color.b, Mathf.Lerp(startColor.a, 0f, t));
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private GameObject CreateBoostEffect(Vector3 position)
    {
        // Create a simple particle effect for the boost
        GameObject boost = new GameObject("BoostEffect");
        boost.transform.position = position;
        
        // Add a particle system if available
        // For now, just create a simple visual effect
        GameObject visualIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visualIndicator.transform.SetParent(boost.transform);
        visualIndicator.transform.localPosition = Vector3.zero;
        visualIndicator.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        // Set color
        MeshRenderer renderer = visualIndicator.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.2f, 1f, 0.4f, 0.7f); // Green, semi-transparent
        }
        
        // Animate pulsing
        StartCoroutine(AnimateBoostPulse(visualIndicator));
        
        return boost;
    }
    
    private IEnumerator AnimateBoostPulse(GameObject indicator)
    {
        if (indicator == null) yield break;
        
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = indicator.transform.localScale;
        Vector3 maxScale = startScale * 2f;
        
        while (elapsed < duration)
        {
            if (indicator == null) yield break;
            
            float t = elapsed / duration;
            float pulse = Mathf.Sin(t * Mathf.PI);
            indicator.transform.localScale = Vector3.Lerp(startScale, maxScale, pulse);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private void CreateSparkleEffect(Vector3 position)
    {
        // In a full implementation, you would instantiate a particle system
        // For now, just output a debug log
        Debug.Log($"Creating sparkle effect at {position}");
        
        // Simple placeholder effect
        GameObject sparkle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sparkle.transform.position = position;
        sparkle.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        
        // Set color
        MeshRenderer renderer = sparkle.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 1f, 1f, 0.8f); // White, semi-transparent
        }
        
        // Destroy after a delay
        Destroy(sparkle, 0.3f);
    }
    
    private void ShowFloatingScoreText(Vector3 position, int points, string prefix = "")
    {
        // Create a text mesh for the score
        GameObject scoreTextObj = new GameObject("ScoreText");
        scoreTextObj.transform.position = position;
        
        // Add TextMesh component
        TextMesh textMesh = scoreTextObj.AddComponent<TextMesh>();
        textMesh.text = prefix + points.ToString();
        textMesh.fontSize = 14;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.yellow;
        
        // Animate moving up and fading out
        StartCoroutine(AnimateFloatingText(scoreTextObj));
    }
    
    private IEnumerator AnimateFloatingText(GameObject textObj)
    {
        if (textObj == null) yield break;
        
        float duration = 1.0f;
        float elapsed = 0f;
        Vector3 startPos = textObj.transform.position;
        Vector3 endPos = startPos + new Vector3(0, 1f, 0); // Move up 1 unit
        
        TextMesh textMesh = textObj.GetComponent<TextMesh>();
        if (textMesh == null) yield break;
        
        while (elapsed < duration)
        {
            if (textObj == null) yield break;
            
            float t = elapsed / duration;
            textObj.transform.position = Vector3.Lerp(startPos, endPos, t);
            
            // Fade out near the end
            if (t > 0.5f)
            {
                float fadeT = (t - 0.5f) * 2f; // Remap 0.5-1 to 0-1
                textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 
                                          Mathf.Lerp(1f, 0f, fadeT));
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Destroy the text object
        Destroy(textObj);
    }
}
