using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public int range;
    public Color color;
    public string spriteName; // Name of the sprite in Resources/ItemSprites/

    public Color GetColor()
    {
        return color;
    }
}
