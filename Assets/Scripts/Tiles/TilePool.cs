// Object Pooling for Tiles
using System.Collections.Generic;
using UnityEngine;

public class TilePool : MonoBehaviour {
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int initialPoolSize = 50;
    private List<GameObject> pooledTiles = new List<GameObject>();
    
    void Start() {
        // Pre-instantiate tiles
        for (int i = 0; i < initialPoolSize; i++) {
            GameObject tile = Instantiate(tilePrefab);
            tile.SetActive(false);
            pooledTiles.Add(tile);
        }
    }
    
    public GameObject GetTile() {
        // Find inactive tile in pool
        foreach (GameObject tile in pooledTiles) {
            if (!tile.activeInHierarchy) {
                tile.SetActive(true);
                return tile;
            }
        }
        
        // If no inactive tiles, create new one
        GameObject newTile = Instantiate(tilePrefab);
        pooledTiles.Add(newTile);
        return newTile;
    }
    
    public void ReturnTile(GameObject tile) {
        tile.SetActive(false);
    }
}