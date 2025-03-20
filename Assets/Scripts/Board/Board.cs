using UnityEngine;

public class Board : MonoBehaviour
{
    public GameObject tilePrefab; // Reference to the Tile prefab
    public GameObject gameTilePrefab; // Reference to the GameTile prefab
    public int rows = 5;          // Number of rows in the grid
    public int columns = 5;       // Number of columns in the grid
    public float tileSpacing = 0.8f; // Spacing between tiles
    public float tileSize = 2.0f; // Size multiplier for tiles

    // Explicitly make tileSpacing accessible to other scripts
    [HideInInspector] // This prevents cluttering the inspector while still keeping it public
    public float actualTileSpacing = 0.8f; // This will be used for movement

    // Colors for the chessboard pattern
    public Color color1 = Color.white;
    public Color color2 = Color.black;

    // Add public variables to expose board dimensions
    [HideInInspector] public float boardStartX;
    [HideInInspector] public float boardStartY;

    private void Start()
    {
        // Set the actual spacing that will be used for movement calculations
        actualTileSpacing = tileSpacing;
        
        // Clamp the values to ensure valid dimensions
        rows = Mathf.Clamp(rows, 2, 10);
        columns = Mathf.Clamp(columns, 2, 10);
        
        GenerateBoard();
        
        // Log board generation details for debugging
        Debug.Log($"Board generated with {rows}x{columns} dimensions, tile spacing: {tileSpacing}, tile size: {tileSize}");
        
        // Ensure TileManager is notified that the board is ready
        if (TileManager.Instance != null)
        {
            TileManager.Instance.OnBoardReady(this);
        }
    }

    // A method to rebuild the board with new dimensions
    public void RebuildBoard(int newRows, int newColumns)
    {
        // Clean up existing children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        
        // Set new dimensions
        rows = Mathf.Clamp(newRows, 2, 10);
        columns = Mathf.Clamp(newColumns, 2, 10);
        
        // Rebuild the board
        GenerateBoard();
    }

    // Method to generate the board
    private void GenerateBoard()
    {
        // Calculate the total dimensions
        float boardWidth = (columns - 1) * tileSpacing;
        float boardHeight = (rows - 1) * tileSpacing;

        // Calculate the starting position offset to center the entire grid
        float startX = -boardWidth / 2;
        float startY = -boardHeight / 2;
        
        // Store these values for use by other scripts
        boardStartX = startX;
        boardStartY = startY;

        // Log board dimensions for debugging
        Debug.Log($"Board dimensions: width={boardWidth}, height={boardHeight}, startX={startX}, startY={startY}");

        // Clear any existing child tiles before regenerating
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            else
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Calculate tile position from the center offset
                Vector3 position = new Vector3(startX + (col * tileSpacing), startY + (row * tileSpacing), 0);

                // Always create a regular tile for each position
                GameObject regularTile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                
                // Initialize the regular tile
                Tile tileScript = regularTile.GetComponent<Tile>();
                if (tileScript != null)
                {
                    tileScript.Initialize($"Tile ({row}, {col})");
                    tileScript.SetMovable(false); // Set board tiles as non-movable
                    
                    // Alternate tile colors for a chessboard pattern
                    Color tileColor = (row + col) % 2 == 0 ? color1 : color2;
                    tileScript.GetComponent<SpriteRenderer>().color = tileColor;
                    
                    // Remove or disable any BoxCollider2D to prevent interaction
                    BoxCollider2D collider = tileScript.GetComponent<BoxCollider2D>();
                    if (collider != null)
                    {
                        collider.enabled = false;
                    }
                    
                    // Remove TileMovement component if it exists
                    TileMovement movement = tileScript.GetComponent<TileMovement>();
                    if (movement != null)
                    {
                        Destroy(movement);
                    }
                }
                
                // Set regular tile size
                regularTile.transform.localScale = new Vector3(tileSize, tileSize, 1f);

                // Log individual tile positions for debugging - but only for the outer tiles to reduce log spam
                if (row == 0 || row == rows-1 || col == 0 || col == columns-1)
                {
                    Debug.Log($"Created board tile at position {position} for grid {col},{row}");
                }
            }
        }
    }

    // Add this method to help TileManager get world positions
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        // Ensure grid positions are within bounds
        int x = Mathf.Clamp(gridPosition.x, 0, columns - 1);
        int y = Mathf.Clamp(gridPosition.y, 0, rows - 1);
        
        Vector3 worldPos = new Vector3(
            boardStartX + (x * tileSpacing), 
            boardStartY + (y * tileSpacing), 
            0
        );
        
        // Log conversion for debugging
        Debug.Log($"Board.GetWorldPosition: Grid {gridPosition} -> World {worldPos}");
        
        return worldPos;
    }
}
