using UnityEngine;

/// <summary>
/// MainMenuState handles the main menu interactions.
/// </summary>
public class MainMenuState : IGameState
{
    private StateManager stateManager;
    
    public MainMenuState(StateManager manager)
    {
        stateManager = manager;
    }
    
    public void EnterState()
    {
        // Show main menu UI
        GameUIManager uiManager = stateManager.GetUIManager();
        if (uiManager != null)
        {
            uiManager.ShowMainMenu();
        }
    }
    
    public void ExitState()
    {
        // Clean up any main menu elements
    }
    
    public void UpdateState()
    {
        // Check for menu interactions
        HandleInput();
    }
    
    public void HandleInput()
    {
        // Handle menu navigation and button clicks
        // This is generally handled by UI button callbacks
    }
    
    // Play button clicked
    public void OnPlayClicked()
    {
        stateManager.ChangeState(StateManager.GameStateType.Playing);
    }
}

/// <summary>
/// PlayingState handles active gameplay.
/// </summary>
public class PlayingState : IGameState
{
    private StateManager stateManager;
    private float levelTimer;
    
    public PlayingState(StateManager manager)
    {
        stateManager = manager;
    }
    
    public void EnterState()
    {
        // Set up the game world
        GameUIManager uiManager = stateManager.GetUIManager();
        LevelManager levelManager = stateManager.GetLevelManager();
        TileManager tileManager = stateManager.GetTileManager();
        
        // Show game UI
        if (uiManager != null)
        {
            uiManager.StartGame();
        }
        
        // Make sure the board is clean before starting
        if (tileManager != null)
        {
            tileManager.ClearBoard();
            tileManager.ValidateAllTilePositions();
        }
        
        // Start the level if we're coming from main menu
        if (levelManager != null)
        {
            // Always ensure we have a valid level
            if (levelManager.GetCurrentLevel() <= 0)
            {
                levelManager.StartLevel(1);
            }
            else
            {
                // Make sure we're generating initial tiles
                levelManager.GenerateInitialTilesForLevel();
            }
        }
        
        // Set the level timer
        if (levelManager != null)
        {
            levelTimer = levelManager.GetCurrentLevelDuration();
        }
        else
        {
            // Default timer if level manager is missing
            levelTimer = 60f;
        }
        
        // Make sure time scale is normal
        Time.timeScale = 1f;
        
        // Force generate a few initial tiles with proper delay
        if (tileManager != null)
        {
            Debug.Log("Forcing generation of initial tiles in PlayingState.EnterState");
            // Since we can't call StartCoroutine directly (not a MonoBehaviour),
            // use the StateManager to start the coroutine for us
            if (stateManager is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.StartCoroutine(GenerateInitialTiles(tileManager));
            }
            else
            {
                Debug.LogError("Unable to start coroutine - StateManager is not a MonoBehaviour");
                // Fallback: generate tiles directly
                for (int i = 0; i < 3; i++)
                {
                    tileManager.GenerateRandomTile();
                }
            }
        }
    }
    
    private System.Collections.IEnumerator GenerateInitialTiles(TileManager tileManager)
    {
        // Wait for physics to settle
        yield return new WaitForSeconds(0.2f);
        
        // Ensure board state is validated
        tileManager.ValidateAllTilePositions();
        
        // Generate tiles with a small delay between them
        for (int i = 0; i < 3; i++)
        {
            tileManager.GenerateRandomTile();
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    public void ExitState()
    {
        // Clean up gameplay elements
    }
    
    public void UpdateState()
    {
        // Update gameplay logic
        UpdateTimer();
        HandleInput();
    }
    
    private void UpdateTimer()
    {
        // Update level timer
        levelTimer -= Time.deltaTime;
        
        // Update UI
        GameUIManager uiManager = stateManager.GetUIManager();
        if (uiManager != null)
        {
            uiManager.UpdateTime(levelTimer);
        }
        
        // Check for level timeout
        if (levelTimer <= 0)
        {
            // Game over
            stateManager.ChangeState(StateManager.GameStateType.GameOver);
        }
    }
    
    public void HandleInput()
    {
        // Handle gameplay input (mostly handled by InputManager)
        
        // Example: Pause the game when Escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            stateManager.ChangeState(StateManager.GameStateType.Paused);
        }
    }
    
    // Call this when tile movement is complete
    public void OnTileMovementComplete()
    {
        // Generate a new tile
        TileManager tileManager = stateManager.GetTileManager();
        if (tileManager != null)
        {
            tileManager.GenerateRandomTile();
        }
    }
    
    // Call this to refresh the timer (e.g. when starting a new level)
    public void RefreshTimer(float newDuration)
    {
        levelTimer = newDuration;
    }
}

/// <summary>
/// PausedState handles game pause functionality.
/// </summary>
public class PausedState : IGameState
{
    private StateManager stateManager;
    
    public PausedState(StateManager manager)
    {
        stateManager = manager;
    }
    
    public void EnterState()
    {
        // Pause game time
        Time.timeScale = 0f;
        
        // Show pause UI
        GameUIManager uiManager = stateManager.GetUIManager();
        if (uiManager != null)
        {
            uiManager.PauseGame();
        }
    }
    
    public void ExitState()
    {
        // Resume game time
        Time.timeScale = 1f;
    }
    
    public void UpdateState()
    {
        HandleInput();
    }
    
