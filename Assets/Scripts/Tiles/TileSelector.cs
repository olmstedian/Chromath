using UnityEngine;

public class TileSelector : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private GameObject selectionIndicator;
    
    private Tile currentlySelectedTile;
    
    private void Start()
    {
        // Subscribe to tile selection events
        boardManager.OnTileSelected += OnTileSelected;
        
        // Hide selection indicator initially
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }
    
    private void OnTileSelected(Tile tile)
    {
        currentlySelectedTile = tile;
        
        if (selectionIndicator != null && tile != null)
        {
            // Position the indicator at the selected tile
            selectionIndicator.transform.position = tile.transform.position;
            selectionIndicator.SetActive(true);
        }
    }
    
    // Update to handle deselection
    public void DeselectTile()
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
        
        currentlySelectedTile = null;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        boardManager.OnTileSelected -= OnTileSelected;
    }
}