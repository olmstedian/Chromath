using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int boardWidth = 6;
    [SerializeField] private int boardHeight = 6;
    [SerializeField] private float cellSize = 1.0f;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform boardParent;
    
    [Header("Game Settings")]
    [SerializeField] private int startingTileCount = 4;
    [SerializeField] private int targetValue = 2048;
    
    // The game board data
    private Tile[,] board;
    
    // Currently selected tile
    private Tile selectedTile = null;
    
    // Tracking game state
    private int score = 0;
    private bool gameOver = false;
    
    // Events
    public System.Action<int> OnScoreChanged;
    public System.Action<Tile> OnTileSelected;
    public System.Action<Tile> OnTileMoved;
    public System.Action<Tile, Tile> OnTilesMerged;
    public System.Action OnGameOver;
    public System.Action OnGameWon;
    
    private void Start()
    {
        InitializeBoard();
        SpawnInitialTiles();
    }
    
    private void InitializeBoard()
    {
        board = new Tile[boardWidth, boardHeight];
        
        // Adjust camera to center the board
        AdjustCamera();
    }
    
    private void AdjustCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            // Center the camera on the board
            cam.transform.position = new Vector3(
                (boardWidth - 1) * cellSize * 0.5f,
                (boardHeight - 1) * cellSize * 0.5f,
                -10f
            );
            
            // Calculate orthographic size based on board dimensions
            float aspectRatio = (float)Screen.width / Screen.height;
            float verticalSize = boardHeight * cellSize * 0.6f;
            float horizontalSize = boardWidth * cellSize * 0.6f / aspectRatio;
            cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
        }
    }
    
    private void SpawnInitialTiles()
    {
        for (int i = 0; i < startingTileCount; i++)
        {
            SpawnRandomTile();
        }
    }
    
    private void SpawnRandomTile()
    {
        List<Vector2Int> emptyCells = GetEmptyCells();
        
        if (emptyCells.Count > 0)
        {
            // Choose random empty cell
            int randomIndex = Random.Range(0, emptyCells.Count);
            Vector2Int cell = emptyCells[randomIndex];
            
            // Choose random value (80% for 2, 20% for 4)
            int value = Random.value < 0.8f ? 2 : 4;
            
            // Choose random color
            TileColor color = (TileColor)Random.Range(0, 5);
            
            // Create the tile data
            TileData tileData = new TileData(value, color);
            
            // Create the tile game object
            SpawnTile(tileData, cell);
        }
    }
    
    private Tile SpawnTile(TileData tileData, Vector2Int gridPosition)
    {
        Vector3 worldPosition = GridToWorldPosition(gridPosition);
        
        // Instantiate the tile prefab
        GameObject tileObject = Instantiate(tilePrefab, worldPosition, Quaternion.identity, boardParent);
        Tile tile = tileObject.GetComponent<Tile>();
        
        // Initialize the tile
        tile.Initialize(tileData, worldPosition);
        
        // Add to board array
        board[gridPosition.x, gridPosition.y] = tile;
        
        return tile;
    }
    
    private List<Vector2Int> GetEmptyCells()
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (board[x, y] == null)
                {
                    emptyCells.Add(new Vector2Int(x, y));
                }
            }
        }
        
        return emptyCells;
    }
    
    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * cellSize, gridPosition.y * cellSize, 0f);
    }
    
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / cellSize);
        int y = Mathf.RoundToInt(worldPosition.y / cellSize);
        
        return new Vector2Int(x, y);
    }
    
    public bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < boardWidth &&
               gridPosition.y >= 0 && gridPosition.y < boardHeight;
    }
    
    public Tile GetTileAt(Vector2Int gridPosition)
    {
        if (IsValidGridPosition(gridPosition))
        {
            return board[gridPosition.x, gridPosition.y];
        }
        return null;
    }
    
    public void SelectTile(Vector2Int gridPosition)
    {
        if (gameOver) return;
        
        Tile tile = GetTileAt(gridPosition);
        if (tile != null)
        {
            selectedTile = tile;
            OnTileSelected?.Invoke(selectedTile);
        }
    }
    
    public void MoveTile(Direction direction)
    {
        if (selectedTile == null || gameOver) return;
        
        // Get the current position of the selected tile
        Vector2Int currentPosition = GetTilePosition(selectedTile);
        Vector2Int targetPosition = currentPosition + GetDirectionOffset(direction);
        
        // Check if the target position is valid
        if (!IsValidGridPosition(targetPosition))
        {
            // Can't move outside the board
            selectedTile = null;
            return;
        }
        
        Tile targetTile = GetTileAt(targetPosition);
        
        // Check if target cell is empty
        if (targetTile == null)
        {
            // Move to empty cell
            board[targetPosition.x, targetPosition.y] = selectedTile;
            board[currentPosition.x, currentPosition.y] = null;
            
            // Update tile position
            selectedTile.MoveTo(GridToWorldPosition(targetPosition));
            
            OnTileMoved?.Invoke(selectedTile);
        }
        // Check if merge is possible
        else if (CanMerge(selectedTile.Data, targetTile.Data))
        {
            // Handle merge
            StartCoroutine(selectedTile.MergeWithTile(targetTile));
            board[currentPosition.x, currentPosition.y] = null;
            
            // Update score
            int mergeValue = selectedTile.Data.value + targetTile.Data.value;
            score += mergeValue;
            OnScoreChanged?.Invoke(score);
            OnTilesMerged?.Invoke(selectedTile, targetTile);
            
            // Check for win condition
            if (targetTile.Data.value >= targetValue)
            {
                gameOver = true;
                OnGameWon?.Invoke();
                return;
            }
            
            // Generate new tile after successful merge
            SpawnRandomTile();
        }
        
        // Reset selection
        selectedTile = null;
        
        // Check for game over condition
        if (IsGameOver())
        {
            gameOver = true;
            OnGameOver?.Invoke();
        }
    }
    
    public void MoveTile(Tile tile, Direction direction)
    {
        selectedTile = tile;
        MoveTile(direction);
    }
    
    private Vector2Int GetTilePosition(Tile tile)
    {
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (board[x, y] == tile)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }
    
    private Vector2Int GetDirectionOffset(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return new Vector2Int(0, 1);
            case Direction.Right:
                return new Vector2Int(1, 0);
            case Direction.Down:
                return new Vector2Int(0, -1);
            case Direction.Left:
                return new Vector2Int(-1, 0);
            default:
                return Vector2Int.zero;
        }
    }
    
    private bool CanMerge(TileData tile1, TileData tile2)
    {
        // Basic merge: same color
        if (tile1.color == tile2.color)
            return true;
        
        // Special tile rules
        if (tile1.isSpecial && (tile1.specialType == SpecialTileType.Wildcard || tile1.specialType == SpecialTileType.Rainbow))
            return true;
            
        if (tile2.isSpecial && (tile2.specialType == SpecialTileType.Wildcard || tile2.specialType == SpecialTileType.Rainbow))
            return true;
        
        return false;
    }
    
    private bool IsGameOver()
    {
        // Check if the board is full
        if (GetEmptyCells().Count > 0)
            return false;
            
        // Check if any valid moves remain
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                Tile tile = board[x, y];
                if (tile != null)
                {
                    // Check in all four directions
                    foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
                    {
                        Vector2Int adjacentPos = new Vector2Int(x, y) + GetDirectionOffset(dir);
                        if (IsValidGridPosition(adjacentPos))
                        {
                            Tile adjacentTile = GetTileAt(adjacentPos);
                            if (adjacentTile != null && CanMerge(tile.Data, adjacentTile.Data))
                            {
                                return false; // Found a valid merge
                            }
                        }
                    }
                }
            }
        }
        
        // No valid moves found
        return true;
    }
    
    public void RestartGame()
    {
        // Clear existing tiles
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (board[x, y] != null)
                {
                    Destroy(board[x, y].gameObject);
                    board[x, y] = null;
                }
            }
        }
        
        // Reset game state
        score = 0;
        gameOver = false;
        selectedTile = null;
        OnScoreChanged?.Invoke(score);
        
        // Restart the game
        SpawnInitialTiles();
    }
    
    // Special tile handling methods
    
    public void HandleJumperTile(Tile tile, Direction direction)
    {
        Vector2Int currentPosition = GetTilePosition(tile);
        Vector2Int intermediatePosition = currentPosition + GetDirectionOffset(direction);
        Vector2Int targetPosition = intermediatePosition + GetDirectionOffset(direction);
        
        // Check if target position is valid
        if (!IsValidGridPosition(targetPosition))
            return;
        
        Tile targetTile = GetTileAt(targetPosition);
        
        // Check if target cell is empty
        if (targetTile == null)
        {
            // Move jumper tile two spaces
            board[targetPosition.x, targetPosition.y] = tile;
            board[currentPosition.x, currentPosition.y] = null;
            
            // Update tile position
            tile.MoveTo(GridToWorldPosition(targetPosition));
            
            OnTileMoved?.Invoke(tile);
        }
        // Check if merge is possible
        else if (CanMerge(tile.Data, targetTile.Data))
        {
            // Handle merge
            StartCoroutine(tile.MergeWithTile(targetTile));
            board[currentPosition.x, currentPosition.y] = null;
            
            // Update score
            int mergeValue = tile.Data.value + targetTile.Data.value;
            score += mergeValue;
            OnScoreChanged?.Invoke(score);
            OnTilesMerged?.Invoke(tile, targetTile);
            
            // Generate new tile after successful merge
            SpawnRandomTile();
        }
    }
    
    public void HandleConverterTile(Tile tile, Direction direction)
    {
        Vector2Int currentPosition = GetTilePosition(tile);
        Vector2Int targetPosition = currentPosition + GetDirectionOffset(direction);
        
        if (IsValidGridPosition(targetPosition))
        {
            Tile targetTile = GetTileAt(targetPosition);
            if (targetTile != null)
            {
                // Convert the adjacent tile's color to match this tile
                targetTile.Data.color = tile.Data.color;
                targetTile.UpdateVisuals();
            }
        }
    }
    
    public void HandleBombTile(Tile tile)
    {
        Vector2Int position = GetTilePosition(tile);
        
        // Clear tiles in a 3x3 area
        for (int x = position.x - 1; x <= position.x + 1; x++)
        {
            for (int y = position.y - 1; y <= position.y + 1; y++)
            {
                Vector2Int targetPos = new Vector2Int(x, y);
                
                // Skip the bomb tile itself
                if (targetPos == position)
                    continue;
                
                if (IsValidGridPosition(targetPos) && GetTileAt(targetPos) != null)
                {
                    Tile targetTile = GetTileAt(targetPos);
                    
                    // Add score for each cleared tile
                    score += targetTile.Data.value;
                    
                    // Destroy the tile
                    Destroy(targetTile.gameObject);
                    board[x, y] = null;
                }
            }
        }
        
        // Update score
        OnScoreChanged?.Invoke(score);
        
        // Fill empty spots with new tiles
        StartCoroutine(FillBoardAfterBombDelay());
    }
    
    private IEnumerator FillBoardAfterBombDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        // Fill empty spots with new random tiles
        List<Vector2Int> emptyCells = GetEmptyCells();
        foreach (Vector2Int cell in emptyCells)
        {
            // Choose random value and color for new tile
            int value = Random.value < 0.8f ? 2 : 4;
            TileColor color = (TileColor)Random.Range(0, 5);
            TileData tileData = new TileData(value, color);
            
            // Spawn the tile
            SpawnTile(tileData, cell);
            
            // Small delay between spawns for visual effect
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    public void HandleMultiplierTile(Tile multiplierTile)
    {
        // Increase the value of all tiles on the board
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                Tile tile = board[x, y];
                if (tile != null)
                {
                    tile.Data.value *= 2;
                    tile.UpdateVisuals();
                }
            }
        }
        
        // Destroy the multiplier tile
        Destroy(multiplierTile.gameObject);
    }
}