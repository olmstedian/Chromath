/// <summary>
/// Interface for game states to implement.
/// Each state handles its own logic and transitions.
/// </summary>
public interface IGameState
{
    // Called when entering this state
    void EnterState();
    
    // Called when exiting this state
    void ExitState();
    
    // Called each frame during this state
    void UpdateState();
    
    // Handle input in this state
    void HandleInput();
}
