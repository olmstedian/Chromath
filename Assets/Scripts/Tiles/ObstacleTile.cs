using UnityEngine;

public class ObstacleTile : MonoBehaviour
{
    [SerializeField] private Sprite obstacleSprite;
    [SerializeField] private Color obstacleColor = new Color(0.4f, 0.4f, 0.4f); // Dark gray
    
    private SpriteRenderer spriteRenderer;
    private Tile tileComponent;
    
    private void Awake()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        tileComponent = GetComponent<Tile>();
        
        if (tileComponent == null)
        {
            Debug.LogError("ObstacleTile requires a Tile component");
        }
        
        // Set appearance
        if (spriteRenderer != null)
        {
            if (obstacleSprite != null)
            {
                spriteRenderer.sprite = obstacleSprite;
            }
            
            spriteRenderer.color = obstacleColor;
            spriteRenderer.sortingOrder = 10; // Make sure it's visible on top of the board
        }
        
        // Make sure it's not movable
        if (tileComponent != null)
        {
            tileComponent.SetMovable(false);
            tileComponent.Initialize("Obstacle Tile");
        }
    }
    
    // Prevent any interactions that might be triggered through other means
    public void BlockInteraction()
    {
        // Override any methods that might allow interaction
    }
}
