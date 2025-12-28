using UnityEngine;
using System.Collections.Generic;
using Entity;

public partial class GameManager : MonoBehaviour
{
    private void UpdateBombPreview()
    {
        if (_isTurnInProgress)
        {
            HidePreview();
            return;
        }
        if (!bombManager.HasBombSelected())
        {
            HidePreview();
            return;
        }
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        int x = GlobalToGridX(worldPos.x);
        int y = GlobalToGridY(worldPos.y);
        if (x >= 0 && x < _width && y >= 0 && y < _height && _board[x, y].Count == 0)
        {
            Vector2Int currentCell = new Vector2Int(x, y);
            if (currentCell != _lastHoveredCell)
            {
                ShowPreview(x, y);
                _lastHoveredCell = currentCell;
            }
        }
        else
        {
            HidePreview();
            _lastHoveredCell = new Vector2Int(-1, -1);
        }
    }

    private void ShowPreview(int x, int y)
    {
        BombType? bombType = bombManager.GetCurrentBombType();
        if (!bombType.HasValue) return;
        BombData bombData = bombManager.GetBombData(bombType.Value);
        if (bombData == null) return;
        float cellSize = boardManager.GetCellSize();
        Vector3 worldPos = boardManager.GridToWorld(x, y);
        if (_ghostBomb == null)
        {
            _ghostBomb = new GameObject("GhostBomb");
            SpriteRenderer sr = _ghostBomb.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
        }
        _ghostBomb.transform.position = worldPos;
        _ghostBomb.transform.localScale = Vector3.one * cellSize;
        SpriteRenderer ghostSr = _ghostBomb.GetComponent<SpriteRenderer>();
        if (ghostSr != null)
        {
            if (_defaultBombSprite != null)
            {
                ghostSr.sprite = _defaultBombSprite;
            }
            else
            {
                ghostSr.sprite = CreateSquareSprite();
            }
            Color bombColor = bombData.GetColor();
            bombColor.a = 0.5f;
            ghostSr.color = bombColor;
        }
        _ghostBomb.SetActive(true);
        int range = bombData.range;
        ShowRangeIndicators(x, y, range);
    }

    private void ShowRangeIndicators(int centerX, int centerY, int range)
    {
        if (_rangeIndicators == null)
            _rangeIndicators = new List<GameObject>();
        foreach (var indicator in _rangeIndicators)
        {
            if (indicator != null)
                indicator.SetActive(false);
        }
        float cellSize = boardManager.GetCellSize();
        int indicatorIndex = 0;
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                int targetX = Mod(centerX + dx, _width);
                int targetY = Mod(centerY + dy, _height);
                Vector3 worldPos = boardManager.GridToWorld(targetX, targetY);
                while (indicatorIndex >= _rangeIndicators.Count)
                {
                    GameObject indicator = new GameObject($"RangeIndicator_{_rangeIndicators.Count}");
                    SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
                    sr.sprite = CreateSquareSprite();
                    sr.sortingOrder = 4;
                    _rangeIndicators.Add(indicator);
                }
                GameObject rangeObj = _rangeIndicators[indicatorIndex];
                rangeObj.transform.position = worldPos;
                rangeObj.transform.localScale = Vector3.one * cellSize;
                SpriteRenderer rangeSr = rangeObj.GetComponent<SpriteRenderer>();
                if (rangeSr != null)
                {
                    rangeSr.color = new Color(1f, 0.3f, 0.3f, 0.4f);
                }
                rangeObj.SetActive(true);
                indicatorIndex++;
            }
        }
    }

    private void HidePreview()
    {
        if (_ghostBomb != null)
            _ghostBomb.SetActive(false);
        if (_rangeIndicators != null)
        {
            foreach (var indicator in _rangeIndicators)
            {
                if (indicator != null)
                    indicator.SetActive(false);
            }
        }
    }

    private Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.white;
        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
    }
}
