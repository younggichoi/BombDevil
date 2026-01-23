using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Direction
{
    Up,
    Down,
    Right,
    Left,
    UpRight,
    UpLeft,
    DownRight,
    DownLeft
}

public class EnemyManager : MonoBehaviour
{
    // Set via Initialize
    private GameObject enemy;
    private Transform enemySet;
    
    // BoardManager and GameManager references
    private BoardManager _boardManager;
    private Color _enemyColor;
    
    private GameObject _enemyPrefab;
    private Sprite _enemySprite;
    private Sprite _stunnedEnemySprite;

    public void Initialize(GameObject enemy, Transform enemySet, Sprite enemySprite, Sprite stunnedEnemySprite)
    {
        this.enemy = enemy;
        this.enemySet = enemySet;
        _boardManager = GameService.Get<BoardManager>();
        var gameManager = GameService.Get<GameManager>();
        _enemyColor = gameManager.GetEnemyColor();
        _enemySprite = enemySprite;
        _stunnedEnemySprite = stunnedEnemySprite;
    }

    public void ClearEnemies()
    {
        if (enemySet != null)
        {
            foreach (Transform child in enemySet)
            {
                Destroy(child.gameObject);
            }
        }
    }

    // create enemy API (call from GameManager)
    public GameObject CreateEnemy(int x, int y)
    {
        // Canvas UI mode only
        Vector2 canvasPos = _boardManager.GridToCanvasPosition(x, y);
        GameObject enemyObj = Instantiate(enemy, enemySet);
        
        // Setup RectTransform
        RectTransform rectTransform = enemyObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = enemyObj.AddComponent<RectTransform>();
        }
        rectTransform.anchoredPosition = canvasPos;
        
        // Set size based on cell size (converted to Canvas pixels)
        float cellSizeCanvas = _boardManager.GetCellSizeCanvas();
        rectTransform.sizeDelta = new Vector2(cellSizeCanvas, cellSizeCanvas);
        
        // Setup Image component for UI rendering
        Image image = enemyObj.GetComponent<Image>();
        if (image == null)
        {
            image = enemyObj.AddComponent<Image>();
        }
        image.sprite = _enemySprite;
        image.color = _enemyColor;
        image.raycastTarget = false;
        
        // Remove SpriteRenderer if exists (not needed for UI)
        SpriteRenderer sr = enemyObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Destroy(sr);
        }
        
        // Initialize enemy component
        Enemy enemyComponent = enemyObj.GetComponent<Enemy>();
        enemyComponent.Initialize(_enemySprite, _stunnedEnemySprite);
        enemyComponent.SetRandomDirection();
        
        return enemyObj;
    }

    public void SetEnemyPrefab(GameObject enemyPrefab)
    {
        _enemyPrefab = enemyPrefab;
    }

    public void SetEnemySprite(Sprite enemySprite)
    {
        _enemySprite = enemySprite;
    }
}
