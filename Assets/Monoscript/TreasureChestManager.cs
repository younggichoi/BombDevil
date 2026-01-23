using UnityEngine;
using UnityEngine.UI;
using Entity;

public class TreasureChestManager : MonoBehaviour
{
    private Transform treasureChestSet;
    private Sprite treasureChestSprite;
    private GameObject treasureChestPrefab;
    
    public void Initialize(Transform treasureChestSet, Sprite treasureChestSprite, GameObject treasureChestPrefab)
    {
        this.treasureChestSet = treasureChestSet;
        this.treasureChestSprite = treasureChestSprite;
        this.treasureChestPrefab = treasureChestPrefab;
    }
    
    public GameObject CreateTreasureChest(int x, int y, int durability, int value)
    {
        var boardManager = GameService.Get<BoardManager>();
        
        // Canvas UI mode only
        Vector2 canvasPos = boardManager.GridToCanvasPosition(x, y);
        GameObject treasureChestObj = Instantiate(treasureChestPrefab, treasureChestSet);
        
        // Setup RectTransform
        RectTransform rectTransform = treasureChestObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = treasureChestObj.AddComponent<RectTransform>();
        }
        rectTransform.anchoredPosition = canvasPos;
        
        // Set size based on cell size (converted to Canvas pixels)
        float cellSizeCanvas = boardManager.GetCellSizeCanvas();
        rectTransform.sizeDelta = new Vector2(cellSizeCanvas, cellSizeCanvas);
        
        // Setup Image component for UI rendering
        Image image = treasureChestObj.GetComponent<Image>();
        if (image == null)
        {
            image = treasureChestObj.AddComponent<Image>();
        }
        image.sprite = treasureChestSprite;
        image.raycastTarget = false;
        
        // Remove SpriteRenderer if exists
        SpriteRenderer sr = treasureChestObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Destroy(sr);
        }
        
        // Initialize TreasureChest component
        TreasureChest treasureChest = treasureChestObj.GetComponent<TreasureChest>();
        treasureChest.Initialize(durability, value);
        
        return treasureChestObj;
    }

    public void ClearTreasureChests()
    {
        if (treasureChestSet != null)
        {
            foreach (Transform chest in treasureChestSet)
            {
                Destroy(chest.gameObject);
            }
        }
    }
}