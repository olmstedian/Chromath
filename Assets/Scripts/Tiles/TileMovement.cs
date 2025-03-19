using System.Collections;
using UnityEngine;

public class TileMovement : MonoBehaviour
{
    [SerializeField] private float moveTime = 0.2f;
    
    // Removed unused moveSpeed field
    
    private bool isMoving = false;
    private Vector3 targetPosition;
    private Tile tileComponent;
    private GameTile gameTileComponent;
    
    private void Awake()
    {
        tileComponent = GetComponent<Tile>();
        gameTileComponent = GetComponent<GameTile>();
    }
    
    // Try to move the tile in a specific direction
    public bool TryMove(Vector2 direction, float tileSpacing)
    {
        if (isMoving)
        {
            return false;
        }
        
        // Force a minimum spacing value to ensure movement is visible
        if (tileSpacing < 0.1f)
        {
            Debug.LogWarning("Tile spacing is too small. Using default value of 1.0");
            tileSpacing = 1.0f;
        }
        
        // Calculate the target position
        Vector3 desiredPosition = transform.position + new Vector3(direction.x, direction.y, 0) * tileSpacing;
        
        // Keep this log for tracking tile movement
        Debug.Log($"Moving tile from {transform.position} to {desiredPosition}");
        
        // Check for tile merging along the path
        if (gameTileComponent != null)
        {
            // Get the current grid position
            TileManager tileManager = TileManager.Instance;
            if (tileManager != null)
            {
                Vector2Int? currentPos = tileManager.GetTilePosition(gameObject);
                if (currentPos.HasValue)
                {
                    // Calculate the target grid position
                    Vector2Int targetPos = currentPos.Value + new Vector2Int(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.y));
                    
                    // Try to move or merge with the tile at the target position
                    bool moveSucceeded = tileManager.MoveTile(gameObject, targetPos);
                    
                    return moveSucceeded;
                }
            }
        }
        
        // Fallback if no tile manager or position is available
        StartCoroutine(MoveToPosition(desiredPosition));
        return true;
    }
    
    // Helper methods for specific directions
    public bool MoveUp(float tileSpacing) => TryMove(Vector2.up, tileSpacing);
    public bool MoveDown(float tileSpacing) => TryMove(Vector2.down, tileSpacing);
    public bool MoveLeft(float tileSpacing) => TryMove(Vector2.left, tileSpacing);
    public bool MoveRight(float tileSpacing) => TryMove(Vector2.right, tileSpacing);
    
    // Check if the move is valid
    private bool IsValidMove(Vector3 position)
    {
        // This can be expanded to check for collisions, board boundaries, etc.
        // For now, we'll just return true
        return true;
    }
    
    // Smoothly move the tile to the target position
    private IEnumerator MoveToPosition(Vector3 position)
    {
        // Keep start movement log
        Debug.Log($"Starting tile movement from {transform.position} to {position}");
        isMoving = true;
        targetPosition = position;
        
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        
        // Force the move to take at least 0.5 seconds so it's visible
        moveTime = Mathf.Max(moveTime, 0.5f);
        
        // Allow the coroutine to actually run
        yield return null;
        
        while (elapsedTime < moveTime)
        {
            float t = elapsedTime / moveTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            // Remove verbose intermediate movement logs
            yield return null;
        }
        
        // Ensure the tile ends up exactly at the target position
        transform.position = targetPosition;
        // Keep movement completion log
        Debug.Log($"Completed tile movement to {transform.position}");
        isMoving = false;
        
        // REMOVE this notification since it's now handled by TileManager
        // if (GameManager.Instance != null)
        // {
        //     GameManager.Instance.OnTileMovementComplete();
        // }
    }
    
    // Public property to check if the tile is currently moving
    public bool IsMoving => isMoving;
}
