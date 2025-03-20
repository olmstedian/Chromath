using System.Collections;
using UnityEngine;

// GameManager class to control game flow and delegate state management to StateManager

public class GameManager : MonoBehaviour
{
    // Manager references
    [Header("Manager References")]
    [SerializeField] private GameUIManager uiManager;
    [SerializeField] private Board boardManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private VisualEffectsManager visualEffects;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private StateManager stateManager; // New reference to StateManager
    
    [Header("Game Settings")]
    [SerializeField] private int startingLevel = 1;
    
    // Local game state variables (for fallback if StateManager isn't available)
    private int localCurrentLevel = 1;
    private bool localIsGameActive = false;
    
    // Legacy enum to support older code
    private enum GameState { MainMenu, Playing, Paused, Win, GameOver }
    private GameState localCurrentState = GameState.MainMenu;
    
    // High score variable (kept in GameManager)
    private int highScore;
    
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Load high score from player prefs
        LoadHighScore();
    }
    
    private void Start()
    {
        // Find references if not assigned
        if (uiManager == null) uiManager = FindObjectOfType<GameUIManager>();
        if (boardManager == null) boardManager = FindObjectOfType<Board>();
        if (inputManager == null) inputManager = FindObjectOfType<InputManager>();
        if (visualEffects == null) visualEffects = FindObjectOfType<VisualEffectsManager>();
        if (scoreManager == null) scoreManager = FindObjectOfType<ScoreManager>();
        if (levelManager == null) levelManager = FindObjectOfType<LevelManager>(); 
        if (stateManager == null) stateManager = FindObjectOfType<StateManager>();
        
        // Add delay to ensure all managers are fully initialized before starting the game
        StartCoroutine(DelayedGameStart());
    }

    // Ensure proper initialization order
    private IEnumerator DelayedGameStart()
    {
        // Wait for one frame to ensure all other Start methods have completed
        yield return null;
        
        // Debug check to verify all managers are found
        Debug.Log($"Starting game with managers: Board:{boardManager!=null}, TileManager:{TileManager.Instance!=null}, LevelManager:{levelManager!=null}, StateManager:{stateManager!=null}");
        
        // Start the game - now handled by StateManager, just in case direct start is needed
        if (stateManager == null)
        {
            StartGame();
        }
    }
    
    // This method is now mostly a wrapper for StateManager
    public void StartGame()
    {
        // If StateManager exists, use it
        if (stateManager != null)
        {
            stateManager.ChangeState(StateManager.GameStateType.Playing);
            return;
        }
        
        // Legacy implementation as fallback
        localCurrentLevel = startingLevel;
        localIsGameActive = true;
        localCurrentState = GameState.Playing;
        
        // Initialize board size and level
        if (boardManager != null && localCurrentLevel == 1)
        {
            boardManager.rows = 4;
            boardManager.columns = 4;
        }
        
        if (levelManager != null)
        {
            levelManager.ResetLevel();
            levelManager.StartLevel(startingLevel);
        }
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.StartGame();
        }
    }
    
    // Methods for state transitions - now delegated to StateManager
    public void PauseGame()
    {
        if (stateManager != null)
        {
            stateManager.ChangeState(StateManager.GameStateType.Paused);
            return;
        }
        
        // Legacy fallback
        localCurrentState = GameState.Paused;
        Time.timeScale = 0f;
        
        if (uiManager != null)
        {
            uiManager.PauseGame();
        }
    }
    
    public void ResumeGame()
    {
        if (stateManager != null)
        {
            stateManager.ChangeState(StateManager.GameStateType.Playing);
            return;
        }
        
        // Legacy fallback
        localCurrentState = GameState.Playing;
        Time.timeScale = 1f;
        
        if (uiManager != null)
        {
            uiManager.ResumeGame();
        }
    }
    
    public void GameOver()
    {
        if (stateManager != null)
        {
            stateManager.ChangeState(StateManager.GameStateType.GameOver);
            return;
        }
        
        // Legacy fallback
        localCurrentState = GameState.GameOver;
        localIsGameActive = false;
        Time.timeScale = 0f;
        
        if (uiManager != null && scoreManager != null)
        {
            int finalScore = scoreManager.GetCurrentScore();
            int highScore = scoreManager.GetHighScore();
            uiManager.ShowGameOverScreen(finalScore, highScore);
        }
    }
    
    public void Win()
    {
        if (stateManager != null)
        {
            stateManager.ChangeState(StateManager.GameStateType.Win);
            return;
        }
        
        // Legacy fallback
        localCurrentState = GameState.Win;
        localIsGameActive = false;
        Time.timeScale = 0f;
        
        if (uiManager != null && scoreManager != null)
        {
            int finalScore = scoreManager.GetCurrentScore();
            int highScore = scoreManager.GetHighScore();
            uiManager.ShowWinScreen(finalScore, highScore);
        }
    }
    
    // This method is called after each tile move to spawn a new random tile
    public void OnTileMovementComplete()
    {
        // Only proceed if game is active
        if (!IsGameActive()) return;
        
        Debug.Log("Spawning new tile after tile movement...");
        
        // Delegate to StateManager if available
        if (stateManager != null)
        {
            PlayingState playingState = stateManager.GetState(StateManager.GameStateType.Playing) as PlayingState;
            if (playingState != null)
            {
                playingState.OnTileMovementComplete();
                return;
            }
        }
        
        // Fallback to direct TileManager if StateManager not available
        if (TileManager.Instance != null)
        {
            TileManager.Instance.GenerateRandomTile();
        }
    }
    
    // Helper method to check if game is active
    public bool IsGameActive()
    {
        // Delegate to StateManager if available
        if (stateManager != null)
        {
            return stateManager.IsGameActive();
        }
        
        // Legacy check
        return localIsGameActive && localCurrentState == GameState.Playing;
    }
    
    // Add this new method to set the level timer
    public void SetLevelTimer(float time)
    {
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateTime(time);
        }
    }
    
    #region Persistence
    
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }
    
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }
    
    #endregion
}
