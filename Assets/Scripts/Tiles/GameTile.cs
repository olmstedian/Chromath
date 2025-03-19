using UnityEngine;
using TMPro;

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
    
    // Current tile color and value
    public TileColor CurrentColor { get; private set; }
    public int TileValue { get; private set; } = 2; // Default starting value
    
    // Reference to the Tile component
    private Tile tileComponent;
    
    // Text component for displaying the number
    [SerializeField] private TextMeshPro numberText;
    
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
        
        // Check for TextMeshPro component
        if (numberText == null)
        {
            // Try to find the TextMeshPro component in children
            numberText = GetComponentInChildren<TextMeshPro>();
            
            // If still not found, create it
            if (numberText == null)
            {
                CreateNumberText();
            }
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
    
    // Create TextMeshPro component if it doesn't exist
    private void CreateNumberText()
    {
        // Create a new GameObject for the 3D text
        GameObject textObj = new GameObject("NumberText3D");
        textObj.transform.SetParent(transform);
        // Position centered on tile, slightly forward
        textObj.transform.localPosition = new Vector3(0, 0, -0.01f);
        textObj.transform.localRotation = Quaternion.identity;
        
        // Add TextMeshPro component (3D version)
        numberText = textObj.AddComponent<TextMeshPro>();
        
        // Configure the text settings
        numberText.alignment = TextAlignmentOptions.Center;
        numberText.fontSize = 5;
        numberText.color = Color.white;
        numberText.fontWeight = FontWeight.Bold;
        
        // Make text face forward (camera)
        numberText.transform.localRotation = Quaternion.identity;
        
        // Set the sorting order higher than the tile sprite
        numberText.sortingOrder = 11;
    }
    
    // Initialize the game tile with a specific color and value
    public void Initialize(TileColor color, int value = 2)
    {
        CurrentColor = color;
        TileValue = value;
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
    
    // Update the tile's visuals based on its color and value
    private void UpdateVisuals()
    {
        Sprite sprite = GetSpriteForColor(CurrentColor);
        if (sprite != null)
        {
            tileComponent.SetSprite(sprite);
            
            // Also update the material color to match
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.material != null)
            {
                Color materialColor = Color.white; // Default
                
                switch (CurrentColor)
                {
                    case TileColor.Red:
                        materialColor = new Color(1f, 0.2f, 0.2f);
                        break;
                    case TileColor.Green:
                        materialColor = new Color(0.2f, 1f, 0.2f);
                        break;
                    case TileColor.Blue:
                        materialColor = new Color(0.2f, 0.5f, 1f);
                        break;
                    case TileColor.Yellow:
                        materialColor = new Color(1f, 0.92f, 0.2f);
                        break;
                }
                
                renderer.material.SetColor("_Color", materialColor);
            }
        }
        
        // Update the number text
        if (numberText != null)
        {
            numberText.text = TileValue.ToString();
            
            // Scale text size based on number of digits
            if (TileValue >= 1000)
                numberText.fontSize = 4f;
            else if (TileValue >= 100)
                numberText.fontSize = 4.5f;
            else
                numberText.fontSize = 5f;
            
            // Ensure text is facing forward
            numberText.transform.localRotation = Quaternion.identity;
        }
    }
    
    // Change the tile's color
    public void ChangeColor(TileColor newColor)
    {
        CurrentColor = newColor;
        UpdateVisuals();
    }
    
    // Double the tile's value (useful when merging tiles)
    public void DoubleValue()
    {
        TileValue *= 2;
        UpdateVisuals();
    }
    
    // Set a specific value
    public void SetValue(int value)
    {
        TileValue = value;
        UpdateVisuals();
    }
}
