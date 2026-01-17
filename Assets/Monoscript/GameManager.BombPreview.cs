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
        if (!BombManager.HasBombSelected())
        {
            HidePreview();
            return;
        }
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        int x = GlobalToGridX(worldPos.x);
        int y = GlobalToGridY(worldPos.y);
        if (x >= 0 && x < _width && y >= 0 && y < _height && !HasBombAt(x, y))
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
        BombType? bombType = BombManager.GetCurrentBombType();
        if (!bombType.HasValue) return;
        BombData bombData = BombManager.GetBombData(bombType.Value);
        if (bombData == null) return;
        float cellSize = BoardManager.GetCellSize();
        Vector3 worldPos = BoardManager.GridToWorld(x, y);
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
        
        // SkyblueBomb shows cross-shaped range, others show square range
        if (bombType.Value == BombType.SkyblueBomb)
        {
            ShowCrossRangeIndicators(x, y);
        }
        else
        {
            int range = bombData.range;
            ShowRangeIndicators(x, y, range);
        }
        
        // Show predicted enemy positions after knockback
        ShowEnemyPredictions(x, y, bombType.Value, bombData);
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
    
    // Show cross-shaped range indicators for SkyblueBomb
    private void ShowCrossRangeIndicators(int centerX, int centerY)
    {
        if (_rangeIndicators == null)
            _rangeIndicators = new List<GameObject>();
        foreach (var indicator in _rangeIndicators)
        {
            if (indicator != null)
                indicator.SetActive(false);
        }
        float cellSize = BoardManager.GetCellSize();
        int indicatorIndex = 0;
        
        // Horizontal line (same Y as bomb, all X)
        for (int dx = 0; dx < _width; dx++)
        {
            if (dx == centerX) continue;  // Skip bomb position
            
            Vector3 worldPos = BoardManager.GridToWorld(dx, centerY);
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
                rangeSr.color = new Color(0.3f, 0.8f, 1f, 0.4f);  // Skyblue color
            }
            rangeObj.SetActive(true);
            indicatorIndex++;
        }
        
        // Vertical line (same X as bomb, all Y)
        for (int dy = 0; dy < _height; dy++)
        {
            if (dy == centerY) continue;  // Skip bomb position
            
            Vector3 worldPos = BoardManager.GridToWorld(centerX, dy);
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
                rangeSr.color = new Color(0.3f, 0.8f, 1f, 0.4f);  // Skyblue color
            }
            rangeObj.SetActive(true);
            indicatorIndex++;
        }
    }
    
    // Show predicted enemy positions after knockback
    private void ShowEnemyPredictions(int bombX, int bombY, BombType bombType, BombData bombData)
    {
        if (_enemyPredictionIndicators == null)
            _enemyPredictionIndicators = new List<GameObject>();
        foreach (var indicator in _enemyPredictionIndicators)
        {
            if (indicator != null)
                indicator.SetActive(false);
        }
        
        // RealBomb kills enemies, no knockback prediction needed
        if (bombType == BombType.RealBomb) return;
        
        float cellSize = BoardManager.GetCellSize();
        int indicatorIndex = 0;
        int range = bombData.range;
        int knockbackDistance = bombData.knockbackDistance;
        
        // Find all enemies and calculate their predicted positions
        for (int ex = 0; ex < _width; ex++)
        {
            for (int ey = 0; ey < _height; ey++)
            {
                foreach (var obj in _board[ex, ey])
                {
                    if (obj == null) continue;
                    Enemy enemy = obj.GetComponent<Enemy>();
                    if (enemy == null) continue;
                    
                    Vector2Int? predictedPos = null;
                    
                    if (bombType == BombType.SkyblueBomb)
                    {
                        // SkyblueBomb: enemies OFF axes move away from nearest axis
                        int dx = GetWrappedDistance(ex, bombX, _width);
                        int dy = GetWrappedDistance(ey, bombY, _height);
                        
                        // Skip if on axis
                        if (dx == 0 || dy == 0) continue;
                        
                        Vector2Int knockbackDir = Vector2Int.zero;
                        if (Mathf.Abs(dx) == 1)
                        {
                            knockbackDir += new Vector2Int(dx, 0);
                        }
                        if (Mathf.Abs(dy) == 1)
                        {
                            knockbackDir += new Vector2Int(0, dy);
                        }
                        predictedPos = new Vector2Int(
                            Mod(ex + knockbackDir.x, _width),
                            Mod(ey + knockbackDir.y, _height)
                        );
                    }
                    else
                    {
                        // Normal bombs (1st-6th): enemies in range knocked away from bomb
                        int dist_x = GetWrappedDistance(bombX, ex, _width);
                        int dist_y = GetWrappedDistance(bombY, ey, _height);

                        // Check if in range (with wrapping)
                        if (Mathf.Abs(dist_x) <= range && Mathf.Abs(dist_y) <= range && !(dist_x == 0 && dist_y == 0))
                        {
                            // The direction from the bomb to the enemy
                            Vector2Int dir = new Vector2Int(
                                dist_x == 0 ? 0 : (dist_x > 0 ? 1 : -1),
                                dist_y == 0 ? 0 : (dist_y > 0 ? 1 : -1)
                            );
                            
                            // Knockback is applied in the same direction
                            Vector2Int knockback = new Vector2Int(
                                dir.x * knockbackDistance,
                                dir.y * knockbackDistance
                            );
                            
                            predictedPos = new Vector2Int(
                                Mod(ex - knockback.x, _width),
                                Mod(ey - knockback.y, _height)
                            );
                        }
                    }
                    
                    // Show prediction indicator if we have a predicted position
                    if (predictedPos.HasValue)
                    {
                        Vector3 worldPos = BoardManager.GridToWorld(predictedPos.Value.x, predictedPos.Value.y);
                        while (indicatorIndex >= _enemyPredictionIndicators.Count)
                        {
                            GameObject indicator = new GameObject($"EnemyPrediction_{_enemyPredictionIndicators.Count}");
                            SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
                            sr.sprite = CreateSquareSprite();
                            sr.sortingOrder = 6;  // Above range indicators
                            _enemyPredictionIndicators.Add(indicator);
                        }
                        GameObject predObj = _enemyPredictionIndicators[indicatorIndex];
                        predObj.transform.position = worldPos;
                        predObj.transform.localScale = Vector3.one * cellSize * 0.6f;  // Smaller than cell
                        SpriteRenderer predSr = predObj.GetComponent<SpriteRenderer>();
                        if (predSr != null)
                        {
                            predSr.color = new Color(0.2f, 1f, 0.2f, 0.6f);  // Green for predicted
                        }
                        predObj.SetActive(true);
                        indicatorIndex++;
                    }
                }
            }
        }
    }

    private void HidePreview()
    {
        if (_ghostBomb != null)
            Destroy(_ghostBomb);
        if (_rangeIndicators != null)
        {
            foreach (var indicator in _rangeIndicators)
            {
                if (indicator != null)
                    Destroy(indicator);
            }
            _rangeIndicators.Clear();
        }
        if (_enemyPredictionIndicators != null)
        {
            foreach (var indicator in _enemyPredictionIndicators)
            {
                if (indicator != null)
                    Destroy(indicator);
            }
            _enemyPredictionIndicators.Clear();
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
    
    // Remove mode preview - shows X marker on bombs
    private void UpdateRemovePreview()
    {
        if (_isTurnInProgress)
        {
            HideRemovePreview();
            return;
        }
        
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        int x = GlobalToGridX(worldPos.x);
        int y = GlobalToGridY(worldPos.y);
        
        // Show X marker if hovering over a bomb
        if (x >= 0 && x < _width && y >= 0 && y < _height && HasBombAt(x, y))
        {
            ShowRemoveIndicator(x, y);
        }
        else
        {
            HideRemovePreview();
        }
    }
    
    private void ShowRemoveIndicator(int x, int y)
    {
        float cellSize = BoardManager.GetCellSize();
        Vector3 worldPos = BoardManager.GridToWorld(x, y);
        
        if (_removeIndicator == null)
        {
            _removeIndicator = new GameObject("RemoveIndicator");
            SpriteRenderer sr = _removeIndicator.AddComponent<SpriteRenderer>();
            sr.sprite = CreateXSprite();
            sr.sortingOrder = 10;  // On top of everything
        }
        
        _removeIndicator.transform.position = worldPos;
        _removeIndicator.transform.localScale = Vector3.one * cellSize * 0.8f;
        
        SpriteRenderer xSr = _removeIndicator.GetComponent<SpriteRenderer>();
        if (xSr != null)
        {
            xSr.color = new Color(1f, 0.2f, 0.2f, 0.9f);  // Red X
        }
        _removeIndicator.SetActive(true);
    }
    
    private void HideRemovePreview()
    {
        if (_removeIndicator != null)
            _removeIndicator.SetActive(false);
    }
    
    // Create X-shaped sprite for remove indicator
    private Sprite CreateXSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        
        // Initialize with transparent
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.clear;
        
        // Draw X shape (two diagonal lines)
        int thickness = 4;
        for (int i = 0; i < size; i++)
        {
            for (int t = -thickness/2; t <= thickness/2; t++)
            {
                // Main diagonal (\)
                int idx1 = i * size + (i + t);
                if (i + t >= 0 && i + t < size && idx1 >= 0 && idx1 < colors.Length)
                    colors[idx1] = Color.white;
                
                // Anti-diagonal (/)
                int idx2 = i * size + (size - 1 - i + t);
                if (size - 1 - i + t >= 0 && size - 1 - i + t < size && idx2 >= 0 && idx2 < colors.Length)
                    colors[idx2] = Color.white;
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}

