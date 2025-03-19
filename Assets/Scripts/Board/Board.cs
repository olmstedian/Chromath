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

    private void Start()
    {
        // Set the actual spacing that will be used for movement calculations
        actualTileSpacing = tileSpacing;
        
        // Clamp the values to ensure valid dimensions
        rows = Mathf.Clamp(rows, 2, 10);
        columns = Mathf.Clamp(columns, 2, 10);
        
        GenerateBoard();
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

        // No need to change localPosition - we'll position tiles relative to center instead
        // transform.localPosition = new Vector3(-boardWidth / 2, -boardHeight / 2, 0);

        // Calculate the starting position offset to center the entire grid
        float startX = -boardWidth / 2;
        float startY = -boardHeight / 2;

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

                // Remove the automatic GameTile creation at (0,0) since we'll now
                // let the TileManager handle this properly with GenerateInitialTiles
                // This prevents duplicate tiles at the same location
                
                /* Remove this section:
                if (row == 0 && col == 0 && gameTilePrefab != null)
                {
                    // Position slightly in front of the regular tile
                    Vector3 gameTilePos = new Vector3(position.x, position.y, position.z - 0.1f);
                    
                    // Instantiate a GameTile at the first cell
                    GameObject gameTileObject = Instantiate(gameTilePrefab, gameTilePos, Quaternion.identity, transform);
                    
                    // Initialize with a color
                    GameTile gameTileScript = gameTileObject.GetComponent<GameTile>();
                    if (gameTileScript != null)
                    {
                        gameTileScript.Initialize(GameTile.TileColor.Red);
                        
                        // Make sure the Tile component of the GameTile is set to movable
                        Tile gameTileComponent = gameTileObject.GetComponent<Tile>();
                        if (gameTileComponent != null)
                        {
                            gameTileComponent.SetMovable(true);
                        }
                    }
                    
                    // Ensure GameTile is rendered on top of regular tiles
                    SpriteRenderer renderer = gameTileObject.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.sortingOrder = 10;
                    }
                    
                    // Set game tile size
                    gameTileObject.transform.localScale = new Vector3(tileSize, tileSize, 1f);
                }
                */
            }
        }
    }

    // Add this method to help TileManager get world positions
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        float boardWidth = (columns - 1) * tileSpacing;
        float boardHeight = (rows - 1) * tileSpacing;
        
        float startX = -boardWidth / 2;
        float startY = -boardHeight / 2;
        
        return new Vector3(
            startX + (gridPosition.x * tileSpacing), 
            startY + (gridPosition.y * tileSpacing), 
            0
        );
    }
}
