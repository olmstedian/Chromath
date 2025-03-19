using UnityEngine;

public class GameTile : MonoBehaviour
{
    // Enum for tile colors
    public enum TileColor
    {
        Red,
        Green,
        Blue,
        Yellow
    }
    
    // Current tile color
    public TileColor CurrentColor { get; private set; }
    
    // Reference to the Tile component
    private Tile tileComponent;
    
    // References to sprite assets
    [SerializeField] private Sprite redSprite;
    [SerializeField] private Sprite greenSprite;
    [SerializeField] private Sprite blueSprite;
    [SerializeField] private Sprite yellowSprite;
    
    private void Awake()
    {
        // Get the Tile component
        tileComponent = GetComponent<Tile>();
        if (tileComponent == null)
        {
            Debug.LogError("GameTile requires a Tile component");
        }
        
        // Ensure this tile is rendered above regular tiles
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // Set a higher sorting order to render on top of other tiles
            renderer.sortingOrder = 10;
            
            // Alternatively, adjust the Z position slightly forward
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, pos.y, -0.1f);
        }
    }
    
    // Initialize the game tile with a specific color
    public void Initialize(TileColor color)
    {
        CurrentColor = color;
        UpdateVisuals();
        
        // Initialize the base tile component as well
        tileComponent.Initialize($"{color} Tile");
    }
    
    // Get the sprite for the current color
    private Sprite GetSpriteForColor(TileColor color)
    {
        switch (color)
        {
            case TileColor.Red:
                return redSprite;
            case TileColor.Green:
                return greenSprite;
            case TileColor.Blue:
                return blueSprite;
            case TileColor.Yellow:
                return yellowSprite;
            default:
                Debug.LogWarning($"No sprite assigned for color: {color}");
                return null;
        }
    }
    
    // Update the tile's visuals based on its color
    private void UpdateVisuals()
    {
        Sprite sprite = GetSpriteForColor(CurrentColor);
        if (sprite != null)
        {
            tileComponent.SetSprite(sprite);
        }
    }
    
    // Change the tile's color
    public void ChangeColor(TileColor newColor)
    {
        CurrentColor = newColor;
        UpdateVisuals();
    }
}
