using UnityEngine;

public class MacOSInputHandler : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private float minSwipeDistance = 20f;
    [SerializeField] private float maxClickDuration = 0.5f; // Max time for a click to be registered
    
    private Vector2 touchStartPosition;  
    private float touchStartTime;
    private bool isDragging = false;
    private Tile selectedTile = null;
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        // Subscribe to tile selected event
        if (boardManager != null)
        {
            boardManager.OnTileSelected += OnTileSelected;
        }
    }
    
    private void Update()
    {
        HandleMouseInput();
        
        // Special handling for trackpad gestures can be added here if needed
    }
    
    private void HandleMouseInput()
    {
        // Mouse button down / Touch begin
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPosition = Input.mousePosition;
            touchStartTime = Time.time;
            isDragging = false;
            
            // Try to select a tile at the click position
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(touchStartPosition);
            worldPosition.z = 0; // Ensure we're on the game plane
            
            Vector2Int gridPosition = boardManager.WorldToGridPosition(worldPosition);
            
            // Select the tile
            boardManager.SelectTile(gridPosition);
        }
        
        // Mouse drag
        if (Input.GetMouseButton(0) && !isDragging && selectedTile != null)
        {
            // Check if we've moved enough to count as a drag
            float dragDistance = Vector2.Distance(touchStartPosition, Input.mousePosition);
            if (dragDistance > minSwipeDistance)
            {
                isDragging = true;
            }
        }
        
        // Mouse button up / Touch end
        if (Input.GetMouseButtonUp(0) && selectedTile != null)
        {
            Vector2 touchEndPosition = Input.mousePosition;
            float touchDuration = Time.time - touchStartTime;
            
            // If it was a quick tap or click, process as selection only
            if (touchDuration <= maxClickDuration && !isDragging)
            {
                // Selection was already handled on mouse down
                // You could add double-click logic here for special tiles
            }
            // If it was a drag/swipe, process the movement
            else if (isDragging)
            {
                Vector2 swipeDelta = touchEndPosition - touchStartPosition;
                
                // Determine swipe direction
                Direction swipeDirection = GetSwipeDirection(swipeDelta);
                
                // Move the selected tile in that direction
                boardManager.MoveTile(swipeDirection);
            }
            
            // Reset state
            isDragging = false;
        }
    }
    
    private Direction GetSwipeDirection(Vector2 swipeDelta)
    {
        // Determine primary direction (up, down, left, right)
        float horizontal = Mathf.Abs(swipeDelta.x);
        float vertical = Mathf.Abs(swipeDelta.y);
        
        if (horizontal > vertical)
        {
            return swipeDelta.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return swipeDelta.y > 0 ? Direction.Up : Direction.Down;
        }
    }
    
    private void OnTileSelected(Tile tile)
    {
        selectedTile = tile;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (boardManager != null)
        {
            boardManager.OnTileSelected -= OnTileSelected;
        }
    }
}