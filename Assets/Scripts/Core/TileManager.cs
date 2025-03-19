using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [Header("Tile References")]
    [SerializeField] private GameObject gameTilePrefab;
    [SerializeField] private Board boardManager;
    
    [Header("Tile Settings")]
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 50;
    
    // Tile tracking
    private Dictionary<Vector2Int, GameObject> tilePositions = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<GameObject, Vector2Int> tilesToPositions = new Dictionary<GameObject, Vector2Int>();
    
    // Object pooling
    private Queue<GameObject> redTilePool = new Queue<GameObject>();
    private Queue<GameObject> greenTilePool = new Queue<GameObject>();
    private Queue<GameObject> blueTilePool = new Queue<GameObject>();
    private Queue<GameObject> yellowTilePool = new Queue<GameObject>();
    
    // Singleton instance
    public static TileManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        if (boardManager == null)
            boardManager = FindObjectOfType<Board>();
            
        InitializeTilePools();
    }
    
    #region Tile Pool Management
    
    private void InitializeTilePools()
    {
        // Initialize pools for each tile color
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateTileForPool(GameTile.TileColor.Red);
            CreateTileForPool(GameTile.TileColor.Green);
            CreateTileForPool(GameTile.TileColor.Blue);
            CreateTileForPool(GameTile.TileColor.Yellow);
        }
    }
    
    private void CreateTileForPool(GameTile.TileColor color)
    {
        GameObject tile = Instantiate(gameTilePrefab, Vector3.zero, Quaternion.identity, transform);
        tile.SetActive(false);
        
        GameTile gameTile = tile.GetComponent<GameTile>();
        if (gameTile != null)
        {
            gameTile.Initialize(color);
        }
        
        // Add to the appropriate pool
        AddTileToPool(tile, color);
    }
    
    private void AddTileToPool(GameObject tile, GameTile.TileColor color)
    {
        switch (color)
        {
            case GameTile.TileColor.Red:
                redTilePool.Enqueue(tile);
                break;
            case GameTile.TileColor.Green:
                greenTilePool.Enqueue(tile);
                break;
            case GameTile.TileColor.Blue:
                blueTilePool.Enqueue(tile);
                break;
            case GameTile.TileColor.Yellow:
                yellowTilePool.Enqueue(tile);
                break;
        }
    }
    
    private GameObject GetTileFromPool(GameTile.TileColor color)
    {
        Queue<GameObject> pool = GetPoolForColor(color);
        
        if (pool.Count == 0)
        {
            // Create a new tile if the pool is empty (up to maxPoolSize)
            if (pool.Count + GetActiveTileCount(color) < maxPoolSize)
            {
                CreateTileForPool(color);
            }
        }
        
        if (pool.Count > 0)
        {
            GameObject tile = pool.Dequeue();
            tile.SetActive(true);
            return tile;
        }
        
        // If we reach here, we've hit the max pool size and all tiles are in use
        // Create a temporary tile that won't be pooled
        Debug.LogWarning($"Max pool size reached for {color} tiles. Creating a temporary tile.");
        GameObject tempTile = Instantiate(gameTilePrefab, Vector3.zero, Quaternion.identity, transform);
        GameTile gameTile = tempTile.GetComponent<GameTile>();
        if (gameTile != null)
        {
            gameTile.Initialize(color);
        }
        return tempTile;
    }
    
    private Queue<GameObject> GetPoolForColor(GameTile.TileColor color)
    {
        switch (color)
        {
            case GameTile.TileColor.Red:
                return redTilePool;
            case GameTile.TileColor.Green:
                return greenTilePool;
            case GameTile.TileColor.Blue:
                return blueTilePool;
            case GameTile.TileColor.Yellow:
                return yellowTilePool;
            default:
                return redTilePool;
        }
    }
    
    private int GetActiveTileCount(GameTile.TileColor color)
    {
        int count = 0;
        foreach (var tile in tilesToPositions.Keys)
        {
            GameTile gameTile = tile.GetComponent<GameTile>();
            if (gameTile != null && gameTile.CurrentColor == color)
            {
                count++;
            }
        }
        return count;
    }
    
    private void ReturnTileToPool(GameObject tile)
    {
        if (tile == null) return;
        
        // Get the tile's color
        GameTile gameTile = tile.GetComponent<GameTile>();
        if (gameTile == null) return;
        
        // Return to the appropriate pool
        tile.SetActive(false);
        tile.transform.SetParent(transform);
        AddTileToPool(tile, gameTile.CurrentColor);
    }
    
    #endregion
    
    #region Tile Placement and Grid Management
    
    public GameObject CreateTileAt(Vector2Int gridPosition, GameTile.TileColor color)
    {
        if (IsTileAt(gridPosition))
        {
            Debug.LogWarning($"Tile already exists at position {gridPosition}");
            return null;
        }
        
        // Get a tile from the pool
        GameObject tile = GetTileFromPool(color);
        
        // Convert grid position to world position
        Vector3 worldPosition = GetWorldPositionFromGrid(gridPosition);
        
        // Position the tile
        tile.transform.position = worldPosition;
        tile.transform.SetParent(boardManager.transform);
        
        // Set the tile's scale
        tile.transform.localScale = new Vector3(boardManager.tileSize, boardManager.tileSize, 1f);
        
        // Ensure proper z-position to be above the board
        Vector3 pos = tile.transform.position;
        tile.transform.position = new Vector3(pos.x, pos.y, -0.1f);
        
        // Track the tile
        tilePositions[gridPosition] = tile;
        tilesToPositions[tile] = gridPosition;
        
        // Ensure the SpriteRenderer has a higher sorting order
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 10;
        }
        
        // Set the tile as movable since it's a game tile
        Tile tileComponent = tile.GetComponent<Tile>();
        if (tileComponent != null)
        {
            tileComponent.SetMovable(true);
        }
        
        return tile;
    }
    
    private Vector3 GetWorldPositionFromGrid(Vector2Int gridPosition)
    {
        float boardWidth = (boardManager.columns - 1) * boardManager.tileSpacing;
        float boardHeight = (boardManager.rows - 1) * boardManager.tileSpacing;
        
        float startX = -boardWidth / 2;
        float startY = -boardHeight / 2;
        
        return new Vector3(
            startX + (gridPosition.x * boardManager.tileSpacing), 
            startY + (gridPosition.y * boardManager.tileSpacing), 
            0
        );
    }
    
    private Vector2Int GetGridPositionFromWorld(Vector3 worldPosition)
    {
        float boardWidth = (boardManager.columns - 1) * boardManager.tileSpacing;
        float boardHeight = (boardManager.rows - 1) * boardManager.tileSpacing;
        
        float startX = -boardWidth / 2;
        float startY = -boardHeight / 2;
        
        int gridX = Mathf.RoundToInt((worldPosition.x - startX) / boardManager.tileSpacing);
        int gridY = Mathf.RoundToInt((worldPosition.y - startY) / boardManager.tileSpacing);
        
        return new Vector2Int(gridX, gridY);
    }
    
    public bool IsTileAt(Vector2Int gridPosition)
    {
        return tilePositions.ContainsKey(gridPosition);
    }
    
    public GameObject GetTileAt(Vector2Int gridPosition)
    {
        if (IsTileAt(gridPosition))
        {
            return tilePositions[gridPosition];
        }
        return null;
    }
    
    public Vector2Int? GetTilePosition(GameObject tile)
    {
        if (tilesToPositions.ContainsKey(tile))
        {
            return tilesToPositions[tile];
        }
        return null;
    }
    
    public void RemoveTileAt(Vector2Int gridPosition)
    {
        if (!IsTileAt(gridPosition)) return;
        
        GameObject tile = tilePositions[gridPosition];
        tilePositions.Remove(gridPosition);
        tilesToPositions.Remove(tile);
        
        ReturnTileToPool(tile);
    }
    
    public bool MoveTile(GameObject tile, Vector2Int newGridPosition)
    {
        if (!tilesToPositions.ContainsKey(tile)) 
        {
            Debug.LogError($"Attempted to move a tile that's not being tracked: {tile.name}");
            return false;
        }
        
        Vector2Int oldGridPosition = tilesToPositions[tile];
        
        // Check if there's a tile at the target position
        if (tilePositions.ContainsKey(newGridPosition))
        {
            // Check if we can merge with the target tile (same color)
            GameObject targetTile = tilePositions[newGridPosition];
            GameTile movingGameTile = tile.GetComponent<GameTile>();
            GameTile targetGameTile = targetTile.GetComponent<GameTile>();
            
            if (movingGameTile != null && targetGameTile != null && 
                movingGameTile.CurrentColor == targetGameTile.CurrentColor)
            {
                // Merge the tiles by adding their values
                int newValue = movingGameTile.TileValue + targetGameTile.TileValue;
                
                // Remove the moving tile
                tilePositions.Remove(oldGridPosition);
                tilesToPositions.Remove(tile);
                ReturnTileToPool(tile);
                
                // Update the target tile's value
                targetGameTile.SetValue(newValue);
                
                // Play merge animation
                StartCoroutine(MergeTileAnimation(targetTile));
                
                // Notify score manager about the merge
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.OnMatchMade(2); // 2 tiles merged
                }
                
                // Notify GameManager to spawn a new tile after merger
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnTileMovementComplete();
                }
                
                return true;
            }
            else
            {
                Debug.LogWarning($"Cannot move to position {newGridPosition} - already occupied by {tilePositions[newGridPosition].name}");
                return false;
            }
        }
        
        // No tile at target position, proceed with normal movement
        // Update tracking dictionaries
        if (tilePositions.ContainsKey(oldGridPosition))
        {
            tilePositions.Remove(oldGridPosition);
        }
        else
        {
            Debug.LogError($"Inconsistency detected: Tile at {oldGridPosition} not found in tilePositions");
        }
        
        tilePositions[newGridPosition] = tile;
        tilesToPositions[tile] = newGridPosition;
        
        Debug.Log($"Updated tracking: Moved tile from {oldGridPosition} to {newGridPosition}");
        
        // Move the tile
        Vector3 newWorldPosition = GetWorldPositionFromGrid(newGridPosition);
        StartCoroutine(MoveTileAnimation(tile, newWorldPosition));
        
        return true;
    }
    
    // Animation for merging tiles
    private IEnumerator MergeTileAnimation(GameObject tile)
    {
        // Scale up and down animation
        Vector3 originalScale = tile.transform.localScale;
        Vector3 expandedScale = originalScale * 1.2f;
        
        // Scale up
        float duration = 0.1f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            tile.transform.localScale = Vector3.Lerp(originalScale, expandedScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Scale back down
        elapsed = 0;
        while (elapsed < duration)
        {
            tile.transform.localScale = Vector3.Lerp(expandedScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final scale is correct
        tile.transform.localScale = originalScale;
    }
    
    private IEnumerator MoveTileAnimation(GameObject tile, Vector3 targetPosition)
    {
        TileMovement tileMovement = tile.GetComponent<TileMovement>();
        if (tileMovement != null)
        {
            Vector3 startPos = tile.transform.position;
            float moveTime = 0.2f;
            float elapsedTime = 0;
            
            while (elapsedTime < moveTime)
            {
                tile.transform.position = Vector3.Lerp(startPos, targetPosition, elapsedTime / moveTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Ensure tile ends at exact position
            tile.transform.position = new Vector3(targetPosition.x, targetPosition.y, tile.transform.position.z);
        }
        else
        {
            // Fallback if no TileMovement component
            tile.transform.position = new Vector3(targetPosition.x, targetPosition.y, tile.transform.position.z);
        }
        
        // After completing movement, notify GameManager to spawn a new tile
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTileMovementComplete();
        }
    }
    
    #endregion
    
    #region Tile Merging and Matching
    
    public bool CheckForMatches(Vector2Int position)
    {
        if (!IsTileAt(position)) return false;
        
        // Get the tile at this position
        GameObject tile = GetTileAt(position);
        GameTile gameTile = tile.GetComponent<GameTile>();
        if (gameTile == null) return false;
        
        // Check for matching neighbors
        List<Vector2Int> matchedPositions = FindMatchingNeighbors(position, gameTile.CurrentColor);
        
        // If we have enough matches, merge them
        if (matchedPositions.Count >= 2) // 3 tiles total (including the original)
        {
            MergeTiles(matchedPositions, gameTile.CurrentColor);
            return true;
        }
        
        return false;
    }
    
    private List<Vector2Int> FindMatchingNeighbors(Vector2Int startPos, GameTile.TileColor color)
    {
        List<Vector2Int> matches = new List<Vector2Int>();
        Queue<Vector2Int> toCheck = new Queue<Vector2Int>();
        HashSet<Vector2Int> checked_Positions = new HashSet<Vector2Int>();
        
        // Start with the initial position
        toCheck.Enqueue(startPos);
        checked_Positions.Add(startPos);
        
        // Check all connected tiles of the same color
        while (toCheck.Count > 0)
        {
            Vector2Int current = toCheck.Dequeue();
            
            // Check each direction
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),  // Up
                new Vector2Int(0, -1), // Down
                new Vector2Int(-1, 0), // Left
                new Vector2Int(1, 0)   // Right
            };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;
                
                // Skip if already checked
                if (checked_Positions.Contains(neighbor)) continue;
                
                // Check if there's a matching tile here
                if (IsTileAt(neighbor))
                {
                    GameObject neighborTile = GetTileAt(neighbor);
                    GameTile neighborGameTile = neighborTile.GetComponent<GameTile>();
                    
                    if (neighborGameTile != null && neighborGameTile.CurrentColor == color)
                    {
                        // Found a match!
                        matches.Add(neighbor);
                        
                        // Add to queue to check its neighbors
                        toCheck.Enqueue(neighbor);
                    }
                }
                
                // Mark as checked
                checked_Positions.Add(neighbor);
            }
        }
        
        return matches;
    }
    
    private void MergeTiles(List<Vector2Int> positions, GameTile.TileColor currentColor)
    {
        if (positions.Count < 2) return;
        
        // Determine the target color for the merged tile
        GameTile.TileColor nextColor = GetNextColor(currentColor);
        
        // Calculate the average position for the merged tile
        Vector2Int mergePosition = positions[0];
        
        // Get the original tile value to calculate new value
        GameObject originalTile = GetTileAt(mergePosition);
        int originalValue = 2; // Default value
        
        if (originalTile != null)
        {
            GameTile gameTile = originalTile.GetComponent<GameTile>();
            if (gameTile != null)
            {
                originalValue = gameTile.TileValue;
            }
        }
        
        // Calculate new value by adding the values
        int newValue = originalValue + positions.Count;
        
        // Cap the value to prevent it from growing too large
        newValue = Mathf.Min(newValue, 12);
        
        // Remove all matched tiles
        foreach (Vector2Int pos in positions)
        {
            RemoveTileAt(pos);
        }
        
        // Create the merged tile with new value
        GameObject mergedTile = CreateTileAt(mergePosition, nextColor);
        if (mergedTile != null)
        {
            GameTile mergedGameTile = mergedTile.GetComponent<GameTile>();
            if (mergedGameTile != null)
            {
                mergedGameTile.SetValue(newValue);
            }
        }
        
        // Trigger effects
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnMatchMade(positions.Count + 1); // +1 for the original tile
        }
    }
    
    private GameTile.TileColor GetNextColor(GameTile.TileColor currentColor)
    {
        // This is a simple implementation - you might want to design a different progression
        switch (currentColor)
        {
            case GameTile.TileColor.Red:
                return GameTile.TileColor.Green;
            case GameTile.TileColor.Green:
                return GameTile.TileColor.Blue;
            case GameTile.TileColor.Blue:
                return GameTile.TileColor.Yellow;
            case GameTile.TileColor.Yellow:
                return GameTile.TileColor.Red;
            default:
                return GameTile.TileColor.Red;
        }
    }
    
    #endregion
    
    #region Board Management
    
    public void ClearBoard()
    {
        // Make a copy of keys to avoid modification while iterating
        List<Vector2Int> positions = new List<Vector2Int>(tilePositions.Keys);
        
        foreach (Vector2Int pos in positions)
        {
            RemoveTileAt(pos);
        }
    }
    
    public void GenerateRandomTile()
    {
        // Find an empty spot on the board
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        
        // Print all currently occupied positions for debugging
        Debug.Log("Currently occupied positions:");
        foreach (Vector2Int pos in tilePositions.Keys)
        {
            Debug.Log($"  Occupied: {pos} by {tilePositions[pos].name}");
        }
        
        for (int row = 0; row < boardManager.rows; row++)
        {
            for (int col = 0; col < boardManager.columns; col++)
            {
                Vector2Int position = new Vector2Int(col, row);
                if (!tilePositions.ContainsKey(position))
                {
                    // Extra verification: physically check for overlap
                    bool positionClear = true;
                    Vector3 worldPos = GetWorldPositionFromGrid(position);
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPos.x, worldPos.y), 0.1f);
                    
                    foreach (Collider2D collider in colliders)
                    {
                        if (collider.GetComponent<GameTile>() != null)
                        {
                            positionClear = false;
                            Debug.LogWarning($"Found tile at {position} through physics check that wasn't in dictionary!");
                            break;
                        }
                    }
                    
                    if (positionClear)
                    {
                        emptyPositions.Add(position);
                    }
                }
            }
        }
        
        if (emptyPositions.Count == 0)
        {
            Debug.LogWarning("No empty spaces for new tiles!");
            return;
        }
        
        // Select a random empty position
        Vector2Int randomPosition = emptyPositions[Random.Range(0, emptyPositions.Count)];
        
        // Final verification before spawning
        if (tilePositions.ContainsKey(randomPosition))
        {
            Debug.LogError($"CRITICAL ERROR: Position {randomPosition} is already occupied in dictionary!");
            return;
        }
        
        // Keep this log for tracking tile spawning
        Debug.Log($"Spawning new tile at position {randomPosition}");
        
        // Select a random color
        GameTile.TileColor randomColor = (GameTile.TileColor)Random.Range(0, 4);
        
        // Create the tile with a value of 2 initially
        GameObject newTile = CreateTileAt(randomPosition, randomColor);
        
        // Verify the tile was created properly
        if (newTile != null)
        {
            // Randomize the initial value between 1 and 6
            GameTile gameTile = newTile.GetComponent<GameTile>();
            if (gameTile != null)
            {
                // Generate a random value between 1 and 6
                int randomValue = Random.Range(1, 7); // Random.Range is exclusive for the upper bound with integers
                gameTile.SetValue(randomValue);
                
                Debug.Log($"Successfully created {randomColor} tile with value {randomValue} at position {randomPosition}");
            }
            
            StartCoroutine(ScaleTileIn(newTile));
        }
        else
        {
            Debug.LogError($"Failed to create tile at position {randomPosition}");
        }
    }
    
    private IEnumerator ScaleTileIn(GameObject tile)
    {
        // Start with a tiny scale
        Vector3 originalScale = tile.transform.localScale;
        tile.transform.localScale = originalScale * 0.1f;
        
        float duration = 0.3f;
        float elapsed = 0;
        
        // Scale up to normal size
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            tile.transform.localScale = Vector3.Lerp(originalScale * 0.1f, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final scale is correct
        tile.transform.localScale = originalScale;
    }
    
    #endregion
}
