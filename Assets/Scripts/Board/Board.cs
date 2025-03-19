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

                // Determine if this is the first cell (0,0) where we want to place a GameTile
                GameObject tileObject;
                if (row == 0 && col == 0 && gameTilePrefab != null)
                {
                    // Instantiate a GameTile at the first cell
                    tileObject = Instantiate(gameTilePrefab, position, Quaternion.identity, transform);
                    
                    // Initialize with a color
                    GameTile gameTileScript = tileObject.GetComponent<GameTile>();
                    if (gameTileScript != null)
                    {
                        gameTileScript.Initialize(GameTile.TileColor.Red);
                    }
                }
                else
                {
                    // Instantiate a regular tile
                    tileObject = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                    
                    // Initialize the tile
                    Tile tileScript = tileObject.GetComponent<Tile>();
                    if (tileScript != null)
                    {
                        tileScript.Initialize($"Tile ({row}, {col})");

                        // Alternate tile colors for a chessboard pattern
                        Color tileColor = (row + col) % 2 == 0 ? color1 : color2;
                        tileScript.GetComponent<SpriteRenderer>().color = tileColor;
                    }
                }
                
                // Set tile size
                tileObject.transform.localScale = new Vector3(tileSize, tileSize, 1f);
            }
        }
    }
}
