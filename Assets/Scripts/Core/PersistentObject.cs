using UnityEngine;

public class PersistentManagers : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private ObstacleManager obstacleManager;
    [SerializeField] private TileAnimationManager tileAnimationManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private StateManager stateManager;
    [SerializeField] private GameUIManager uiManager;
    
    private void Awake()
    {
        // Ensure this GameObject persists across scenes
        DontDestroyOnLoad(gameObject);
        
        // Check for required managers and create them if needed
        InitializeManagers();
    }
    
    // Initialize any missing managers
    private void InitializeManagers()
    {
        // Only create managers that aren't already referenced
        if (gameManager == null)
        {
            gameManager = GetComponentInChildren<GameManager>();
            if (gameManager == null && transform.Find("GameManager") == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gmObj.transform.SetParent(transform);
                gameManager = gmObj.AddComponent<GameManager>();
                Debug.Log("Created GameManager");
            }
        }
        
        if (tileManager == null)
        {
            tileManager = GetComponentInChildren<TileManager>();
            if (tileManager == null && transform.Find("TileManager") == null)
            {
                GameObject tmObj = new GameObject("TileManager");
                tmObj.transform.SetParent(transform);
                tileManager = tmObj.AddComponent<TileManager>();
                Debug.Log("Created TileManager");
            }
        }
        
        if (obstacleManager == null)
        {
            obstacleManager = GetComponentInChildren<ObstacleManager>();
            if (obstacleManager == null && transform.Find("ObstacleManager") == null)
            {
                GameObject omObj = new GameObject("ObstacleManager");
                omObj.transform.SetParent(transform);
                obstacleManager = omObj.AddComponent<ObstacleManager>();
                Debug.Log("Created ObstacleManager");
            }
        }
        
        // Add initialization for TileAnimationManager
        if (tileAnimationManager == null)
        {
            tileAnimationManager = GetComponentInChildren<TileAnimationManager>();
            if (tileAnimationManager == null && transform.Find("TileAnimationManager") == null)
            {
                GameObject tamObj = new GameObject("TileAnimationManager");
                tamObj.transform.SetParent(transform);
                tileAnimationManager = tamObj.AddComponent<TileAnimationManager>();
                Debug.Log("Created TileAnimationManager");
            }
        }
        
        // Initialize other managers as needed
        InitializeManager<ScoreManager>(ref scoreManager, "ScoreManager");
        InitializeManager<LevelManager>(ref levelManager, "LevelManager");
        InitializeManager<StateManager>(ref stateManager, "StateManager");
        InitializeManager<GameUIManager>(ref uiManager, "GameUIManager");
    }
    
    // Generic method to initialize manager components
    private void InitializeManager<T>(ref T manager, string managerName) where T : Component
    {
        if (manager == null)
        {
            manager = GetComponentInChildren<T>();
            if (manager == null && transform.Find(managerName) == null)
            {
                GameObject obj = new GameObject(managerName);
                obj.transform.SetParent(transform);
                manager = obj.AddComponent<T>();
                Debug.Log($"Created {managerName}");
            }
        }
    }
}
