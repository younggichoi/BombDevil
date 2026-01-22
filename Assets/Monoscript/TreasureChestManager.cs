using UnityEngine;
using System.Collections.Generic;

public class TreasureChestManager : MonoBehaviour
{
    private static Transform _treasureChestSet;
    private static Sprite _treasureChestSprite;
    private static GameObject _treasureChestPrefab;

    public void Initialize(Transform treasureChestSet, Sprite treasureChestSprite, GameObject treasureChestPrefab)
    {
        _treasureChestSet = treasureChestSet;
        _treasureChestSprite = treasureChestSprite;
        _treasureChestPrefab = treasureChestPrefab;
    }

    public static GameObject CreateTreasureChest(int x, int y, int durability, int value)
    {
        var boardManager = GameService.Get<BoardManager>();
        Vector3 worldPos = boardManager.GridToWorld(x, y);
        GameObject treasureChestObj = Instantiate(_treasureChestPrefab, worldPos, Quaternion.identity, _treasureChestSet);
        TreasureChest treasureChest = treasureChestObj.GetComponent<TreasureChest>();
        treasureChest.Initialize(durability, value);   
        
        SpriteRenderer sr = treasureChestObj.GetComponent<SpriteRenderer>();
        sr.sprite = _treasureChestSprite;

        // Scale sprite to fit exactly one cell
        float cellSize = boardManager.GetCellSize();
        Vector2 spriteSize = sr.sprite.bounds.size;
        float scaleX = cellSize / spriteSize.x;
        float scaleY = cellSize / spriteSize.y;
        float scale = Mathf.Min(scaleX, scaleY);  // Keep aspect ratio, fit within cell
        treasureChestObj.transform.localScale = Vector3.one * scale;
        
        return treasureChestObj;
    }
}