using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle References")]
    [SerializeField] private GameObject obstacleTilePrefab;
    [SerializeField] private Board boardManager;
    
    [Header("Obstacle Settings")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private float durabilityMultiplier = 1.0f; // Higher levels could have more durable obstacles
    
    // Tracking dictionaries
    private Dictionary<Vector2Int, GameObject> obstaclePositions = new Dictionary<Vector2Int, GameObject>();
    
    // Object pooling
    private Queue<GameObject> obstaclePool = new Queue<GameObject>();
    
    // Singleton instance
    public static ObstacleManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Find board reference if not assigned
        if (boardManager == null)
            boardManager = FindObjectOfType<Board>();
        
        if (boardManager == null)
        {
            Debug.LogError("ObstacleManager could not find Board reference! Obstacles cannot be placed.");
            return;
        }
        
        // Initialize obstacle pool
        InitializeObstaclePool();
        
        Debug.Log($"ObstacleManager initialized with board: {boardManager.name}");
    }
    
    #region Obstacle Pool Management
    
    private void InitializeObstaclePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateObstacleForPool();
        }
    }
    
    private void CreateObstacleForPool()
    {
        GameObject obstacle = Instantiate(obstacleTilePrefab, Vector3.zero, Quaternion.identity, transform);
        obstacle.SetActive(false);
        
        // Initialize obstacle tile
        ObstacleTile obstacleTile = obstacle.GetComponent<ObstacleTile>();
        if (obstacleTile != null)
        {
            // Any specific obstacle initialization can go here
        }
        
        obstaclePool.Enqueue(obstacle);
    }
    
    private GameObject GetObstacleFromPool()
    {
        if (obstaclePool.Count == 0)
        {
            CreateObstacleForPool();
        }
        
        GameObject obstacle = obstaclePool.Dequeue();
        obstacle.SetActive(true);
        return obstacle;
    }
    
    private void ReturnObstacleToPool(GameObject obstacle)
    {
        if (obstacle == null) return;
        
        obstacle.SetActive(false);
        obstacle.transform.SetParent(transform);
        obstaclePool.Enqueue(obstacle);
    }
    
    #endregion
    
    #region Obstacle Placement
    
    // Create an obstacle at a specific grid position
    public GameObject CreateObstacleAt(Vector2Int gridPosition)
    {
        if (boardManager == null)
        {
            Debug.LogError("Cannot create obstacle - Board reference is missing!");
            return null;
        }
        
        // Check if there's already something at this position (using TileManager)
        if (TileManager.Instance != null && TileManager.Instance.IsTileAt(gridPosition))
        {
            Debug.LogWarning($"Cannot place obstacle at {gridPosition} - position is already occupied");
            return null;
        }
        
        // Check if we already have an obstacle here
        if (obstaclePositions.ContainsKey(gridPosition))
        {
            Debug.LogWarning($"Obstacle already exists at position {gridPosition}");
            return null;
        }
        
        // Get obstacle from pool
        GameObject obstacle = GetObstacleFromPool();
        
        // Get world position
        Vector3 worldPosition = GetWorldPositionFromGrid(gridPosition);
        
        // Position the obstacle
        obstacle.transform.position = worldPosition;
        obstacle.transform.SetParent(boardManager.transform);
        
        // Ensure proper z-position
        Vector3 pos = obstacle.transform.position;
        obstacle.transform.position = new Vector3(pos.x, pos.y, -0.1f);
        
        // Set scale
        obstacle.transform.localScale = new Vector3(boardManager.tileSize, boardManager.tileSize, 1f);
        
        // Track the obstacle
        obstaclePositions[gridPosition] = obstacle;
        
        // Make sure renderer has high sorting order
        SpriteRenderer renderer = obstacle.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 10;
        }
        
        Debug.Log($"Created obstacle at position {gridPosition}");
        
        return obstacle;
    }
    
    // Generate a random obstacle on the board
    public GameObject GenerateRandomObstacle()
    {
        if (boardManager == null)
        {
            Debug.LogError("Cannot generate obstacle - Board reference is missing!");
            return null;
        }
        
        // Find an empty spot on the board
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        
        for (int row = 0; row < boardManager.rows; row++)
        {
            for (int col = 0; col < boardManager.columns; col++)
            {
                Vector2Int position = new Vector2Int(col, row);
                
                // Skip if TileManager has a tile here
                if (TileManager.Instance != null && TileManager.Instance.IsTileAt(position))
                {
                    continue;
                }
                
                // Skip if we already have an obstacle here
                if (obstaclePositions.ContainsKey(position))
                {
                    continue;
                }
                
                // This position is available
                emptyPositions.Add(position);
            }
        }
        
        if (emptyPositions.Count == 0)
        {
            Debug.LogWarning("No empty spaces for obstacle tiles!");
            return null;
        }
        
        // Select a random empty position
        Vector2Int randomPosition = emptyPositions[Random.Range(0, emptyPositions.Count)];
        
        // Create the obstacle
        GameObject obstacle = CreateObstacleAt(randomPosition);
        
        // Add scale-in animation
        if (obstacle != null)
        {
            StartCoroutine(ScaleObstacleIn(obstacle));
        }
        
        return obstacle;
    }
    
    // Animate obstacle appearing
    private IEnumerator ScaleObstacleIn(GameObject obstacle)
    {
        // Start with a tiny scale
        Vector3 originalScale = obstacle.transform.localScale;
        obstacle.transform.localScale = originalScale * 0.1f;
        
        float duration = 0.3f;
        float elapsed = 0;
        
        // Scale up to normal size
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            obstacle.transform.localScale = Vector3.Lerp(originalScale * 0.1f, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final scale is correct
        obstacle.transform.localScale = originalScale;
    }
    
    #endregion
    
    #region Obstacle Management
    
    // Check if there's an obstacle at a specific position
    public bool IsObstacleAt(Vector2Int gridPosition)
    {
        return obstaclePositions.ContainsKey(gridPosition);
    }
    
    // Get the obstacle at a specific position
    public GameObject GetObstacleAt(Vector2Int gridPosition)
    {
        if (IsObstacleAt(gridPosition))
        {
            return obstaclePositions[gridPosition];
        }
        return null;
    }
    
    // Remove an obstacle at a specific position
    public void RemoveObstacleAt(Vector2Int gridPosition)
    {
        if (!IsObstacleAt(gridPosition)) return;
        
        GameObject obstacle = obstaclePositions[gridPosition];
        obstaclePositions.Remove(gridPosition);
        
        // Return to pool
        ReturnObstacleToPool(obstacle);
    }
    
    // Clear all obstacles
    public void ClearAllObstacles()
    {
        // Create a list of positions to avoid modifying dictionary during iteration
        List<Vector2Int> positions = new List<Vector2Int>(obstaclePositions.Keys);
        
        foreach (Vector2Int pos in positions)
        {
            RemoveObstacleAt(pos);
        }
        
        obstaclePositions.Clear();
    }
    
    // Damage an obstacle
    public void DamageObstacle(Vector2Int gridPosition, int damage = 1)
    {
        if (!IsObstacleAt(gridPosition)) return;
        
        GameObject obstacle = obstaclePositions[gridPosition];
        ObstacleTile obstacleTile = obstacle.GetComponent<ObstacleTile>();
        
        if (obstacleTile != null)
        {
            bool destroyed = obstacleTile.TakeDamage(damage);
            if (destroyed)
            {
                // Obstacle was destroyed
                RemoveObstacleAt(gridPosition);
                
                // Notify score system
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddPoints(50); // Points for destroying obstacle
                }
            }
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    // Helper to convert grid position to world position
    private Vector3 GetWorldPositionFromGrid(Vector2Int gridPosition)
    {
        if (boardManager == null)
        {
            Debug.LogError("Board reference is missing in ObstacleManager!");
            return Vector3.zero;
        }
        
        return boardManager.GetWorldPosition(gridPosition);
    }
    
    // Set obstacle durability based on level
    public void SetObstacleDurability(float multiplier)
    {
        durabilityMultiplier = multiplier;
    }
    
    #endregion
    
    // Add new method to adapt the ObstacleTile script
    public void UpdateObstacleTileScript(GameObject obstacleTile, int durability)
    {
        ObstacleTile tileScript = obstacleTile.GetComponent<ObstacleTile>();
        if (tileScript != null)
        {
            tileScript.SetDurability(Mathf.RoundToInt(durability * durabilityMultiplier));
        }
    }
}
