using UnityEngine;
using TMPro;
using Entity;

public class StageRoot : MonoBehaviour
{
    // Manager Objects (found automatically)
    private GameManager gameManager;
    private EnemyManager enemyManager;
    private BombManager bombManager;
    private BoardManager boardManager;
    
    // Prefabs and sprites (received from StageManager)
    private GameObject enemyPrefab;
    private GameObject auxiliaryBombPrefab;
    private Sprite enemySprite;
    
    // Scene objects (found by name)
    private Transform enemySet;
    private Transform auxiliaryBombSet;
    private TMP_Text leftoverAuxiliaryBombText;
    
    // Public accessor for GameManager (used by StageManager)
    public GameManager GameManager => gameManager;
    

    public void Install(int stageId, StageCommonData commonData, 
        GameObject enemyPrefab, GameObject auxiliaryBombPrefab, Sprite enemySprite)
    {
        // Store prefabs and sprites
        this.enemyPrefab = enemyPrefab;
        this.auxiliaryBombPrefab = auxiliaryBombPrefab;
        this.enemySprite = enemySprite;
        
        // Find managers in children
        gameManager = GetComponentInChildren<GameManager>();
        enemyManager = GetComponentInChildren<EnemyManager>();
        bombManager = GetComponentInChildren<BombManager>();
        boardManager = GetComponentInChildren<BoardManager>();
        
        // Find scene objects by name
        enemySet = GameObject.Find("EnemySet")?.transform;
        auxiliaryBombSet = GameObject.Find("AuxiliaryBombSet")?.transform;
        leftoverAuxiliaryBombText = GameObject.Find("LeftoverAuxiliaryBomb")?.GetComponent<TMP_Text>();
        
        // Initialize GameManager first (loads stage data including board sprite path)
        gameManager.Initialize(enemyManager, bombManager, stageId, commonData);
        
        // Load board sprite from Resources using path from JSON
        Sprite boardSprite = LoadSprite(gameManager.GetBoardSpritePath(), "board");
        
        // Initialize BoardManager (calculates cellSize, scales board sprite)
        boardManager.Initialize(gameManager, boardSprite);
        
        // Set BoardManager reference in GameManager
        gameManager.SetBoardManager(boardManager);
        
        // Initialize other managers with BoardManager reference and sprites
        enemyManager.Initialize(enemyPrefab, enemySet, gameManager, boardManager, enemySprite);
        bombManager.Initialize(auxiliaryBombPrefab, gameManager, auxiliaryBombSet, leftoverAuxiliaryBombText, boardManager);
        
        // Create enemies for this stage
        gameManager.CreateEnemy();
    }
    
    // Load sprite from Resources folder
    private Sprite LoadSprite(string path, string assetName)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"StageRoot: {assetName} sprite path is empty!");
            return null;
        }
        
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogError($"StageRoot: Failed to load {assetName} sprite from Resources/{path}");
        }
        return sprite;
    }
}
