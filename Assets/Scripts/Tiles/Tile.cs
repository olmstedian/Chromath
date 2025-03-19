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

    // Reference to InputManager
    private static InputManager inputManager;

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

    private void Start()
    {
        // Try multiple ways to find the InputManager
        FindInputManager();
    }

    private void FindInputManager()
    {
        // If already found, don't search again
        if (inputManager != null) return;

        // Try to find using FindObjectOfType
        inputManager = FindObjectOfType<InputManager>();

        if (inputManager == null)
        {
            // Try to find by name
            GameObject imObj = GameObject.Find("InputManager");
            if (imObj != null)
            {
                inputManager = imObj.GetComponent<InputManager>();
            }

            // If still not found, look for any active GameObject with the InputManager component
            if (inputManager == null)
            {
                InputManager[] managers = FindObjectsOfType<InputManager>(true); // Include inactive
                if (managers.Length > 0)
                {
                    inputManager = managers[0];
                    Debug.Log($"Found InputManager on {inputManager.gameObject.name}");
                }
                else
                {
                    Debug.LogError("InputManager not found in scene. Please add an InputManager GameObject!");
                }
            }
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

        // Try to find InputManager again if it's null
        if (inputManager == null)
        {
            FindInputManager();
        }

        // Notify InputManager that this tile has been selected
        if (inputManager != null)
        {
            inputManager.SetSelectedTile(gameObject);
            Debug.Log($"Notified InputManager about click on {TileType}");
        }
        else
        {
            Debug.LogWarning("InputManager not found. Cannot process swipe input.");
        }

        OnTileClicked();
    }
}
