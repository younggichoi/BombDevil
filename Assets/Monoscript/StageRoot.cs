using UnityEngine;
using UnityEngine.UI;
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
    private GameObject realBombPrefab;
    private Sprite enemySprite;
    
    // Scene objects (found by name)
    private Transform enemySet;
    private Transform auxiliaryBombSet;
    private Transform realBombSet;
    private TMP_Text blueBombText;
    private TMP_Text greenBombText;
    private TMP_Text pinkBombText;
    private TMP_Text realBombText;
    
    // Check UI objects (found by name)
    private GameObject blueBombChecked;
    private GameObject greenBombChecked;
    private GameObject pinkBombChecked;
    private GameObject realBombChecked;
    
    // Explode button and text
    private Button explodeButton;
    private TMP_Text explodeButtonText;
    
    // Public accessor for GameManager (used by StageManager)
    public GameManager GameManager => gameManager;
    

    public void Install(int stageId, StageCommonData commonData, 
        GameObject enemyPrefab, GameObject auxiliaryBombPrefab, GameObject realBombPrefab, Sprite enemySprite)
    {
        // Store prefabs and sprites
        this.enemyPrefab = enemyPrefab;
        this.auxiliaryBombPrefab = auxiliaryBombPrefab;
        this.realBombPrefab = realBombPrefab;
        this.enemySprite = enemySprite;
        
        // Find managers in children
        gameManager = GetComponentInChildren<GameManager>();
        enemyManager = GetComponentInChildren<EnemyManager>();
        bombManager = GetComponentInChildren<BombManager>();
        boardManager = GetComponentInChildren<BoardManager>();
        
        // Find scene objects by name
        enemySet = GameObject.Find("EnemySet")?.transform;
        auxiliaryBombSet = GameObject.Find("AuxiliaryBombSet")?.transform;
        realBombSet = GameObject.Find("RealBombSet")?.transform;
        blueBombText = GameObject.Find("LeftoverBlueBomb")?.GetComponent<TMP_Text>();
        greenBombText = GameObject.Find("LeftoverGreenBomb")?.GetComponent<TMP_Text>();
        pinkBombText = GameObject.Find("LeftoverPinkBomb")?.GetComponent<TMP_Text>();
        realBombText = GameObject.Find("LeftoverRealBomb")?.GetComponent<TMP_Text>();
        
        // Find check UI objects by name (must be active in scene to be found)
        blueBombChecked = GameObject.Find("BlueBombChecked");
        greenBombChecked = GameObject.Find("GreenBombChecked");
        pinkBombChecked = GameObject.Find("PinkBombChecked");
        realBombChecked = GameObject.Find("RealBombChecked");
        
        // Deactivate all check UIs at game start
        if (blueBombChecked != null) blueBombChecked.SetActive(false);
        if (greenBombChecked != null) greenBombChecked.SetActive(false);
        if (pinkBombChecked != null) pinkBombChecked.SetActive(false);
        if (realBombChecked != null) realBombChecked.SetActive(false);
        
        // Find explode button and text
        GameObject explodeButtonObj = GameObject.Find("ExplodeButton");
        if (explodeButtonObj != null)
        {
            explodeButton = explodeButtonObj.GetComponent<Button>();
        }
        explodeButtonText = GameObject.Find("ExplodeButtonText")?.GetComponent<TMP_Text>();
        
        // Initialize GameManager first (loads stage data including board sprite path)
        gameManager.Initialize(enemyManager, bombManager, stageId, commonData);
        
        // Load board sprite from Resources using path from JSON
        Sprite boardSprite = LoadSprite(gameManager.GetBoardSpritePath(), "board");
        
        // Initialize BoardManager (calculates cellSize, scales board sprite)
        boardManager.Initialize(gameManager, boardSprite);
        
        // Set BoardManager reference in GameManager
        gameManager.SetBoardManager(boardManager);
        
        // Find and set InfoText UI
        TMP_Text infoText = GameObject.Find("InfoText")?.GetComponent<TMP_Text>();
        gameManager.SetInfoText(infoText);
        
        // Find and set Stage Stats UI
        TMP_Text stageText = GameObject.Find("StageText")?.GetComponent<TMP_Text>();
        TMP_Text timeText = GameObject.Find("TimeText")?.GetComponent<TMP_Text>();
        TMP_Text turnText = GameObject.Find("TurnText")?.GetComponent<TMP_Text>();
        gameManager.SetStageStatsUI(stageText, timeText, turnText);
        
        // Initialize other managers with BoardManager reference and sprites
        enemyManager.Initialize(enemyPrefab, enemySet, gameManager, boardManager, enemySprite);
        bombManager.Initialize(auxiliaryBombPrefab, realBombPrefab, gameManager, 
            auxiliaryBombSet, realBombSet,
            blueBombText, greenBombText, pinkBombText, realBombText,
            blueBombChecked, greenBombChecked, pinkBombChecked, realBombChecked,
            explodeButtonText, boardManager);
        
        // Connect explode button click event
        if (explodeButton != null)
        {
            explodeButton.onClick.RemoveAllListeners();
            explodeButton.onClick.AddListener(gameManager.OnExplodeButtonClick);
        }
        
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
