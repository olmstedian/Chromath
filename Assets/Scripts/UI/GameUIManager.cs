using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Panel References")]
    [SerializeField] private GameObject topBarPanel;     // Panel for score, time, level
    [SerializeField] private GameObject gamePanel;       // Panel containing the game board
    [SerializeField] private GameObject mainMenuPanel;   // Main menu elements
    [SerializeField] private GameObject pausePanel;      // Pause menu
    [SerializeField] private GameObject winPanel;        // Win screen
    [SerializeField] private GameObject gameOverPanel;   // Game Over screen

    [Header("Gameplay UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Win UI Elements")]
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    [Header("Game Over UI Elements")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
    
    // Keep track of the current UI state
    private enum UIState { MainMenu, Gameplay, Paused, Win, GameOver }
    private UIState currentState;
    
    // Singleton pattern for easy access
    public static GameUIManager Instance { get; private set; }
    
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
    }
    
    private void Start()
    {
        // Initialize UI - Start with gameplay visible instead of main menu
        StartGame();
    }

    // Optional: Add a method to initialize UI without showing any panels
    public void InitializeUIWithoutMenu()
    {
        // Hide all panels first
        if (topBarPanel) topBarPanel.SetActive(false);
        if (gamePanel) gamePanel.SetActive(false);
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        
        // Then only show the game and top bar
        if (gamePanel) gamePanel.SetActive(true);
        if (topBarPanel) topBarPanel.SetActive(true);
        
        // Reset UI values
        ResetGameUI();
        
        // Set the current state
        currentState = UIState.Gameplay;
    }
    
    #region UI State Management
    
    public void ShowMainMenu()
    {
        SetActivePanel(UIState.MainMenu);
    }
    
    public void StartGame()
    {
        SetActivePanel(UIState.Gameplay);
        ResetGameUI();
    }
    
    public void PauseGame()
    {
        SetActivePanel(UIState.Paused);
        Time.timeScale = 0; // Pause the game
    }
    
    public void ResumeGame()
    {
        SetActivePanel(UIState.Gameplay);
        Time.timeScale = 1; // Resume the game
    }
    
    public void ShowWinScreen(int finalScore, int highScore)
    {
        SetActivePanel(UIState.Win);
        UpdateFinalScore(finalScore, highScore);
    }
    
    public void ShowGameOverScreen(int finalScore, int highScore)
    {
        SetActivePanel(UIState.GameOver);
        UpdateGameOverScore(finalScore, highScore);
    }
    
    private void SetActivePanel(UIState newState)
    {
        // Deactivate all panels first
        if (topBarPanel) topBarPanel.SetActive(false);
        if (gamePanel) gamePanel.SetActive(false);
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        
        // Activate appropriate panels based on state
        switch (newState)
        {
            case UIState.MainMenu:
                if (mainMenuPanel) mainMenuPanel.SetActive(true);
                break;
                
            case UIState.Gameplay:
                if (gamePanel) gamePanel.SetActive(true);
                if (topBarPanel) topBarPanel.SetActive(true);
                break;
                
            case UIState.Paused:
                if (gamePanel) gamePanel.SetActive(true); // Keep game visible but frozen
                if (topBarPanel) topBarPanel.SetActive(true);
                if (pausePanel) pausePanel.SetActive(true);
                break;
                
            case UIState.Win:
                if (winPanel) winPanel.SetActive(true);
                break;
                
            case UIState.GameOver:
                if (gameOverPanel) gameOverPanel.SetActive(true);
                break;
        }
        
        currentState = newState;
    }
    
    #endregion
    
    #region UI Updates
    
    public void UpdateScore(int score)
    {
        if (scoreText)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    public void UpdateTime(float time)
    {
        if (timeText)
        {
            // Format time as minutes:seconds
            int minutes = (int)(time / 60);
            int seconds = (int)(time % 60);
            timeText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }
    
    public void UpdateLevel(int level)
    {
        if (levelText)
        {
            levelText.text = $"Level: {level}";
        }
    }
    
    private void UpdateFinalScore(int finalScore, int highScore)
    {
        if (finalScoreText)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }
        
        if (highScoreText)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }
    
    private void UpdateGameOverScore(int finalScore, int highScore)
    {
        if (gameOverScoreText)
        {
            gameOverScoreText.text = $"Your Score: {finalScore}";
        }
        
        if (gameOverHighScoreText)
        {
            gameOverHighScoreText.text = $"High Score: {highScore}";
        }
    }
    
    private void ResetGameUI()
    {
        UpdateScore(0);
        UpdateTime(0);
        UpdateLevel(1);
    }
    
    #endregion
    
    #region Button Actions
    
    // These methods can be connected to UI buttons
    
    public void OnPlayButtonClicked()
    {
        StartGame();
    }
    
    public void OnPauseButtonClicked()
    {
        PauseGame();
    }
    
    public void OnResumeButtonClicked()
    {
        ResumeGame();
    }
    
    public void OnRestartButtonClicked()
    {
        ResumeGame(); // Ensure time scale is reset
        StartGame();
    }
    
    public void OnMainMenuButtonClicked()
    {
        ResumeGame(); // Ensure time scale is reset
        ShowMainMenu();
    }
    
    public void OnQuitButtonClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void OnGameOverRestartButtonClicked()
    {
        ResumeGame(); // Ensure time scale is reset
        StartGame();
    }
    
    #endregion
}
