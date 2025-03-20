using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StateManager handles game state transitions and management.
/// It works with GameState implementations to control game flow.
/// </summary>
public class StateManager : MonoBehaviour
{
    // Game state enum (can be public for easy reference)
    public enum GameStateType { MainMenu, Playing, Paused, Win, GameOver, LevelTransition }
    
    // Current state
    private IGameState currentState;
    private GameStateType currentStateType;
    
    // Dictionary of available states
    private Dictionary<GameStateType, IGameState> states = new Dictionary<GameStateType, IGameState>();
    
    // References to other managers
    [Header("Manager References")]
    [SerializeField] private GameUIManager uiManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private TileManager tileManager;
    
    // Singleton pattern
    public static StateManager Instance { get; private set; }
    
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
        
        // Find references if not assigned
        if (uiManager == null) uiManager = FindObjectOfType<GameUIManager>();
        if (scoreManager == null) scoreManager = FindObjectOfType<ScoreManager>();
        if (levelManager == null) levelManager = FindObjectOfType<LevelManager>();
        if (tileManager == null) tileManager = FindObjectOfType<TileManager>();
    }
    
    private void Start()
    {
        // Initialize all available states
        InitializeStates();
        
        // DEVELOPMENT MODE: Start directly in Playing state
        // Change this to MainMenu for release builds
        ChangeState(GameStateType.Playing);
        
        Debug.Log("StateManager initialized - Starting directly in Playing state for development");
    }
    
    private void Update()
    {
        // Update the current state
        if (currentState != null)
        {
            currentState.UpdateState();
        }
    }
    
    private void InitializeStates()
    {
        // Create and register all game states
        states.Add(GameStateType.MainMenu, new MainMenuState(this));
        states.Add(GameStateType.Playing, new PlayingState(this));
        states.Add(GameStateType.Paused, new PausedState(this));
        states.Add(GameStateType.Win, new WinState(this));
        states.Add(GameStateType.GameOver, new GameOverState(this));
        states.Add(GameStateType.LevelTransition, new LevelTransitionState(this));
    }
    
    // Change to a new state
    public void ChangeState(GameStateType newStateType)
    {
        // Exit current state
        if (currentState != null)
        {
            currentState.ExitState();
        }
        
        // Get the new state
        if (!states.TryGetValue(newStateType, out IGameState newState))
        {
            Debug.LogError($"State {newStateType} not found!");
            return;
        }
        
        // Set current state
        currentState = newState;
        currentStateType = newStateType;
        
        // Enter new state
        currentState.EnterState();
        
        Debug.Log($"Game state changed to {newStateType}");
    }
    
    // Get a specific state by type
    public IGameState GetState(GameStateType stateType)
    {
        if (states.TryGetValue(stateType, out IGameState state))
        {
            return state;
        }
        
        Debug.LogWarning($"State {stateType} not found!");
        return null;
    }
    
    // Get current state type
    public GameStateType GetCurrentStateType()
    {
        return currentStateType;
    }
    
    // Check if game is in an active playable state
    public bool IsGameActive()
    {
        return currentStateType == GameStateType.Playing;
    }
    
    // Accessor methods for managers
    public GameUIManager GetUIManager() => uiManager;
    public ScoreManager GetScoreManager() => scoreManager;
    public LevelManager GetLevelManager() => levelManager;
    public TileManager GetTileManager() => tileManager;
}
