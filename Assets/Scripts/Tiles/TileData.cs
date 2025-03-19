using UnityEngine;

[System.Serializable]
public class TileData
{
    public int value;
    public TileColor color;
    public bool isSpecial;
    public SpecialTileType specialType;

    public TileData(int value, TileColor color)
    {
        this.value = value;
        this.color = color;
        this.isSpecial = false;
        this.specialType = SpecialTileType.None;
    }

    public void MergeWith(TileData other)
    {
        this.value += other.value;
        
        // Check if this merge creates a special tile
        CheckForSpecialTileCreation();
    }

    private void CheckForSpecialTileCreation()
    {
        if (value >= 64)
        {
            isSpecial = true;
            specialType = (SpecialTileType)Random.Range(4, 6); // Level 3 special tiles
        }
        else if (value >= 32)
        {
            isSpecial = true;
            specialType = (SpecialTileType)Random.Range(2, 4); // Level 2 special tiles
        }
        else if (value >= 16)
        {
            isSpecial = true;
            specialType = (SpecialTileType)Random.Range(0, 2); // Level 1 special tiles
        }
    }
}

public enum TileColor
{
    Red, 
    Blue, 
    Green, 
    Yellow, 
    Purple
}

public enum SpecialTileType
{
    None,
    Wildcard,
    Jumper,
    Converter,
    Multiplier,
    Bomb,
    Rainbow
}

public enum Direction
{
    Up,
    Right,
    Down,
    Left
}