using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Entity;

public partial class GameManager : MonoBehaviour
{
    private GameObject _ghostItem;
    private List<GameObject> _itemRangeIndicators;
    private Vector2Int _lastHoveredItemCell = new Vector2Int(-1, -1);

    private void UpdateItemPreview()
    {
        if (_isTurnInProgress)
        {
            HideItemPreview();
            return;
        }
        if (!ItemManager.HasItemSelected())
        {
            HideItemPreview();
            return;
        }
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        int x = GlobalToGridX(worldPos.x);
        int y = GlobalToGridY(worldPos.y);
        if (x >= 0 && x < _width && y >= 0 && y < _height && _board[x, y].Count == 0)
        {
            Vector2Int currentCell = new Vector2Int(x, y);
            if (currentCell != _lastHoveredItemCell)
            {
                ShowItemPreview(x, y);
                _lastHoveredItemCell = currentCell;
            }
        }
        else
        {
            HideItemPreview();
            _lastHoveredItemCell = new Vector2Int(-1, -1);
        }
    }

    private void ShowItemPreview(int x, int y)
    {
        ItemType? itemType = ItemManager.GetCurrentItemType();
        if (!itemType.HasValue) return;
        ItemData itemData = ItemManager.GetItemData(itemType.Value);
        if (itemData == null) return;
        
        // Canvas UI mode only
        Vector2 canvasPos = BoardManager.GridToCanvasPosition(x, y);
        float cellSizeCanvas = BoardManager.GetCellSizeCanvas();
        
        if (_ghostItem == null)
        {
            _ghostItem = new GameObject("GhostItem");
            _ghostItem.transform.SetParent(BoardManager.GetParentCanvas(), false);
            _ghostItem.AddComponent<RectTransform>();
            _ghostItem.AddComponent<Image>();
        }
        
        RectTransform rect = _ghostItem.GetComponent<RectTransform>();
        rect.anchoredPosition = canvasPos;
        rect.sizeDelta = new Vector2(cellSizeCanvas, cellSizeCanvas);
        
        Image image = _ghostItem.GetComponent<Image>();
        Color itemColor = Color.white;
        if (itemData.fieldSprite != null)
        {
            image.sprite = itemData.fieldSprite;
        }
        else
        {
            image.sprite = CreateSquareSprite();
            itemColor = itemData.GetColor();
        }   
        itemColor.a = 0.5f;
        image.color = itemColor;
        image.raycastTarget = false;
        
        _ghostItem.SetActive(true);
        
        if (itemType == ItemType.Megaphone)
        {
            ShowItemCrossRangeIndicators(x, y);
        }
        else
        {
            // other preview algorithm
        }
    }

    private void ShowItemRangeIndicators(int centerX, int centerY, int range)
    {
        if (_itemRangeIndicators == null)
            _itemRangeIndicators = new List<GameObject>();
        foreach (var indicator in _itemRangeIndicators)
        {
            if (indicator != null)
                indicator.SetActive(false);
        }
        
        int indicatorIndex = 0;
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                int targetX = Mod(centerX + dx, _width);
                int targetY = Mod(centerY + dy, _height);
                
                while (indicatorIndex >= _itemRangeIndicators.Count)
                {
                    GameObject indicator = CreateRangeIndicator($"ItemRangeIndicator_{_itemRangeIndicators.Count}", 4);
                    _itemRangeIndicators.Add(indicator);
                }
                GameObject rangeObj = _itemRangeIndicators[indicatorIndex];
                PositionRangeIndicator(rangeObj, targetX, targetY, new Color(0.3f, 0.7f, 1f, 0.4f));
                rangeObj.SetActive(true);
                indicatorIndex++;
            }
        }
    }

    // Show cross-shaped range indicators for items (e.g., Megaphone)
    private void ShowItemCrossRangeIndicators(int centerX, int centerY)
    {
        if (_itemRangeIndicators == null)
            _itemRangeIndicators = new List<GameObject>();
        foreach (var indicator in _itemRangeIndicators)
        {
            if (indicator != null)
                indicator.SetActive(false);
        }
        
        int indicatorIndex = 0;
        
        // Horizontal line (same Y as item, all X)
        for (int dx = 0; dx < _width; dx++)
        {
            if (dx == centerX) continue;  // Skip item position
            
            while (indicatorIndex >= _itemRangeIndicators.Count)
            {
                GameObject indicator = CreateRangeIndicator($"ItemRangeIndicator_{_itemRangeIndicators.Count}", 4);
                _itemRangeIndicators.Add(indicator);
            }
            GameObject rangeObj = _itemRangeIndicators[indicatorIndex];
            PositionRangeIndicator(rangeObj, dx, centerY, new Color(0.3f, 0.7f, 1f, 0.4f));
            rangeObj.SetActive(true);
            indicatorIndex++;
        }
        
        // Vertical line (same X as item, all Y)
        for (int dy = 0; dy < _height; dy++)
        {
            if (dy == centerY) continue;  // Skip item position
            
            while (indicatorIndex >= _itemRangeIndicators.Count)
            {
                GameObject indicator = CreateRangeIndicator($"ItemRangeIndicator_{_itemRangeIndicators.Count}", 4);
                _itemRangeIndicators.Add(indicator);
            }
            GameObject rangeObj = _itemRangeIndicators[indicatorIndex];
            PositionRangeIndicator(rangeObj, centerX, dy, new Color(0.3f, 0.7f, 1f, 0.4f));
            rangeObj.SetActive(true);
            indicatorIndex++;
        }
    }

    private void HideItemPreview()
    {
        if (_ghostItem != null)
            _ghostItem.SetActive(false);
        if (_itemRangeIndicators != null)
        {
            foreach (var indicator in _itemRangeIndicators)
            {
                if (indicator != null)
                    indicator.SetActive(false);
            }
        }
    }
}
