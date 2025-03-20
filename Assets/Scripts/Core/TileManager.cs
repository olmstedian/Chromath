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
    // Add the missing physicsTiles dictionary
    private Dictionary<Vector2Int, GameObject> physicsTiles = new Dictionary<Vector2Int, GameObject>();
    
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
        
        if (boardManager == null)
        {
            Debug.LogError("TileManager could not find Board reference! Tiles cannot be placed.");
            return;
        }
            
        InitializeTilePools();
        
        // Debug log to ensure TileManager started properly
        Debug.Log($"TileManager initialized with board: {boardManager.name}, dimensions: {boardManager.rows}x{boardManager.columns}");
        
        // Test tile generation on startup
        if (GameManager.Instance == null) // Only do this if GameManager isn't handling it
        {
            StartCoroutine(GenerateTestTile());
        }
    }

    // Add this method to allow Board to notify TileManager when it's ready
    public void OnBoardReady(Board board)
    {
        boardManager = board;
        Debug.Log("TileManager received board ready notification");
    }

    private IEnumerator GenerateTestTile()
    {
        // Wait a bit to ensure the board is fully set up
        yield return new WaitForSeconds(0.5f);
        
        // Generate a test tile to verify functionality
        GenerateRandomTile();
        Debug.Log("Generated test tile to verify TileManager is working");
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
        if (boardManager == null)
        {
            Debug.LogError("Cannot create tile - Board reference is missing!");
            return null;
        }
        
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
        
        // Ensure proper z-position to be above the board
        Vector3 pos = tile.transform.position;
        tile.transform.position = new Vector3(pos.x, pos.y, -0.1f);
        
        // Set the tile's scale based on board's tileSize
        tile.transform.localScale = new Vector3(boardManager.tileSize, boardManager.tileSize, 1f);
        
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
        
        // Log successful tile creation
        Debug.Log($"Successfully created tile at grid position {gridPosition}, world position {worldPosition}");
        
        return tile;
    }
    
    // Helper method to get all tile positions on the board
    public List<Vector2Int> GetAllTilePositions()
    {
        return new List<Vector2Int>(tilePositions.Keys);
    }

    // Helper method to get world position from grid position
    public Vector3 GetWorldPositionFromGrid(Vector2Int gridPosition)
    {
        if (boardManager == null)
        {
            Debug.LogError("Board reference is missing in TileManager!");
            return Vector3.zero;
        }

        // Use Board's built-in method to get the world position
        return boardManager.GetWorldPosition(gridPosition);
    }

    private Vector2Int GetGridPositionFromWorld(Vector3 worldPosition)
    {
        if (boardManager == null)
        {
            Debug.LogError("Board reference is missing in TileManager!");
            return Vector2Int.zero;
        }

        // Calculate grid position using the board's properties
        float tileSpacing = boardManager.tileSpacing;
        int gridX = Mathf.RoundToInt((worldPosition.x - boardManager.boardStartX) / tileSpacing);
        int gridY = Mathf.RoundToInt((worldPosition.y - boardManager.boardStartY) / tileSpacing);
        
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
        // Safety check for basic validations
        if (tile == null)
        {
            Debug.LogError("Attempted to move a null tile");
            return false;
        }
        
        if (!tilesToPositions.ContainsKey(tile)) 
        {
            Debug.LogError($"Attempted to move a tile that's not being tracked: {tile.name}");
            return false;
        }
        
        // Validate grid positions are within bounds
        if (newGridPosition.x < 0 || newGridPosition.x >= boardManager.columns ||
            newGridPosition.y < 0 || newGridPosition.y >= boardManager.rows)
        {
            Debug.LogWarning($"Attempted to move tile to out-of-bounds position: {newGridPosition}");
            return false;
        }
        
        Vector2Int oldGridPosition = tilesToPositions[tile];
        
        // Check if there's a tile at the target position
        if (tilePositions.ContainsKey(newGridPosition))
        {
            // Check if we can merge with the target tile (same color)
            GameObject targetTile = tilePositions[newGridPosition];
            if (targetTile == null)
            {
                // Target tile reference is null, remove it and allow movement
                Debug.LogWarning($"Found null tile reference at {newGridPosition} - cleaning up");
                tilePositions.Remove(newGridPosition);
            }
            else
            {
                // Get GameTile components to check for same color
                GameTile movingGameTile = tile.GetComponent<GameTile>();
                GameTile targetGameTile = targetTile.GetComponent<GameTile>();
                
                if (movingGameTile != null && targetGameTile != null && 
                    movingGameTile.CurrentColor == targetGameTile.CurrentColor)
                {
                    // Merge the tiles by adding their values
                    int newValue = movingGameTile.TileValue + targetGameTile.TileValue;
                    
                    // Update dictionaries first
                    tilePositions.Remove(oldGridPosition);
                    tilesToPositions.Remove(tile);
                    
                    // Update the target tile's value
                    targetGameTile.SetValue(newValue);
                    
                    // Play merge animation
                    if (TileAnimationManager.Instance != null)
                    {
                        StartCoroutine(TileAnimationManager.Instance.MergeTileAnimation(targetTile));
                    }
                    else
                    {
                        // Direct animation fallback if TileAnimationManager is unavailable
                        Debug.LogWarning("TileAnimationManager not available for merge animation");
                    }
                    
                    // Return the moving tile to the pool
                    ReturnTileToPool(tile);
                    
                    // Check for chain merges (safely)
                    StartCoroutine(CheckForChainMergesCoroutine(newGridPosition, targetGameTile.CurrentColor));
                    
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
                    // Instead of just warning, let's look for an alternate direction to move the tile
                    Debug.LogWarning($"Cannot move to position {newGridPosition} - already occupied by {targetTile.name}. Trying to find alternate path...");
                    
                    // Try adjacent positions if they're free
                    Vector2Int[] alternateDirections = new Vector2Int[]
                    {
                        Vector2Int.up,
                        Vector2Int.right,
                        Vector2Int.down,
                        Vector2Int.left
                    };
                    
                    // The direction we were trying to move
                    Vector2Int attemptedDirection = newGridPosition - oldGridPosition;
                    
                    // Try the perpendicular directions first (most intuitive for player)
                    foreach (Vector2Int dir in alternateDirections)
                    {
                        // Skip the direction we already tried
                        if (dir == attemptedDirection) continue;
                        
                        // Skip the opposite direction (would be confusing)
                        if (dir == -attemptedDirection) continue;
                        
                        Vector2Int alternatePosition = oldGridPosition + dir;
                        
                        // Check if the position is valid
                        if (alternatePosition.x >= 0 && alternatePosition.x < boardManager.columns &&
                            alternatePosition.y >= 0 && alternatePosition.y < boardManager.rows &&
                            !tilePositions.ContainsKey(alternatePosition))
                        {
                            // Found a free adjacent position - use it instead
                            Debug.Log($"Found alternate movement path to {alternatePosition}");
                            return MoveTile(tile, alternatePosition);
                        }
                    }
                    
                    // If no alternate path, return false
                    return false;
                }
            }
        }
        
        // No tile at target position, proceed with normal movement
        try
        {
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
            
            // Use TileAnimationManager instead of local coroutine
            if (TileAnimationManager.Instance != null)
            {
                StartCoroutine(TileAnimationManager.Instance.MoveTileAnimation(tile, newWorldPosition));
            }
            else
            {
                // Fallback to direct position change if animation manager not available
                tile.transform.position = newWorldPosition;
                
                // Still notify GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnTileMovementComplete();
                }
            }
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error moving tile: {e.Message}");
            return false;
        }
    }
    
    // Check for and process chain merges - replace with animation manager calls
    private IEnumerator CheckForChainMergesCoroutine(Vector2Int position, GameTile.TileColor color)
    {
        // Wait a short time before checking for chain merges to let animations complete
        yield return new WaitForSeconds(0.2f);
        
        // Safety check - verify position is valid
        if (!tilePositions.ContainsKey(position))
        {
            Debug.LogWarning($"Cannot check for chain merges at {position} - position no longer exists in dictionary");
            yield break;
        }
        
        // Get the merged tile to use as target
        GameObject mergedTile = tilePositions[position];
        
        if (mergedTile == null)
        {
            Debug.LogWarning($"Merged tile at {position} is null");
            yield break;
        }
        
        GameTile mergedGameTile = mergedTile.GetComponent<GameTile>();
        if (mergedGameTile == null)
        {
            Debug.LogWarning($"Merged tile at {position} has no GameTile component");
            yield break;
        }
        
        int originalValue = mergedGameTile.TileValue;
        
        // Track all tiles that will merge in this chain
        List<GameObject> tilesToMerge = new List<GameObject>();
        List<Vector2Int> positionsToMerge = new List<Vector2Int>();
        
        // Check in all four directions for same-color tiles
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int adjacentPos = position + dir;
            
            // Safety check - verify adjacent position is valid
            if (tilePositions.TryGetValue(adjacentPos, out GameObject adjacentTile) && adjacentTile != null)
            {
                GameTile adjacentGameTile = adjacentTile.GetComponent<GameTile>();
                
                if (adjacentGameTile != null && adjacentGameTile.CurrentColor == color)
                {
                    tilesToMerge.Add(adjacentTile);
                    positionsToMerge.Add(adjacentPos);
                    originalValue += adjacentGameTile.TileValue;
                }
            }
        }
        
        // If there are tiles to merge, perform the chain merge
        if (tilesToMerge.Count > 0)
        {
            Debug.Log($"Chain merging {tilesToMerge.Count} additional tiles with the merged tile");
            
            // Check if we have 3 or more tiles in total (including original)
            bool createSpecialTile = tilesToMerge.Count >= 2; // 3+ tiles total
            
            // Double check that merged tile still exists
            if (!tilePositions.ContainsKey(position) || tilePositions[position] == null)
            {
                Debug.LogWarning("Merged tile no longer exists - canceling chain merge");
                yield break;
            }

            // Get the total value from all merged tiles
            int totalValue = originalValue;
            
            // Process merged tiles
            foreach (var pos in positionsToMerge)
            {
                // Safety check - verify position is still valid
                if (tilePositions.TryGetValue(pos, out GameObject tileToRemove) && tileToRemove != null)
                {
                    tilePositions.Remove(pos);
                    tilesToPositions.Remove(tileToRemove);
                    
                    // Animate movement to the merged position using TileAnimationManager
                    if (TileAnimationManager.Instance != null)
                    {
                        StartCoroutine(TileAnimationManager.Instance.AnimateTileToMerge(
                            tileToRemove, GetWorldPositionFromGrid(position)));
                    }
                    
                    // Small delay for visual effect
                    yield return new WaitForSeconds(0.1f);
                }
            }
            
            // Special tile creation for 3+ merged tiles
            if (createSpecialTile)
            {
                // Determine which special tile to create based on merge pattern
                SpecialTileType specialType = DetermineSpecialTileType(position, positionsToMerge);
                
                // Update the merged tile to be a special tile
                GameTile targetTile = tilePositions[position].GetComponent<GameTile>();
                if (targetTile != null)
                {
                    targetTile.SetValue(totalValue);
                    targetTile.MakeSpecial(specialType);
                }
                
                // Apply visual effect for special tile creation using TileAnimationManager
                if (TileAnimationManager.Instance != null && tilePositions.ContainsKey(position) && tilePositions[position] != null)
                {
                    StartCoroutine(TileAnimationManager.Instance.SpecialTileCreationEffect(tilePositions[position]));
                }
            }
            else
            {
                // Regular value update for normal merges
                mergedGameTile.SetValue(totalValue);
            }
            
            // Play merge animation on the target tile if it still exists
            if (TileAnimationManager.Instance != null && tilePositions.ContainsKey(position) && tilePositions[position] != null)
            {
                StartCoroutine(TileAnimationManager.Instance.MergeTileAnimation(tilePositions[position]));
            }
            
            // Notify score manager about the chain merge
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnMatchMade(tilesToMerge.Count);
            }
            
            // Don't recursively check for more merges - too error-prone
            // Just run one level of chain merges and stop
        }
    }
    
    // Determine what kind of special tile to create based on merge pattern
    private SpecialTileType DetermineSpecialTileType(Vector2Int centerPos, List<Vector2Int> mergedPositions)
    {
        // Check for row pattern
        bool isRowPattern = true;
        int row = centerPos.y;
        foreach (Vector2Int pos in mergedPositions)
        {
            if (pos.y != row)
            {
                isRowPattern = false;
                break;
            }
        }
        
        // Check for column pattern
        bool isColumnPattern = true;
        int column = centerPos.x;
        foreach (Vector2Int pos in mergedPositions)
        {
            if (pos.x != column)
            {
                isColumnPattern = false;
                break;
            }
        }
        
        // Check for L shape or other patterns
        
        // Assign special tile type based on pattern
        if (isRowPattern)
        {
            return SpecialTileType.RowClear;
        }
        else if (isColumnPattern)
        {
            return SpecialTileType.ColumnClear;
        }
        else if (mergedPositions.Count >= 4)
        {
            return SpecialTileType.AreaClear;
        }
        else 
        {
            // Default to value boost for 3-tile merges with no pattern
            return SpecialTileType.ValueBoost;
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
    
    // Enhanced clear board with better cleanup and safeguards
    public void ClearBoard()
    {
        Debug.Log("Clearing the board of all tiles");
        
        // Stop any running coroutines to prevent errors with chain merges
        StopAllCoroutines();
        
        // Make a copy of the keys to avoid modification during iteration
        List<Vector2Int> positions = new List<Vector2Int>(tilePositions.Keys);
        
        // Remove all tiles immediately for a clean start
        foreach (Vector2Int pos in positions)
        {
            if (tilePositions.TryGetValue(pos, out GameObject tile))
            {
                // Remove from dictionaries
                tilePositions.Remove(pos);
                if (tile != null)
                {
                    tilesToPositions.Remove(tile);
                    ReturnTileToPool(tile);
                }
            }
        }
        
        // Additional check for any lingering GameTile objects that might not be properly tracked
        GameTile[] existingTiles = FindObjectsOfType<GameTile>();
        foreach (GameTile tile in existingTiles)
        {
            if (tile != null && tile.gameObject != null && tile.gameObject.activeInHierarchy)
            {
                // Double check if it's already tracked
                bool isTracked = false;
                if (tilesToPositions.ContainsKey(tile.gameObject))
                {
                    isTracked = true;
                }
                
                if (!isTracked)
                {
                    Debug.LogWarning($"Found untracked GameTile at {tile.transform.position} - returning to pool");
                    // Try to remove from any dictionary entries just in case
                    Vector2Int? position = GetGridPositionFromWorldSafe(tile.transform.position);
                    if (position.HasValue && tilePositions.ContainsKey(position.Value))
                    {
                        tilePositions.Remove(position.Value);
                    }
                    
                    ReturnTileToPool(tile.gameObject);
                }
            }
        }
        
        // Final safety check - clear dictionaries completely in case something was missed
        tilePositions.Clear();
        tilesToPositions.Clear();
        
        // Add a Physics2D sync to ensure everything is cleaned up
        Physics2D.SyncTransforms();
    }
    
    // Helper method to safely get grid position even for objects not precisely aligned
    private Vector2Int? GetGridPositionFromWorldSafe(Vector3 worldPosition)
    {
        if (boardManager == null) return null;
        
        // Use board's properties for consistent calculations
        float tileSpacing = boardManager.tileSpacing;
        float tolerance = tileSpacing * 0.25f;
        
        // Try to find the closest grid position
        for (int col = 0; col < boardManager.columns; col++)
        {
            for (int row = 0; row < boardManager.rows; row++)
            {
                float gridX = boardManager.boardStartX + (col * tileSpacing);
                float gridY = boardManager.boardStartY + (row * tileSpacing);
                
                if (Mathf.Abs(worldPosition.x - gridX) < tolerance && 
                    Mathf.Abs(worldPosition.y - gridY) < tolerance)
                {
                    return new Vector2Int(col, row);
                }
            }
        }
        
        // If no matching grid position is found, return null
        return null;
    }

    // New method to clear the board with animation
    private IEnumerator ClearBoardWithAnimation(List<Vector2Int> positions)
    {
        // Sort positions for a wave effect (optional)
        // positions.Sort((a, b) => (a.x + a.y).CompareTo(b.x + b.y));
        
        // Remove tiles with small delays between them
        foreach (Vector2Int pos in positions)
        {
            if (tilePositions.TryGetValue(pos, out GameObject tile))
            {
                // Animate the tile shrinking away
                if (TileAnimationManager.Instance != null)
                {
                    StartCoroutine(TileAnimationManager.Instance.ScaleTileOut(tile));
                    
                    // Make sure to remove the tile after animation completes
                    StartCoroutine(RemoveTileAfterDelay(pos, 0.3f)); // Match the animation duration
                }
                else
                {
                    // Direct removal if TileAnimationManager is unavailable
                    RemoveTileAt(pos);
                }
                
                // Small delay between tiles for wave effect
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    // Add a helper method to remove tiles after animation completes
    private IEnumerator RemoveTileAfterDelay(Vector2Int position, float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveTileAt(position);
    }

    public void GenerateRandomTile()
    {
        if (boardManager == null)
        {
            Debug.LogError("Cannot generate tile - Board reference is missing in TileManager!");
            return;
        }
        
        // Find an empty spot on the board
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        
        Debug.Log($"Finding empty positions for new tile. Board size: {boardManager.rows}x{boardManager.columns}");
        
        // Make sure the board dimensions are valid
        if (boardManager.rows <= 0 || boardManager.columns <= 0)
        {
            Debug.LogError($"Invalid board dimensions: {boardManager.rows}x{boardManager.columns}");
            return;
        }
        
        // First, run a full validation to fix any inconsistencies before generating a new tile
        ValidateAllTilePositions();
        
        // Clear physicsTiles dictionary before use to avoid stale data
        physicsTiles.Clear();
        
        for (int row = 0; row < boardManager.rows; row++)
        {
            for (int col = 0; col < boardManager.columns; col++)
            {
                Vector2Int position = new Vector2Int(col, row);
                if (!tilePositions.ContainsKey(position))
                {
                    // Check for obstacles as well (NEW)
                    if (ObstacleManager.Instance != null && ObstacleManager.Instance.IsObstacleAt(position))
                    {
                        continue; // Skip positions with obstacles
                    }
                    
                    // Extra verification: physically check for overlap
                    bool positionClear = true;
                    Vector3 worldPos = GetWorldPositionFromGrid(position);
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPos.x, worldPos.y), 0.2f);
                    
                    foreach (Collider2D collider in colliders)
                    {
                        if (collider.GetComponent<GameTile>() != null)
                        {
                            positionClear = false;
                            GameObject foundTile = collider.gameObject;
                            physicsTiles[position] = foundTile;
                            
                            // Don't log a warning here, just fix it silently
                            // Only if we're sure it's not tracked elsewhere (avoid duplicates)
                            if (!tilesToPositions.ContainsKey(foundTile))
                            {
                                SyncPhysicsTileToDictionary(foundTile, position);
                            }
                            else
                            {
                                // The tile is tracked, but at a different position - possible conflict
                                Vector2Int currentTrackedPos = tilesToPositions[foundTile];
                                if (currentTrackedPos != position)
                                {
                                    Debug.Log($"Fixing mismatch: Tile is physically at {position} but was tracked at {currentTrackedPos}");
                                    // Remove the tile from its old tracked position
                                    if (tilePositions.ContainsKey(currentTrackedPos) && tilePositions[currentTrackedPos] == foundTile)
                                    {
                                        tilePositions.Remove(currentTrackedPos);
                                    }
                                    // Update to the new position
                                    tilePositions[position] = foundTile;
                                    tilesToPositions[foundTile] = position;
                                }
                            }
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
        
        // If we found inconsistencies, let's try to resolve them
        if (physicsTiles.Count > 0)
        {
            Debug.Log($"Fixed {physicsTiles.Count} dictionary inconsistencies before generating new tile");
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
        
        // Double check with physics system one more time
        Vector3 spawnWorldPos = GetWorldPositionFromGrid(randomPosition);
        Collider2D[] finalCheck = Physics2D.OverlapCircleAll(new Vector2(spawnWorldPos.x, spawnWorldPos.y), 0.2f);
        foreach (Collider2D collider in finalCheck)
        {
            if (collider.GetComponent<GameTile>() != null)
            {
                Debug.LogError($"Physics system detected a tile at {randomPosition} right before spawning! Aborting spawn.");
                return;
            }
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
                
                // Only keep essential logs
                Debug.Log($"Created {randomColor} tile with value {randomValue} at position {randomPosition}");
            }
            
            // Use new animation manager for scaling in
            if (TileAnimationManager.Instance != null)
            {
                StartCoroutine(TileAnimationManager.Instance.ScaleTileIn(newTile));
            }
        }
        else
        {
            Debug.LogError($"Failed to create tile at position {randomPosition}");
        }
    }

    // Enhanced version of SyncPhysicsTileToDictionary to be more thorough
    private void SyncPhysicsTileToDictionary(GameObject tile, Vector2Int position)
    {
        if (tile == null) return;
        
        // First, check if this tile is already tracked somewhere else
        Vector2Int? existingPosition = null;
        if (tilesToPositions.TryGetValue(tile, out Vector2Int pos))
        {
            existingPosition = pos;
        }
        
        // Check if this position is already occupied in the dictionary
        GameObject existingTileAtPosition = null;
        if (tilePositions.TryGetValue(position, out GameObject tileAtPos))
        {
            existingTileAtPosition = tileAtPos;
        }
        
        // Case 1: Tile already tracked elsewhere, position has different tile
        if (existingPosition.HasValue && existingTileAtPosition != null && existingTileAtPosition != tile)
        {
            // This is a complex conflict situation
            Debug.Log($"Complex conflict: Tile {tile.name} tracked at {existingPosition.Value} but detected at {position}, which has {existingTileAtPosition.name}");
            
            // First, determine which scenario is physically correct
            Vector3 positionWorldSpace = GetWorldPositionFromGrid(position);
            Vector3 existingPosWorldSpace = GetWorldPositionFromGrid(existingPosition.Value);
            
            // Check tile's actual position
            Vector3 tileActualPos = tile.transform.position;
            float distToPosition = Vector3.Distance(tileActualPos, positionWorldSpace);
            float distToExistingPos = Vector3.Distance(tileActualPos, existingPosWorldSpace);
            
            // Trust physics - the tile is physically closer to 'position'
            if (distToPosition < distToExistingPos)
            {
                Debug.Log($"Trusting physics: Tile is closer to {position} than {existingPosition.Value}");
                
                // Remove tile from old position
                tilePositions.Remove(existingPosition.Value);
                
                // Dispose of the other tile that was wrongly at this position
                if (existingTileAtPosition != null)
                {
                    tilesToPositions.Remove(existingTileAtPosition);
                    ReturnTileToPool(existingTileAtPosition);
                }
                
                // Update tracking for this tile
                tilePositions[position] = tile;
                tilesToPositions[tile] = position;
            }
            else
            {
                // Trust existing tracking - leave tile where it is in the dictionary
                // Just fix the position of the existing tile if needed
                if (existingTileAtPosition != null)
                {
                    existingTileAtPosition.transform.position = positionWorldSpace;
                }
            }
        }
        // Case 2: Tile already tracked, but position is empty
        else if (existingPosition.HasValue && existingTileAtPosition == null)
        {
            // Double check if the tile is actually closer to this position
            Vector3 positionWorldSpace = GetWorldPositionFromGrid(position);
            Vector3 existingPosWorldSpace = GetWorldPositionFromGrid(existingPosition.Value);
            
            // Check which one is closer to the tile's actual position
            float distToPosition = Vector3.Distance(tile.transform.position, positionWorldSpace);
            float distToExistingPos = Vector3.Distance(tile.transform.position, existingPosWorldSpace);
            
            if (distToPosition < distToExistingPos)
            {
                // Update tracking to the new position
                tilePositions.Remove(existingPosition.Value);
                tilePositions[position] = tile;
                tilesToPositions[tile] = position;
                Debug.Log($"Moved tile tracking from {existingPosition.Value} to {position} based on physics detection");
            }
        }
        // Case 3: Tile not tracked, position already has a different tile
        else if (!existingPosition.HasValue && existingTileAtPosition != null && existingTileAtPosition != tile)
        {
            // Check which tile is actually at this position physically
            Vector3 positionWorldSpace = GetWorldPositionFromGrid(position);
            float distTileToPos = Vector3.Distance(tile.transform.position, positionWorldSpace);
            float distExistingTileToPos = Vector3.Distance(existingTileAtPosition.transform.position, positionWorldSpace);
            
            if (distTileToPos < distExistingTileToPos)
            {
                // The new tile is actually closer to this position, update tracking
                tilesToPositions.Remove(existingTileAtPosition);
                tilePositions[position] = tile;
                tilesToPositions[tile] = position;
                Debug.Log($"Replaced tracked tile at {position} based on physics detection");
            }
        }
        // Case 4: Tile not tracked, position is empty
        else if (!existingPosition.HasValue && existingTileAtPosition == null)
        {
            // Simple case - just add the tile to tracking
            tilePositions[position] = tile;
            tilesToPositions[tile] = position;
            Debug.Log($"Added untracked tile at {position} to dictionary based on physics");
        }
        // Case 5: Tile is tracked at this position already
        else if (existingPosition.HasValue && existingPosition.Value == position && existingTileAtPosition == tile)
        {
            // Everything is correct, do nothing
        }
    }

    // Add a more comprehensive validation method
    public void ValidateAllTilePositions()
    {
        Debug.Log("Performing full tile position validation...");
        
        // First, sync the physics state to ensure accurate collision detection
        Physics2D.SyncTransforms();
        
        // Temporary dictionaries to store corrected data
        Dictionary<Vector2Int, GameObject> validatedPositions = new Dictionary<Vector2Int, GameObject>();
        Dictionary<GameObject, Vector2Int> validatedTiles = new Dictionary<GameObject, Vector2Int>();
        
        // Track tiles that need to be returned to the pool
        List<GameObject> tilesToReturnToPool = new List<GameObject>();
        
        // Find all game tiles in the scene
        GameTile[] allTiles = FindObjectsOfType<GameTile>();
        int fixedCount = 0;
        
        foreach (GameTile tile in allTiles)
        {
            if (tile == null || tile.gameObject == null) continue;
            
            // Get current world position
            Vector3 worldPos = tile.transform.position;
            
            // Convert to grid position
            Vector2Int gridPos = GetGridPositionFromWorld(worldPos);
            
            // Check if this position is already claimed in our new validated dictionary
            if (validatedPositions.TryGetValue(gridPos, out GameObject existingTile))
            {
                // Conflict - two tiles at same position
                Debug.LogWarning($"Found two tiles at position {gridPos}: {tile.name} and {existingTile.name}");
                
                // Keep the one with the highest value or if values are equal, keep one randomly
                GameTile existingGameTile = existingTile.GetComponent<GameTile>();
                if (existingGameTile != null && tile.TileValue < existingGameTile.TileValue)
                {
                    // Keep existing tile, return this one to pool
                    tilesToReturnToPool.Add(tile.gameObject);
                }
                else
                {
                    // Keep new tile, return existing to pool
                    validatedPositions[gridPos] = tile.gameObject;
                    validatedTiles[tile.gameObject] = gridPos;
                    tilesToReturnToPool.Add(existingTile);
                    validatedTiles.Remove(existingTile);
                    fixedCount++;
                }
            }
            else
            {
                // Position is free, add this tile
                validatedPositions[gridPos] = tile.gameObject;
                validatedTiles[tile.gameObject] = gridPos;
                
                // Check if this resolves an inconsistency
                bool wasInconsistent = true;
                
                // Check if the tile was previously tracked
                if (tilesToPositions.TryGetValue(tile.gameObject, out Vector2Int oldPos))
                {
                    // Check if it was at a different position
                    if (oldPos != gridPos)
                    {
                        Debug.Log($"Fixed position for {tile.name}: {oldPos} -> {gridPos}");
                        fixedCount++;
                    }
                    else
                    {
                        wasInconsistent = false;
                    }
                }
                // Or if the position had a different tile
                else if (tilePositions.TryGetValue(gridPos, out GameObject oldTile) && oldTile != tile.gameObject)
                {
                    Debug.Log($"Fixed tile at position {gridPos}: {oldTile.name} -> {tile.name}");
                    fixedCount++;
                }
                // Or if neither the tile nor position was tracked
                else if (!tilesToPositions.ContainsKey(tile.gameObject) && !tilePositions.ContainsKey(gridPos))
                {
                    Debug.Log($"Added untracked tile {tile.name} at position {gridPos}");
                    fixedCount++;
                }
                else
                {
                    wasInconsistent = false;
                }
                
                // Ensure tile is properly positioned at grid point
                if (wasInconsistent)
                {
                    Vector3 correctWorldPos = GetWorldPositionFromGrid(gridPos);
                    if (Vector3.Distance(tile.transform.position, correctWorldPos) > 0.1f)
                    {
                        tile.transform.position = new Vector3(correctWorldPos.x, correctWorldPos.y, tile.transform.position.z);
                        Debug.Log($"Adjusted position of {tile.name} to align with grid at {gridPos}");
                    }
                }
            }
        }
        
        // Check for tracked tiles that don't exist anymore
        foreach (var kvp in tilesToPositions)
        {
            if (kvp.Key == null || !kvp.Key.activeInHierarchy || !validatedTiles.ContainsKey(kvp.Key))
            {
                Debug.Log($"Found tracked tile that doesn't exist or is inactive at position {kvp.Value}");
                fixedCount++;
            }
        }
        
        // Now return conflict tiles to pool
        foreach (GameObject tile in tilesToReturnToPool)
        {
            ReturnTileToPool(tile);
        }
        
        // Finally, replace the old dictionaries with validated ones
        tilePositions = validatedPositions;
        tilesToPositions = validatedTiles;
        
        // One final physics sync to ensure everything is up to date
        Physics2D.SyncTransforms();
        
        Debug.Log($"Tile position validation complete. Fixed {fixedCount} inconsistencies.");
    }

    // Instead, add a method to coordinate with ObstacleManager
    public void CheckForObstacleCollision(Vector2Int position)
    {
        // Check if there's an obstacle at this position
        if (ObstacleManager.Instance != null && ObstacleManager.Instance.IsObstacleAt(position))
        {
            // Damage the obstacle
            ObstacleManager.Instance.DamageObstacle(position);
        }
    }
    
    #endregion
}
