using UnityEngine;

public class Tile : MonoBehaviour
{
    // Property to store the tile's type or state
    public string TileType { get; set; }

    // Reference to the SpriteRenderer component
    private SpriteRenderer spriteRenderer;

    // Reference to the BoxCollider2D component
    private BoxCollider2D boxCollider;

    private TileMovement movement;

    private void Awake()
    {
        // Initialize the SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component is missing on the Tile GameObject.");
        }

        // Initialize the BoxCollider2D
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // Ensure collider size matches the sprite size
        if (spriteRenderer != null && boxCollider != null)
        {
            boxCollider.size = spriteRenderer.bounds.size;
        }

        // Configure the BoxCollider2D
        boxCollider.isTrigger = false; // Changed to non-trigger for raycasting

        // Get the movement component or add it if it doesn't exist
        movement = GetComponent<TileMovement>();
        if (movement == null)
        {
            movement = gameObject.AddComponent<TileMovement>();
        }
    }

    // Method to initialize the tile
    public void Initialize(string tileType)
    {
        TileType = tileType;
        // Additional initialization logic can go here
    }

    // Method to set the sprite for the tile
    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    // Method to handle tile interactions
    public void OnTileClicked()
    {
        Debug.Log($"Tile clicked: {TileType}");
        // The input handling is now done by the InputManager
    }

    // Method to handle mouse input for macOS
    private void OnMouseDown()
    {
        Debug.Log($"Tile clicked: {TileType}");
        OnTileClicked();
    }
}
