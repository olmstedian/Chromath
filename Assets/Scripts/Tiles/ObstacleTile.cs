using UnityEngine;
using TMPro;

public class ObstacleTile : MonoBehaviour
{
    [SerializeField] private Sprite obstacleSprite;
    [SerializeField] private Color obstacleColor = new Color(0.4f, 0.4f, 0.4f); // Dark gray
    [SerializeField] private int durability = 1; // How many hits to destroy
    
    private SpriteRenderer spriteRenderer;
    private Tile tileComponent;
    private TextMeshPro durabilityText;
    
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
        
        // Create durability text if durability > 1
        if (durability > 1)
        {
            CreateDurabilityText();
        }
    }
    
    // Create text to display durability
    private void CreateDurabilityText()
    {
        GameObject textObj = new GameObject("DurabilityText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 0, -0.01f);
        
        durabilityText = textObj.AddComponent<TextMeshPro>();
        durabilityText.alignment = TextAlignmentOptions.Center;
        durabilityText.fontSize = 5;
        durabilityText.color = Color.white;
        durabilityText.text = durability.ToString();
        durabilityText.sortingOrder = 11;
    }
    
    // Set the durability (called by ObstacleManager)
    public void SetDurability(int value)
    {
        durability = Mathf.Max(1, value);
        UpdateDurabilityDisplay();
    }
    
    // Damage the obstacle, returns true if destroyed
    public bool TakeDamage(int damage)
    {
        durability -= damage;
        
        // Play damage animation
        StartCoroutine(DamageAnimation());
        
        // Update display
        UpdateDurabilityDisplay();
        
        // Return true if destroyed
        return durability <= 0;
    }
    
    // Animate damage effect
    private System.Collections.IEnumerator DamageAnimation()
    {
        if (spriteRenderer == null) yield break;
        
        // Store original color
        Color originalColor = spriteRenderer.color;
        Color flashColor = Color.red;
        
        // Flash red
        spriteRenderer.color = flashColor;
        
        // Wait briefly
        yield return new WaitForSeconds(0.1f);
        
        // Restore original color
        spriteRenderer.color = originalColor;
    }
    
    // Update text display
    private void UpdateDurabilityDisplay()
    {
        if (durabilityText != null)
        {
            if (durability > 1)
            {
                durabilityText.text = durability.ToString();
                durabilityText.gameObject.SetActive(true);
            }
            else
            {
                durabilityText.gameObject.SetActive(false);
            }
        }
        else if (durability > 1)
        {
            // Create text if needed
            CreateDurabilityText();
        }
    }
    
    // Prevent any interactions
    public void BlockInteraction()
    {
        // This method exists to prevent interaction
    }
}
