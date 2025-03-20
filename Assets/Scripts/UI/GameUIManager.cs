using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Panel References")]
    [SerializeField] private GameObject topBarPanel;     // Panel for score, time, level
    [SerializeField] private GameObject gamePanel;       // Panel containing the game board
    [SerializeField] private GameObject mainMenuPanel;   // Main menu elements
    [SerializeField] private GameObject pausePanel;      // Pause menu
    [SerializeField] private GameObject winPanel;        // Win screen
    [SerializeField] private GameObject gameOverPanel;   // Game Over screen
    [SerializeField] private GameObject levelUpPanel;    // Level Up notification panel

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

    [Header("Level Up UI Elements")]
    [SerializeField] private TextMeshProUGUI levelUpText;
    [SerializeField] private TextMeshProUGUI newLevelText;
    [SerializeField] private Image levelUpFillBar;
    [SerializeField] private Button continueButton; // Add continue button reference
    
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
        
        // If this isn't the first level, show the level up panel (unless suppressed)
        if (level > 1 && currentState == UIState.Gameplay && !SuppressLevelUpPanel)
        {
            ShowLevelUpPanel(level);
        }
    }

    // Add a new method that updates the level text without showing the level up panel
    public void UpdateLevelSilently(int level)
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

    // Method to show the level up panel with continue button
    public void ShowLevelUpPanel(int newLevel)
    {
        if (levelUpPanel != null)
        {
            // Set the text values
            if (newLevelText != null)
            {
                newLevelText.text = $"Level {newLevel}";
            }
            
            if (levelUpText != null)
            {
                // Add different messages based on level
                string levelMessage = "Level Up!";
                
                if (newLevel > 5)
                {
                    levelMessage = "Getting Harder!";
                }
                
                if (newLevel > 8)
                {
                    levelMessage = "Master Level!";
                }
                
                levelUpText.text = levelMessage;
            }
            
            // Setup the continue button
            if (continueButton != null)
            {
                // Clear existing listeners and add new one
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(() => OnContinueToNextLevel(newLevel));
            }
            
            // Ensure the panel is in front of everything
            Canvas levelUpCanvas = levelUpPanel.GetComponent<Canvas>();
            if (levelUpCanvas != null)
            {
                // Make sure it has the highest sorting order
                levelUpCanvas.sortingOrder = 100;
            }
            else
            {
                // If it doesn't have its own canvas, adjust any canvas group
                CanvasGroup canvasGroup = levelUpPanel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    // Make sure it's fully visible
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
                
                // Force it to the foreground by setting its position
                RectTransform rect = levelUpPanel.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.SetAsLastSibling();
                }
            }
            
            // Deactivate the game panel to ensure the level up panel is fully visible
            if (gamePanel != null)
            {
                gamePanel.SetActive(false);
            }
            
            // Pause the game while showing the level up panel
            Time.timeScale = 0;
            
            // Make the panel active
            levelUpPanel.SetActive(true);
            
            // Play panel entry animation if you have one
            StartCoroutine(AnimateLevelUpPanel(false)); // false = don't auto-hide
        }
    }

    // New method to handle continue button click
    private void OnContinueToNextLevel(int level)
    {
        // Hide the level up panel
        levelUpPanel.SetActive(false);
        
        // Re-activate the game panel
        if (gamePanel != null)
        {
            gamePanel.SetActive(true);
        }
        
        // Resume game time
        Time.timeScale = 1;
        
        // Call level manager to start the next level - prevent showing level up panel again
        if (LevelManager.Instance != null)
        {
            Debug.Log($"Continue button clicked - transitioning to level {level}");
            
            // Temporarily disable showing level up panel while transitioning
            SuppressLevelUpPanel = true;
            
            LevelManager.Instance.StartNextLevelAfterContinue(level);
        }
        else
        {
            Debug.LogError("Cannot continue to next level - LevelManager.Instance is null");
        }
    }

    // Updated animation for level up panel (with auto-hide parameter)
    private IEnumerator AnimateLevelUpPanel(bool autoHide = true)
    {
        // Scale up effect for panel
        RectTransform panelRect = levelUpPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            Vector3 startScale = panelRect.localScale * 0.8f;
            Vector3 endScale = panelRect.localScale;
            panelRect.localScale = startScale;
            
            // Animate scaling and fill bar simultaneously
            float animationTime = 1.0f;
            float elapsed = 0f;
            
            while (elapsed < animationTime)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled time because game is paused
                float t = elapsed / animationTime;
                
                // Update scale
                panelRect.localScale = Vector3.Lerp(startScale, endScale, t);
                
                // Update fill bar if it exists
                if (levelUpFillBar != null)
                {
                    levelUpFillBar.fillAmount = t;
                }
                
                yield return null;
            }
            
            // Ensure we end at exactly the right values
            panelRect.localScale = endScale;
            if (levelUpFillBar != null)
            {
                levelUpFillBar.fillAmount = 1f;
            }
        }
        
        // If autoHide, wait and then hide the panel
        if (autoHide)
        {
            // Wait for user to read
            yield return new WaitForSecondsRealtime(2.0f);
            
            // Fade out effect
            CanvasGroup canvasGroup = levelUpPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                float fadeTime = 0.5f;
                float elapsed = 0f;
                
                while (elapsed < fadeTime)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                    yield return null;
                }
                
                canvasGroup.alpha = 0f;
            }
            
            // Hide the panel
            levelUpPanel.SetActive(false);
            
            // Reset opacity if we used fade effect
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
            
            // Resume game time
            Time.timeScale = 1;
        }
    }

    // Add this property to control whether level up panel should appear
    public bool SuppressLevelUpPanel { get; set; } = false;
}