    public void HandleInput()
    {
        // Check for resume input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            stateManager.ChangeState(StateManager.GameStateType.Playing);
        }
    }
    
    // Resume button clicked
    public void OnResumeClicked()
    {
        stateManager.ChangeState(StateManager.GameStateType.Playing);
    }
    
    // Main menu button clicked
    public void OnMainMenuClicked()
    {
        stateManager.ChangeState(StateManager.GameStateType.MainMenu);
    }
}

/// <summary>
/// WinState handles game win condition.
/// </summary>
public class WinState : IGameState
{
    private StateManager stateManager;
    
    public WinState(StateManager manager)
    {
        stateManager = manager;
    }
    
    public void EnterState()
    {
        // Stop gameplay
        Time.timeScale = 0f;
        
        // Show win UI
        GameUIManager uiManager = stateManager.GetUIManager();
        ScoreManager scoreManager = stateManager.GetScoreManager();
        
        if (uiManager != null && scoreManager != null)
        {
            int finalScore = scoreManager.GetCurrentScore();
            int highScore = scoreManager.GetHighScore();
            uiManager.ShowWinScreen(finalScore, highScore);
        }
    }
    
    public void ExitState()
    {
        // Clean up win state
        Time.timeScale = 1f;
    }
    
    public void UpdateState()
    {
        HandleInput();
    }
    
    public void HandleInput()
    {
        // Handle win screen input
    }
    
    // Restart button clicked
    public void OnRestartClicked()
    {
        ResetGame();
        stateManager.ChangeState(StateManager.GameStateType.Playing);
    }
    
    // Main menu button clicked
    public void OnMainMenuClicked()
    {
        ResetGame();
        stateManager.ChangeState(StateManager.GameStateType.MainMenu);
    }
    
    private void ResetGame()
    {
        // Reset score
        ScoreManager scoreManager = stateManager.GetScoreManager();
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }
        
        // Reset level
        LevelManager levelManager = stateManager.GetLevelManager();
        if (levelManager != null)
        {
            levelManager.ResetLevel();
        }
    }
}

/// <summary>
/// GameOverState handles game over condition.
/// </summary>
public class GameOverState : IGameState
{
    private StateManager stateManager;
    
    public GameOverState(StateManager manager)
    {
        stateManager = manager;
    }
    
    public void EnterState()
    {
        // Stop gameplay
        Time.timeScale = 0f;
        
        // Show game over UI
        GameUIManager uiManager = stateManager.GetUIManager();
        ScoreManager scoreManager = stateManager.GetScoreManager();
        
        if (uiManager != null && scoreManager != null)
        {
            int finalScore = scoreManager.GetCurrentScore();
            int highScore = scoreManager.GetHighScore();
            uiManager.ShowGameOverScreen(finalScore, highScore);
        }
    }
    
    public void ExitState()
    {
        // Clean up game over state
        Time.timeScale = 1f;
    }
    
    public void UpdateState()
    {
        HandleInput();
    }
    
    public void HandleInput()
    {
        // Handle game over screen input
    }
    
    // Restart button clicked
    public void OnRestartClicked()
    {
        ResetGame();
        stateManager.ChangeState(StateManager.GameStateType.Playing);
    }
    
    // Main menu button clicked
    public void OnMainMenuClicked()
    {
        ResetGame();
        stateManager.ChangeState(StateManager.GameStateType.MainMenu);
    }
    
    private void ResetGame()
    {
        // Reset score
        ScoreManager scoreManager = stateManager.GetScoreManager();
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }
        
        // Reset level
        LevelManager levelManager = stateManager.GetLevelManager();
        if (levelManager != null)
        {
            levelManager.ResetLevel();
        }
    }
}

/// <summary>
/// LevelTransitionState handles transitions between levels.
/// </summary>
public class LevelTransitionState : IGameState
{
    private StateManager stateManager;
    private int targetLevel;
    
    public LevelTransitionState(StateManager manager)
    {
        stateManager = manager;
    }
    
    public void EnterState()
    {
        // Pause game time during transition
        Time.timeScale = 0f;
        
        // Get current level
        LevelManager levelManager = stateManager.GetLevelManager();
        targetLevel = levelManager != null ? levelManager.GetCurrentLevel() + 1 : 1;
        
        // Show level transition UI
        GameUIManager uiManager = stateManager.GetUIManager();
        if (uiManager != null)
        {
            uiManager.ShowLevelUpPanel(targetLevel);
        }
    }
    
    public void ExitState()
    {
        // Clean up transition state
        Time.timeScale = 1f;
    }
    
    public void UpdateState()
    {
        HandleInput();
    }
    
    public void HandleInput()
    {
        // Handle level transition input (mostly through UI buttons)
    }
    
    // Continue button clicked
    public void OnContinueClicked()
    {
        // Start the next level
        LevelManager levelManager = stateManager.GetLevelManager();
        if (levelManager != null)
        {
            levelManager.StartNextLevelAfterContinue(targetLevel);
        }
        
        // Update timer in PlayingState
        PlayingState playingState = stateManager.GetState(StateManager.GameStateType.Playing) as PlayingState;
        if (playingState != null && levelManager != null)
        {
            playingState.RefreshTimer(levelManager.GetCurrentLevelDuration());
        }
        
        // Transition back to playing state
        stateManager.ChangeState(StateManager.GameStateType.Playing);
    }
}
