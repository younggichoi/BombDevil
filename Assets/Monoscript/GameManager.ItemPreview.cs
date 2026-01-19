using UnityEngine;
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
        float cellSize = BoardManager.GetCellSize();
        Vector3 worldPos = BoardManager.GridToWorld(x, y);
        if (_ghostItem == null)
        {
            _ghostItem = new GameObject("GhostItem");
            SpriteRenderer sr = _ghostItem.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
        }
        _ghostItem.transform.position = worldPos;
        _ghostItem.transform.localScale = Vector3.one * cellSize;
        SpriteRenderer ghostSr = _ghostItem.GetComponent<SpriteRenderer>();
        if (ghostSr != null)
        {
            Color itemColor = Color.white;
            if (itemData.fieldSprite != null)
            {
                ghostSr.sprite = itemData.fieldSprite;
                Vector2 spriteSize = ghostSr.sprite.bounds.size;
                float scaleX = cellSize / spriteSize.x;
                float scaleY = cellSize / spriteSize.y;
                float scale = Mathf.Min(scaleX, scaleY);  // Keep aspect ratio, fit within cell
                _ghostItem.transform.localScale = Vector3.one * scale;
            }
            else
            {
                ghostSr.sprite = CreateSquareSprite();
                itemColor = itemData.GetColor();
            }   
            itemColor.a = 0.5f;
            ghostSr.color = itemColor;
        }
        _ghostItem.SetActive(true);
        int range = itemData.range;
        ShowItemRangeIndicators(x, y, range);
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
        float cellSize = BoardManager.GetCellSize();
        int indicatorIndex = 0;
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                int targetX = Mod(centerX + dx, _width);
                int targetY = Mod(centerY + dy, _height);
                Vector3 worldPos = BoardManager.GridToWorld(targetX, targetY);
                while (indicatorIndex >= _itemRangeIndicators.Count)
                {
                    GameObject indicator = new GameObject($"ItemRangeIndicator_{_itemRangeIndicators.Count}");
                    SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
                    sr.sprite = CreateSquareSprite();
                    sr.sortingOrder = 4;
                    _itemRangeIndicators.Add(indicator);
                }
                GameObject rangeObj = _itemRangeIndicators[indicatorIndex];
                rangeObj.transform.position = worldPos;
                rangeObj.transform.localScale = Vector3.one * cellSize;
                SpriteRenderer rangeSr = rangeObj.GetComponent<SpriteRenderer>();
                if (rangeSr != null)
                {
                    rangeSr.color = new Color(0.3f, 0.7f, 1f, 0.4f);
                }
                rangeObj.SetActive(true);
                indicatorIndex++;
            }
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
