using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int maxLevel = 10;
    [SerializeField] private float baseLevelDuration = 60f;
    [SerializeField] private float levelTimeDecreasePercentage = 0.1f; // 10% less time per level
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float initialTileGenerationInterval = 2.0f;
    [SerializeField] private float minimumTileGenerationInterval = 0.5f;
    [SerializeField] private float tileGenerationDecreaseRate = 0.1f; // Decrease by 10% per level
    [SerializeField] private int[] scoreThresholds; // Score required to reach each level
    
    [Header("Tile Probability")]
    [SerializeField] private AnimationCurve redTileProbability;
    [SerializeField] private AnimationCurve greenTileProbability;
    [SerializeField] private AnimationCurve blueTileProbability;
    [SerializeField] private AnimationCurve yellowTileProbability;
    
    // References to other managers
    private GameManager gameManager;
    private ScoreManager scoreManager;
    private TileManager tileManager;
    
    // Level state
    private int currentLevel;
    private float currentLevelDuration;
    private float currentTileGenerationInterval;
    private float tileGenerationTimer;
    private bool isGeneratingTiles = false;
    
    // Singleton instance
    public static LevelManager Instance { get; private set; }
    
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
        
        // Initialize default probabilities if not set
        if (redTileProbability.keys.Length == 0)
            SetDefaultProbabilityCurve(ref redTileProbability, 0.25f);
        if (greenTileProbability.keys.Length == 0)
            SetDefaultProbabilityCurve(ref greenTileProbability, 0.25f);
        if (blueTileProbability.keys.Length == 0)
            SetDefaultProbabilityCurve(ref blueTileProbability, 0.25f);
        if (yellowTileProbability.keys.Length == 0)
            SetDefaultProbabilityCurve(ref yellowTileProbability, 0.25f);
            
        // Initialize score thresholds if not set
        if (scoreThresholds == null || scoreThresholds.Length == 0)
        {
            scoreThresholds = new int[maxLevel];
            for (int i = 0; i < maxLevel; i++)
            {
                scoreThresholds[i] = 1000 * (i + 1);
            }
        }
    }
    
    private void Start()
    {
        // Find references to other managers
        gameManager = FindObjectOfType<GameManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        
        // Use direct reference without explicit namespace
        tileManager = FindObjectOfType<TileManager>();
        
        // Initialize level settings
        ResetLevel();
    }
    
    private void Update()
    {
        // Remove automatic tile generation - we only want tiles to spawn after moves
        // if (isGeneratingTiles)
        // {
        //     tileGenerationTimer -= Time.deltaTime;
        //     
        //     if (tileGenerationTimer <= 0)
        //     {
        //         GenerateRandomTile();
        //         tileGenerationTimer = currentTileGenerationInterval;
        //     }
        // }
        
        // Only keep score-based level progression
        if (scoreManager != null)
        {
            int currentScore = scoreManager.GetCurrentScore();
            CheckForLevelUp(currentScore);
        }
    }
    
    #region Level Management
    
    public void StartLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, maxLevel);
        
        // Calculate level duration
        float timeMultiplier = Mathf.Pow(1 - levelTimeDecreasePercentage, currentLevel - 1);
        currentLevelDuration = baseLevelDuration * timeMultiplier;
        
        // Calculate tile generation interval
        float intervalMultiplier = Mathf.Pow(1 - tileGenerationDecreaseRate, currentLevel - 1);
        currentTileGenerationInterval = Mathf.Max(
            initialTileGenerationInterval * intervalMultiplier, 
            minimumTileGenerationInterval
        );
        
        // Notify the game manager about the level change
        if (gameManager != null)
        {
            GameUIManager.Instance?.UpdateLevel(currentLevel);
        }
        
        // Start tile generation
        StartTileGeneration();
    }
    
    public void ResetLevel()
    {
        currentLevel = startingLevel;
        StopTileGeneration();
        StartLevel(currentLevel);
    }
    
    public void AdvanceToNextLevel()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            StartLevel(currentLevel);
        }
        else
        {
            // Game completed
            if (gameManager != null)
            {
                gameManager.Win();
            }
        }
    }
    
    private void CheckForLevelUp(int currentScore)
    {
        if (currentLevel >= maxLevel) return;
        
        int nextLevelThreshold = scoreThresholds[currentLevel - 1];
        if (currentScore >= nextLevelThreshold)
        {
            AdvanceToNextLevel();
        }
    }
    
    #endregion
    
    #region Tile Generation
    
    public void StartTileGeneration()
    {
        // We're no longer using automatic tile generation
        isGeneratingTiles = true;
        // Don't need the timer anymore
        // tileGenerationTimer = currentTileGenerationInterval;
    }
    
    public void StopTileGeneration()
    {
        isGeneratingTiles = false;
    }
    
    // This method will now only be called after a tile is moved (via GameManager.OnTileMovementComplete)
    public void GenerateRandomTile()
    {
        if (tileManager == null) return;
        
        // Determine the color based on level probabilities
        GameTile.TileColor color = GetRandomColorBasedOnLevel();
        
        // Let the tile manager create a tile
        tileManager.GenerateRandomTile();
    }
    
    private GameTile.TileColor GetRandomColorBasedOnLevel()
    {
        // Normalize level to 0-1 range for curve evaluation
        float normalizedLevel = (float)(currentLevel - 1) / (float)(maxLevel - 1);
        
        // Get probabilities for each color at current level
        float redProb = redTileProbability.Evaluate(normalizedLevel);
        float greenProb = greenTileProbability.Evaluate(normalizedLevel);
        float blueProb = blueTileProbability.Evaluate(normalizedLevel);
        float yellowProb = yellowTileProbability.Evaluate(normalizedLevel);
        
        // Normalize probabilities
        float totalProb = redProb + greenProb + blueProb + yellowProb;
        redProb /= totalProb;
        greenProb /= totalProb;
        blueProb /= totalProb;
        yellowProb /= totalProb;
        
        // Random selection based on probabilities
        float random = Random.value;
        if (random < redProb)
            return GameTile.TileColor.Red;
        else if (random < redProb + greenProb)
            return GameTile.TileColor.Green;
        else if (random < redProb + greenProb + blueProb)
            return GameTile.TileColor.Blue;
        else
            return GameTile.TileColor.Yellow;
    }
    
    private void SetDefaultProbabilityCurve(ref AnimationCurve curve, float baseValue)
    {
        curve = new AnimationCurve(
            new Keyframe(0, baseValue),
            new Keyframe(0.5f, baseValue),
            new Keyframe(1, baseValue)
        );
    }
    
    #endregion
    
    #region Getters and Setters
    
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    public float GetCurrentLevelDuration()
    {
        return currentLevelDuration;
    }
    
    public float GetCurrentTileGenerationInterval()
    {
        return currentTileGenerationInterval;
    }
    
    #endregion
    
    #region Special Game Elements
    
    // This method would be called based on level or score thresholds
    public void IntroduceSpecialElement(int elementType)
    {
        switch (elementType)
        {
            case 0: // Example: Introduce obstacle tiles
                // tileManager.CreateObstacleTile();
                break;
            case 1: // Example: Introduce power-up tiles
                // tileManager.CreatePowerUpTile();
                break;
            case 2: // Example: Change board layout
                // boardManager.ChangeLayout(currentLevel);
                break;
            default:
                break;
        }
    }
    
    // Check for introducing special elements based on level
    private void CheckForSpecialElements()
    {
        switch (currentLevel)
        {
            case 3:
                IntroduceSpecialElement(0); // Introduce obstacles at level 3
                break;
            case 5:
                IntroduceSpecialElement(1); // Introduce power-ups at level 5
                break;
            case 7:
                IntroduceSpecialElement(2); // Change board layout at level 7
                break;
        }
    }
    
    #endregion
}
