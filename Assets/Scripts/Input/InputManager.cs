using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private float minSwipeDistance = 20f;
    [SerializeField] private Board board;
    
    // Direct test control - for debugging
    [SerializeField] private GameObject testTile;
    
    private Vector2 touchStartPos;
    private bool isTrackingSwipe = false;
    private GameObject selectedTile = null;
    
    private void Update()
    {
        // Only handle swipe tracking if we have a selected tile
        if (selectedTile != null && isTrackingSwipe)
        {
            HandleSwipeTracking();
        }
        
        // Direct testing controls
        TestDirectMovement();
    }
    
    // Called directly from Tile.OnMouseDown
    public void SetSelectedTile(GameObject tile)
    {
        selectedTile = tile;
        touchStartPos = Mouse.current.position.ReadValue();
        isTrackingSwipe = true;
        Debug.Log($"InputManager: Tracking swipe for {tile.name} from {touchStartPos}");
    }
    
    private void HandleSwipeTracking()
    {
        // Check if the mouse button is released
        if (!Mouse.current.leftButton.isPressed)
        {
            Vector2 touchEndPos = Mouse.current.position.ReadValue();
            Vector2 swipeDelta = touchEndPos - touchStartPos;
            
            Debug.Log($"Swipe detected: {swipeDelta}, Magnitude: {swipeDelta.magnitude}");
            
            // Process the swipe if it meets minimum distance
            if (swipeDelta.magnitude >= minSwipeDistance)
            {
                ProcessSwipe(swipeDelta);
            }
            else
            {
                Debug.Log("Swipe too short, ignoring");
            }
            
            // Reset tracking
            isTrackingSwipe = false;
            selectedTile = null;
        }
    }
    
    private void ProcessSwipe(Vector2 swipeDelta)
    {
        TileMovement tileMovement = selectedTile.GetComponent<TileMovement>();
        if (tileMovement == null)
        {
            Debug.LogError("No TileMovement component found on selected tile");
            return;
        }
        
        // Use the board's spacing for movement
        float moveDistance = board != null ? board.actualTileSpacing : 1.0f;
        
        // Determine swipe direction
        float x = Mathf.Abs(swipeDelta.x);
        float y = Mathf.Abs(swipeDelta.y);
        
        Debug.Log($"Processing swipe: {swipeDelta}, Magnitude: {swipeDelta.magnitude}");
        
        if (x > y)
        {
            // Horizontal swipe
            if (swipeDelta.x > 0)
            {
                Debug.Log("RIGHT swipe");
                tileMovement.MoveRight(moveDistance);
            }
            else
            {
                Debug.Log("LEFT swipe");
                tileMovement.MoveLeft(moveDistance);
            }
        }
        else
        {
            // Vertical swipe
            if (swipeDelta.y > 0)
            {
                Debug.Log("UP swipe");
                tileMovement.MoveUp(moveDistance);
            }
            else
            {
                Debug.Log("DOWN swipe");
                tileMovement.MoveDown(moveDistance);
            }
        }
    }
    
    private void TestDirectMovement()
    {
        // If no test tile is assigned, try to find one
        if (testTile == null && board != null)
        {
            // First try to find the red game tile if it exists
            GameObject[] allTiles = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject tile in allTiles)
            {
                GameTile gameTile = tile.GetComponent<GameTile>();
                if (gameTile != null && gameTile.CurrentColor == GameTile.TileColor.Red)
                {
                    testTile = tile;
                    Debug.Log($"Found red game tile: {testTile.name}");
                    break;
                }
            }
            
            // If not found, fallback to other methods
            if (testTile == null)
            {
                testTile = GameObject.FindWithTag("GameTile");
                if (testTile == null && board.transform.childCount > 0)
                {
                    testTile = board.transform.GetChild(0).gameObject;
                    Debug.Log($"Using first board child as test tile: {testTile.name}");
                }
            }
        }
        
        // Skip if we still don't have a test tile
        if (testTile == null) 
        {
            Debug.LogWarning("No test tile found. Cannot test movement.");
            return;
        }
        
        TileMovement movement = testTile.GetComponent<TileMovement>();
        if (movement == null)
        {
            Debug.LogError($"Test tile {testTile.name} has no TileMovement component!");
            return;
        }
        
        // Use a fixed spacing that's guaranteed to make visible movement
        float moveDistance = 1.0f;
        if (board != null)
        {
            moveDistance = board.actualTileSpacing > 0 ? board.actualTileSpacing : 1.0f;
        }
        
        // Test movement with keyboard
        if (Keyboard.current != null) // New Input System
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                Debug.Log($"TEST: Moving Up with distance {moveDistance}");
                movement.MoveUp(moveDistance);
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                Debug.Log($"TEST: Moving Down with distance {moveDistance}");
                movement.MoveDown(moveDistance);
            }
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                Debug.Log($"TEST: Moving Left with distance {moveDistance}");
                movement.MoveLeft(moveDistance);
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                Debug.Log($"TEST: Moving Right with distance {moveDistance}");
                movement.MoveRight(moveDistance);
            }
        }
    }
}
