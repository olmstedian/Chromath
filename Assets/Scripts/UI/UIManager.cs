using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;
    
    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    
    private void Start()
    {
        // Subscribe to board events
        if (boardManager != null)
        {
            boardManager.OnScoreChanged += UpdateScore;
            boardManager.OnGameOver += ShowGameOver;
            boardManager.OnGameWon += ShowWin;
        }
        
        // Add button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        // Hide end game panels
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        
        // Initialize score
        UpdateScore(0);
    }
    
    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
    
    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + scoreText.text;
            }
        }
    }
    
    private void ShowWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + scoreText.text;
            }
        }
    }
    
    private void RestartGame()
    {
        // Hide panels
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        
        // Reset the game
        if (boardManager != null)
        {
            boardManager.RestartGame();
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (boardManager != null)
        {
            boardManager.OnScoreChanged -= UpdateScore;
            boardManager.OnGameOver -= ShowGameOver;
            boardManager.OnGameWon -= ShowWin;
        }
        
        // Remove button listeners
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }
    }
}