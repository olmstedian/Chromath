using UnityEngine;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    // Score variables
    private int currentScore = 0;
    private int highScore = 0;
    
    // Combo variables
    private int currentCombo = 0;
    private float comboTimer = 0f;
    private float comboTimerMax = 2f;
    private bool isComboActive = false;
    
    // Multiplier variables
    private float scoreMultiplier = 1f;
    private const float baseMultiplier = 1f;
    
    // PlayerPrefs keys
    private const string HighScoreKey = "HighScore";
    
    // References
    [SerializeField] private GameUIManager uiManager;
    
    // Singleton pattern
    public static ScoreManager Instance { get; private set; }
    
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
        
        // Load high score
        LoadHighScore();
    }
    
    private void Start()
    {
        // Find UI manager if not assigned
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<GameUIManager>();
        }
    }
    
    private void Update()
    {
        // Update combo timer if combo is active
        if (isComboActive)
        {
            comboTimer -= Time.deltaTime;
            
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }
    
    #region Score Methods
    
    // Add points with current multiplier
    public void AddPoints(int basePoints)
    {
        int pointsToAdd = Mathf.RoundToInt(basePoints * scoreMultiplier);
        currentScore += pointsToAdd;
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(currentScore);
        }
        
        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
    }
    
    // Add points with specific multiplier
    public void AddPoints(int basePoints, float multiplier)
    {
        int pointsToAdd = Mathf.RoundToInt(basePoints * multiplier);
        currentScore += pointsToAdd;
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(currentScore);
        }
        
        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
    }
    
    // Reset score
    public void ResetScore()
    {
        currentScore = 0;
        ResetMultiplier();
        ResetCombo();
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(currentScore);
        }
    }
    
    #endregion
    
    #region Combo Methods
    
    // Increment combo and update multiplier
    public void IncrementCombo()
    {
        currentCombo++;
        isComboActive = true;
        comboTimer = comboTimerMax;
        
        // Update multiplier based on combo
        UpdateMultiplierFromCombo();
        
        // Visual/audio feedback could be triggered here
    }
    
    // Reset combo
    public void ResetCombo()
    {
        currentCombo = 0;
        isComboActive = false;
        
        // Reset multiplier
        ResetMultiplier();
    }
    
    private void UpdateMultiplierFromCombo()
    {
        // Example multiplier progression:
        // Combo 0-1: 1x
        // Combo 2-3: 1.5x
        // Combo 4-5: 2x
        // Combo 6+: 3x
        
        if (currentCombo <= 1)
        {
            scoreMultiplier = baseMultiplier;
        }
        else if (currentCombo <= 3)
        {
            scoreMultiplier = baseMultiplier * 1.5f;
        }
        else if (currentCombo <= 5)
        {
            scoreMultiplier = baseMultiplier * 2f;
        }
        else
        {
            scoreMultiplier = baseMultiplier * 3f;
        }
    }
    
    #endregion
    
    #region Multiplier Methods
    
    // Set multiplier
    public void SetMultiplier(float multiplier)
    {
        scoreMultiplier = multiplier;
    }
    
    // Reset multiplier to base value
    public void ResetMultiplier()
    {
        scoreMultiplier = baseMultiplier;
    }
    
    // Add temporary multiplier boost
    public void AddMultiplierBoost(float boostAmount, float duration)
    {
        StartCoroutine(MultiplierBoostCoroutine(boostAmount, duration));
    }
    
    private IEnumerator MultiplierBoostCoroutine(float boostAmount, float duration)
    {
        // Store original multiplier
        float originalMultiplier = scoreMultiplier;
        
        // Apply boost
        scoreMultiplier += boostAmount;
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Restore original multiplier
        scoreMultiplier = originalMultiplier;
    }
    
    #endregion
    
    #region High Score Management
    
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HighScoreKey, highScore);
        PlayerPrefs.Save();
    }
    
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }
    
    // Reset high score
    public void ResetHighScore()
    {
        highScore = 0;
        SaveHighScore();
    }
    
    #endregion
    
    #region Getters
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    public int GetHighScore()
    {
        return highScore;
    }
    
    public int GetCurrentCombo()
    {
        return currentCombo;
    }
    
    public float GetScoreMultiplier()
    {
        return scoreMultiplier;
    }
    
    #endregion
    
    #region Game Event Handlers
    
    // Call this when a match is made
    public void OnMatchMade(int matchSize)
    {
        // Calculate base points (e.g., 100 per tile)
        int basePoints = 100 * matchSize;
        
        // Increment combo
        IncrementCombo();
        
        // Add points
        AddPoints(basePoints);
    }
    
    // Call this when a special move is performed
    public void OnSpecialMove(int basePoints)
    {
        // Special moves get a 2x multiplier
        AddPoints(basePoints, 2f);
        
        // Also increment combo
        IncrementCombo();
    }
    
    // Call this at the end of the game
    public int FinalizeScore()
    {
        // Check for high score one last time
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
        
        return currentScore;
    }
    
    #endregion
}
