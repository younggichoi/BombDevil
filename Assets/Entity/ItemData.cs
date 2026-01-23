using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public int range;
    public Color color;
    public string fieldSpriteName; // Name of the sprite in Resources/Item/
    public string iconSpriteName; // Name of the sprite in Resources/Item/
    public Sprite fieldSprite;
    public Sprite iconSprite; 

    public Color GetColor()
    {
        return color;
    }
}
