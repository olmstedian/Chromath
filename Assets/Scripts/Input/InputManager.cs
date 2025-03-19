using UnityEngine;
using UnityEngine.InputSystem; // Add this for new Input System

public class InputManager : MonoBehaviour
{
    [SerializeField] private float minSwipeDistance = 20f;
    [SerializeField] private Board board;
    
    // Direct test control - for debugging
    [SerializeField] private GameObject testTile;
    
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private bool isTouching = false;
    private GameObject selectedTile = null;
    
    private void Update()
    {
        // Handle input for both mobile and desktop
        HandleInput();
        
        // Direct testing controls
        TestDirectMovement();
    }
    
    private void HandleInput()
    {
        // Desktop mouse input
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            isTouching = true;
            
            // Cast a ray to find the selected tile
            selectedTile = GetTileAtPosition(touchStartPos);
            if (selectedTile != null)
            {
                Debug.Log($"Selected tile: {selectedTile.name}");
            }
        }
        else if (Input.GetMouseButtonUp(0) && isTouching)
        {
            touchEndPos = Input.mousePosition;
            isTouching = false;
            
            // Process the swipe only if we have a selected tile
            if (selectedTile != null)
            {
                Debug.Log($"Processing swipe for tile: {selectedTile.name}");
                ProcessSwipe();
            }
            
            selectedTile = null;
        }
        
        // Mobile touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                touchStartPos = touch.position;
                isTouching = true;
                
                // Cast a ray to find the selected tile
                selectedTile = GetTileAtPosition(touchStartPos);
            }
            else if (touch.phase == UnityEngine.TouchPhase.Ended && isTouching)
            {
                touchEndPos = touch.position;
                isTouching = false;
                
                // Process the swipe only if we have a selected tile
                if (selectedTile != null)
                {
                    ProcessSwipe();
                }
                
                selectedTile = null;
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
    
    private GameObject GetTileAtPosition(Vector2 screenPosition)
    {
        // Convert screen position to world position for 2D
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        
        if (hit.collider != null)
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name}");
            // Check if the hit object has a Tile component
            if (hit.collider.gameObject.GetComponent<Tile>() != null)
            {
                return hit.collider.gameObject;
            }
        }
        else
        {
            Debug.Log("No collider hit");
        }
        
        return null;
    }
    
    private void ProcessSwipe()
    {
        Vector2 swipeDelta = touchEndPos - touchStartPos;
        
        // Check if the swipe distance is greater than the minimum
        if (swipeDelta.magnitude < minSwipeDistance)
        {
            Debug.Log("Swipe too short");
            return;
        }
        
        // Get the tile movement component
        TileMovement tileMovement = selectedTile.GetComponent<TileMovement>();
        if (tileMovement == null)
        {
            Debug.LogError("No TileMovement component found on selected tile");
            return;
        }
        
        // Determine swipe direction
        float x = Mathf.Abs(swipeDelta.x);
        float y = Mathf.Abs(swipeDelta.y);
        
        Debug.Log($"Swipe delta: {swipeDelta}, Magnitude: {swipeDelta.magnitude}");
        
        if (x > y)
        {
            // Horizontal swipe
            if (swipeDelta.x > 0)
            {
                // Right swipe
                Debug.Log("Right swipe");
                tileMovement.MoveRight(board.tileSpacing);
            }
            else
            {
                // Left swipe
                Debug.Log("Left swipe");
                tileMovement.MoveLeft(board.tileSpacing);
            }
        }
        else
        {
            // Vertical swipe
            if (swipeDelta.y > 0)
            {
                // Up swipe
                Debug.Log("Up swipe");
                tileMovement.MoveUp(board.tileSpacing);
            }
            else
            {
                // Down swipe
                Debug.Log("Down swipe");
                tileMovement.MoveDown(board.tileSpacing);
            }
        }
    }
}
