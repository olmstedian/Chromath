using UnityEngine;
using UnityEngine.UI;

public class DebugGameController : MonoBehaviour
{
    [SerializeField] private Button spawnTileButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button advanceLevelButton;
    
    private void Start()
    {
        // Find buttons if not assigned
        if (spawnTileButton == null)
            spawnTileButton = transform.Find("SpawnTileButton")?.GetComponent<Button>();
        
        if (startGameButton == null)
            startGameButton = transform.Find("StartGameButton")?.GetComponent<Button>();
            
        if (advanceLevelButton == null)
            advanceLevelButton = transform.Find("AdvanceLevelButton")?.GetComponent<Button>();
        
        // Connect button events
        if (spawnTileButton != null)
        {
            spawnTileButton.onClick.AddListener(SpawnTile);
        }
        
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
        }
        
        if (advanceLevelButton != null)
        {
            advanceLevelButton.onClick.AddListener(AdvanceLevel);
        }
    }
    
    // Spawn a new tile for testing
    public void SpawnTile()
    {
        Debug.Log("Debug: Spawning tile via debug button");
        
        if (TileManager.Instance != null)
        {
            TileManager.Instance.GenerateRandomTile();
        }
        else
        {
            Debug.LogError("TileManager.Instance is null");
        }
    }
    
    // Start the game for testing
    public void StartGame()
    {
        Debug.Log("Debug: Starting game via debug button");
        
        if (StateManager.Instance != null)
        {
            StateManager.Instance.ChangeState(StateManager.GameStateType.Playing);
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("Neither StateManager nor GameManager found");
        }
    }
    
    // Advance to the next level for testing
    public void AdvanceLevel()
    {
        Debug.Log("Debug: Advancing level via debug button");
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AdvanceToNextLevel();
        }
        else
        {
            Debug.LogError("LevelManager.Instance is null");
        }
    }
    
    // Add debug tiles at key press
    private void Update()
    {
        // Press T to spawn a tile
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnTile();
        }
        
        // Press G to start game
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartGame();
        }
        
        // Press L to advance level
        if (Input.GetKeyDown(KeyCode.L))
        {
            AdvanceLevel();
        }
    }
}
