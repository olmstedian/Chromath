using System.Collections;
using UnityEngine;

public class TileMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveTime = 0.2f;
    
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
            Debug.Log("Cannot move - already moving");
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
        
        Debug.Log($"Attempting to move from {transform.position} to {desiredPosition}");
        
        // Always allow the move for now (debugging)
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
        Debug.Log($"Starting movement from {transform.position} to {position}");
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
            Debug.Log($"Moving: t={t}, pos={transform.position}");
            yield return null;
        }
        
        // Ensure the tile ends up exactly at the target position
        transform.position = targetPosition;
        Debug.Log($"Movement complete: pos={transform.position}");
        isMoving = false;
    }
    
    // Public property to check if the tile is currently moving
    public bool IsMoving => isMoving;
}
