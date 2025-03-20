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

    [Header("Obstacle Settings")]
    [SerializeField] private int minLevelForObstacles = 2; // Start introducing obstacles at level 2
    [SerializeField] private float baseObstacleProbability = 0.05f; // 5% chance at the minimum level
    [SerializeField] private float maxObstacleProbability = 0.3f; // Up to 30% chance at higher levels
    [SerializeField] private int maxObstaclesPerLevel = 3; // Maximum number of obstacles to introduce per level
    [SerializeField] private AnimationCurve obstacleDifficultyCurve; // How quickly obstacles should increase

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

    // Add tracking variables
    private int obstaclesInCurrentLevel = 0;
    
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

        // Initialize obstacle curve if not set
        if (obstacleDifficultyCurve.keys.Length == 0)
        {
            obstacleDifficultyCurve = new AnimationCurve(
                new Keyframe(0, 0),
                new Keyframe(0.3f, 0.2f),
                new Keyframe(0.7f, 0.6f),
                new Keyframe(1, 1)
            );
        }
            
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
        
        // Don't initialize level settings here - this will be called by GameManager
        // Comment out to prevent duplicate initialization
        // ResetLevel();
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
        
        // Reset obstacle counter for new level
        obstaclesInCurrentLevel = 0;
        
        // Generate initial tiles for the new level
        GenerateInitialTilesForLevel();
        
        // Introduce initial obstacles for this level
        IntroduceInitialObstacles();
        
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
            
            // Update UI with level up panel and continue button
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowLevelUpPanel(currentLevel);
                // NOTE: The actual level transition will happen when the Continue button is clicked
            }
            else
            {
                // If no UI manager, proceed directly
                StartNextLevelAfterContinue(currentLevel);
            }
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

    // New method to start the next level after Continue button is clicked
    public void StartNextLevelAfterContinue(int level)
    {
        Debug.Log($"Starting level {level} after continue button click");
        
        // Prevent the UI from triggering level-up again during this process
        GameUIManager uiManager = GameUIManager.Instance;
        if (uiManager != null)
        {
            uiManager.SuppressLevelUpPanel = true; // Add this flag to GameUIManager
        }
        
        // Clear the board
        ClearBoardForNextLevel();
        
        // Make sure we use the passed level value, not the current level
        // This fixes potential desync issues
        currentLevel = level;
        
        // Recalculate level duration and other settings
        float timeMultiplier = Mathf.Pow(1 - levelTimeDecreasePercentage, currentLevel - 1);
        currentLevelDuration = baseLevelDuration * timeMultiplier;
        
        // Recalculate tile generation interval
        float intervalMultiplier = Mathf.Pow(1 - tileGenerationDecreaseRate, currentLevel - 1);
        currentTileGenerationInterval = Mathf.Max(
            initialTileGenerationInterval * intervalMultiplier, 
            minimumTileGenerationInterval
        );
        
        // Update the game timer
        if (gameManager != null)
        {
            gameManager.SetLevelTimer(currentLevelDuration);
        }
        
        // Reset obstacle counter for new level
        obstaclesInCurrentLevel = 0;
        
        // Generate initial tiles for this level after a short delay
        StartCoroutine(DelayedTileGeneration());
    }

    private IEnumerator DelayedTileGeneration()
    {
        // Short delay to ensure UI has transitioned properly
        yield return new WaitForSeconds(0.5f);
        
        // Generate initial tiles for the new level
        GenerateInitialTilesForLevel();
        
        // Introduce initial obstacles for this level
        IntroduceInitialObstacles();
        
        // Start tile generation
        StartTileGeneration();
        
        // Update UI
        if (GameUIManager.Instance != null)
        {
            // Update level display but don't trigger the level up panel
            GameUIManager.Instance.UpdateLevelSilently(currentLevel);
            
            // Re-enable level-up panels for future level increases
            GameUIManager.Instance.SuppressLevelUpPanel = false;
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

    // New method to clear the board for next level
    private void ClearBoardForNextLevel()
    {
        // Clear existing tiles and obstacles
        if (tileManager != null)
        {
            tileManager.ClearBoard();
        }
    }

    // New method to generate initial tiles for each level
    private void GenerateInitialTilesForLevel()
    {
        // Clear any existing debug logs for cleaner output
        Debug.Log("Generating initial tiles for level " + currentLevel);
        
        // Add more initial tiles as the level increases
        int initialTiles = Mathf.Min(3 + currentLevel - 1, 6); // Start with 3, add 1 per level, cap at 6
        
        // Generate initial tiles
        if (tileManager != null)
        {
            // First make sure the board is clear
            tileManager.ClearBoard();
            
            for (int i = 0; i < initialTiles; i++)
            {
                // Add a small delay between tile generation for better visualization
                StartCoroutine(GenerateTileWithDelay(i * 0.2f));
            }
        }
    }

    private IEnumerator GenerateTileWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (tileManager != null)
        {
            tileManager.GenerateRandomTile();
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
    
    // This method will now have a chance to spawn an obstacle instead of a normal tile
    public void GenerateRandomTile()
    {
        if (tileManager == null) return;
        
        // Check if we should spawn an obstacle instead
        bool spawnObstacle = false;
        
        if (currentLevel >= minLevelForObstacles)
        {
            float obstacleChance = GetObstacleProbabilityForLevel();
            spawnObstacle = Random.value < obstacleChance && obstaclesInCurrentLevel < maxObstaclesPerLevel;
            
            if (spawnObstacle)
            {
                tileManager.GenerateRandomObstacleTile();
                obstaclesInCurrentLevel++;
                return;
            }
        }
        
        // Otherwise spawn a normal tile
        GameTile.TileColor color = GetRandomColorBasedOnLevel();
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
            case 0: // Introduce obstacle tiles
                if (tileManager != null && obstaclesInCurrentLevel < maxObstaclesPerLevel)
                {
                    tileManager.GenerateRandomObstacleTile();
                    obstaclesInCurrentLevel++;
                }
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

    private void IntroduceInitialObstacles()
    {
        // Only introduce obstacles at or above minLevelForObstacles
        if (currentLevel < minLevelForObstacles) return;
        
        // Calculate how many obstacles to introduce
        int levelBasedMax = Mathf.FloorToInt(maxObstaclesPerLevel * GetObstacleProbabilityForLevel());
        int obstaclesToSpawn = Random.Range(1, levelBasedMax + 1);
        
        Debug.Log($"Introducing {obstaclesToSpawn} obstacles at level {currentLevel}");
        
        // Spawn the obstacles
        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            if (tileManager != null)
            {
                tileManager.GenerateRandomObstacleTile();
                obstaclesInCurrentLevel++;
            }
        }
    }

    // Calculate obstacle probability based on current level
    private float GetObstacleProbabilityForLevel()
    {
        if (currentLevel < minLevelForObstacles) return 0f;
        
        // Normalize level to 0-1 range for curve evaluation
        float normalizedLevel = Mathf.Clamp01((float)(currentLevel - minLevelForObstacles) / (float)(maxLevel - minLevelForObstacles));
        
        // Evaluate probability using the curve
        float probability = baseObstacleProbability + (maxObstacleProbability - baseObstacleProbability) * obstacleDifficultyCurve.Evaluate(normalizedLevel);
        
        return probability;
    }
    
    #endregion
}
