using System.Collections;
using UnityEngine;

// GameManager class to control game flow and state management 

public class GameManager : MonoBehaviour
{
    // Game state enum
    public enum GameState { MainMenu, Playing, Paused, Win, GameOver }
    private GameState currentState;
    
    // Manager references
    [Header("Manager References")]
    [SerializeField] private GameUIManager uiManager;
    [SerializeField] private Board boardManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private VisualEffectsManager visualEffects;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LevelManager levelManager; // Add LevelManager reference
    
    [Header("Game Settings")]
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private float levelDuration = 60f; // Seconds per level
    [SerializeField] private int scorePerMatch = 100;
    [SerializeField] private float matchTimeBonus = 5f;
    
    // Game state variables
    private int currentLevel;
    private int currentScore;
    private int highScore;
    private float levelTimer;
    private bool isGameActive;
    
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        if (levelManager == null) levelManager = FindObjectOfType<LevelManager>(); // Find LevelManager
        
        // Initialize the game
        StartGame();
    }
    
    private void Update()
    {
        if (currentState == GameState.Playing)
        {
            // Update level timer - use LevelManager's duration if available
            if (levelManager != null)
            {
                levelTimer -= Time.deltaTime;
                uiManager.UpdateTime(levelTimer);
                
                // Check for level completion or game over
                if (levelTimer <= 0)
                {
                    GameOver();
                }
            }
            else 
            {
                // Fallback to original timer logic
                levelTimer -= Time.deltaTime;
                uiManager.UpdateTime(levelTimer);
                
                if (levelTimer <= 0)
                {
                    GameOver();
                }
            }
        }
    }
    
    #region Game Flow Control
    
    public void StartGame()
    {
        // Reset game variables
        currentLevel = startingLevel;
        currentScore = 0;
        isGameActive = true;
        
        // Initialize timer based on LevelManager or fallback
        if (levelManager != null)
        {
            // Configure board size based on level
            if (boardManager != null)
            {
                // For level 1, set board size to 4x4
                if (currentLevel == 1)
                {
                    boardManager.rows = 4;
                    boardManager.columns = 4;
                }
            }
            
            levelManager.ResetLevel();
            levelManager.StartLevel(startingLevel);
            levelTimer = levelManager.GetCurrentLevelDuration();
            currentLevel = levelManager.GetCurrentLevel();
            
            // IMPORTANT: Remove this since LevelManager will handle initial tile generation
            // We don't want both GameManager and LevelManager generating tiles
            /*
            // Generate initial random tiles
            if (currentLevel == 1)
            {
                GenerateInitialTiles(3); // Generate 3 random tiles for level 1
            }
            */
        }
        else
        {
            levelTimer = levelDuration;
            
            // Only generate initial tiles if LevelManager doesn't exist
            if (currentLevel == 1)
            {
                GenerateInitialTiles(3); // Generate 3 random tiles for level 1
            }
        }
        
        // Update UI
        uiManager.StartGame();
        uiManager.UpdateLevel(currentLevel);
        uiManager.UpdateScore(currentScore);
        uiManager.UpdateTime(levelTimer);
        
        // Set game state
        SetGameState(GameState.Playing);
    }
    
    // Generate initial random tiles at game start
    private void GenerateInitialTiles(int count)
    {
        // Use TileManager instance directly instead of explicit namespace
        if (TileManager.Instance != null)
        {
            for (int i = 0; i < count; i++)
            {
                TileManager.Instance.GenerateRandomTile();
            }
        }
    }
    
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
            
            // Pause level-specific systems
            if (levelManager != null)
            {
                levelManager.StopTileGeneration();
            }
            
            uiManager.PauseGame();
        }
    }
    
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
            
            // Resume level-specific systems
            if (levelManager != null)
            {
                levelManager.StartTileGeneration();
            }
            
            uiManager.ResumeGame();
        }
    }
    
    public void NextLevel()
    {
        if (levelManager != null)
        {
            levelManager.AdvanceToNextLevel();
            currentLevel = levelManager.GetCurrentLevel();
            levelTimer = levelManager.GetCurrentLevelDuration();
        }
        else
        {
            currentLevel++;
            levelTimer = levelDuration;
            IncreaseDifficulty();
        }
        
        // Update UI
        uiManager.UpdateLevel(currentLevel);
        uiManager.UpdateTime(levelTimer);
    }
    
    public void GameOver()
    {
        isGameActive = false;
        SetGameState(GameState.GameOver);
        
        // Stop level systems
        if (levelManager != null)
        {
            levelManager.StopTileGeneration();
        }
        
        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
        
        // Show game over screen
        uiManager.ShowGameOverScreen(currentScore, highScore);
    }
    
    public void Win()
    {
        isGameActive = false;
        SetGameState(GameState.Win);
        
        // Stop level systems
        if (levelManager != null)
        {
            levelManager.StopTileGeneration();
        }
        
        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
        
        // Show win screen
        uiManager.ShowWinScreen(currentScore, highScore);
    }
    
    private void SetGameState(GameState newState)
    {
        currentState = newState;
        // Additional state-specific logic can be added here
    }
    
    #endregion
    
    #region Gameplay Methods
    
    public void AddScore(int points)
    {
        currentScore += points;
        uiManager.UpdateScore(currentScore);
    }
    
    public void AddTime(float seconds)
    {
        levelTimer += seconds;
        uiManager.UpdateTime(levelTimer);
    }
    
    public void OnTileMatched(int matchSize)
    {
        // Call ScoreManager to handle the scoring
        scoreManager.OnMatchMade(matchSize);
        
        // Add time bonus
        AddTime(matchTimeBonus);
        
        // Check for level progression via LevelManager or locally
        if (levelManager != null)
        {
            // LevelManager will handle progression based on score
        }
        else
        {
            CheckWinCondition();
        }
    }
    
    private void CheckWinCondition()
    {
        // TODO: Implement win condition (e.g., clear all tiles, reach score threshold)
        // For now, we'll just use a placeholder
        
        // Example: Win if score exceeds 1000 per level
        if (currentScore >= 1000 * currentLevel)
        {
            // Level complete!
            StartCoroutine(LevelCompleteSequence());
        }
    }
    
    private IEnumerator LevelCompleteSequence()
    {
        // Show level complete visual feedback
        // TODO: Add level complete animation/effects
        
        yield return new WaitForSeconds(2f);
        
        // Check if final level
        if (currentLevel >= 10) // Example: 10 levels max
        {
            Win();
        }
        else
        {
            NextLevel();
        }
    }
    
    private void IncreaseDifficulty()
    {
        // Introduce more obstacles as difficulty increases
        if (levelManager != null)
        {
            // Chance to introduce an obstacle
            if (currentLevel >= 2 && Random.value < 0.5f)
            {
                levelManager.IntroduceSpecialElement(0); // Introduce obstacles
            }
        }
        
        // TODO: Implement difficulty scaling based on level
        // Examples:
        // - Decrease time bonus
        // - Require more matches
        // - Add more tile types
        // - Introduce obstacles
    }
    
    // This method is called after each tile move to spawn a new random tile
    public void OnTileMovementComplete()
    {
        // Only generate a new tile when in Playing state
        if (currentState == GameState.Playing)
        {
            Debug.Log("Spawning new tile after tile movement...");
            
            // Use LevelManager if available to handle color probabilities
            if (levelManager != null)
            {
                levelManager.GenerateRandomTile();
            }
            // Fallback to direct TileManager if LevelManager not available
            else if (TileManager.Instance != null)
            {
                TileManager.Instance.GenerateRandomTile();
            }
        }
    }

    // Add this new method to set the level timer
    public void SetLevelTimer(float time)
    {
        levelTimer = time;
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateTime(levelTimer);
        }
    }

    #endregion
    
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
    
    #region Public Helper Methods
    
    // Check if game is in an active playable state
    public bool IsGameActive()
    {
        return isGameActive && currentState == GameState.Playing;
    }
    
    // Get current game state
    public GameState GetGameState()
    {
        return currentState;
    }
    
    #endregion
}
