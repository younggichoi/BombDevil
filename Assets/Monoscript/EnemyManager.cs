using System.Collections.Generic;
using UnityEngine;

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
        Vector3 worldPos = _boardManager.GridToWorld(x, y);
        GameObject enemyObj = Instantiate(enemy, worldPos, Quaternion.identity, enemySet);
        
        SpriteRenderer sr = enemyObj.GetComponent<SpriteRenderer>();
        sr.color = _enemyColor;
        
        // Set sprite if provided
        if (_enemySprite != null)
        {
            sr.sprite = _enemySprite;
        }
        
        // Scale sprite to fit exactly one cell
        float cellSize = _boardManager.GetCellSize();
        Vector2 spriteSize = sr.sprite.bounds.size;
        float scaleX = cellSize / spriteSize.x;
        float scaleY = cellSize / spriteSize.y;
        float scale = Mathf.Min(scaleX, scaleY);  // Keep aspect ratio, fit within cell
        enemyObj.transform.localScale = Vector3.one * scale;
        
        // Initialize enemy with unique id
        Enemy enemyComponent = enemyObj.GetComponent<Enemy>();
        enemyComponent.Initialize(_enemySprite, _stunnedEnemySprite);
        
        // Set random direction for this enemy (Up, Down, Left, Right)
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

