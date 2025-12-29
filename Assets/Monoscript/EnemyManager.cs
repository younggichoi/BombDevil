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
    private GameManager _gameManager;
    private Color _enemyColor;
    
    // Enemy sprite (set via Initialize)
    private Sprite _enemySprite;
    
    public void Initialize(GameObject enemy, Transform enemySet, GameManager gameManager, BoardManager boardManager, Sprite enemySprite)
    {
        this.enemy = enemy;
        this.enemySet = enemySet;
        _boardManager = boardManager;
        _gameManager = gameManager;
        _enemyColor = gameManager.GetEnemyColor();
        _enemySprite = enemySprite;
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
        enemyComponent.Initialize(_gameManager, _boardManager, _enemySprite);
        
        // Set random direction for this enemy (Up, Down, Left, Right)
        enemyComponent.SetRandomDirection();
        
        return enemyObj;
    }
}

