using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Entity;
using System.Collections.Generic;

public class StageRoot : MonoBehaviour
{
    // Manager Objects (found automatically)
    private GameManager gameManager;
    private EnemyManager enemyManager;
    private BombManager bombManager;
    private BoardManager boardManager;
    private ItemManager itemManager;
    private WallManager wallManager;
    
    // Prefabs and sprites (received from StageManager)
    private GameObject enemyPrefab;
    private GameObject auxiliaryBombPrefab;
    private GameObject realBombPrefab;
    private GameObject wallPrefab;
    private Sprite enemySprite;
    private Sprite stunnedEnemySprite;
    private Sprite fieldSprite;
    private Sprite wallSprite;
    
    // Scene objects (found by name)
    private Transform enemySet;
    private Transform auxiliaryBombSet;
    private Transform realBombSet;
    private TMP_Text _1stBombText;
    private TMP_Text _2ndBombText;
    private TMP_Text _3rdBombText;

    // Bomb Icon objects (found by name)
    private GameObject _1stBombIcon;
    private GameObject _2ndBombIcon;
    private GameObject _3rdBombIcon;
    private GameObject _realBombIcon;
    
    // Check UI objects (found by name)
    private GameObject _1stBombChecked;
    private GameObject _2ndBombChecked;
    private GameObject _3rdBombChecked;
    private GameObject realBombChecked;

    // UI Buttons
    private Button resetButton;
    private Button exitButton;
    
    // Explode button and text
    private Button explodeButton;
    private TMP_Text explodeButtonText;

    // Cached references for optimization
    private bool _isInitialized = false;
    private Transform itemSet;
    private Dictionary<ItemType, GameObject> itemPrefabs;
    private TMP_Text infoText;
    private TMP_Text stageText;
    private TMP_Text timeText;
    private TMP_Text turnText;
    
    // Public accessor for GameManager (used by StageManager)
    public GameManager GameManager => gameManager;
    

    public void Install(int stageId, IngameCommonData commonData, 
        GameObject enemyPrefab, GameObject auxiliaryBombPrefab, GameObject realBombPrefab, 
        GameObject wallPrefab, GameObject itemIcon, Sprite enemySprite, Sprite stunnedEnemySprite, Sprite fieldSprite, Sprite wallSprite)
    {
        // 1. One-time Init (Find objects, cache references, set listeners)
        if (!_isInitialized)
        {
            // Store prefabs and sprites (assuming they are common for all stages as per StageManager)
            this.enemyPrefab = enemyPrefab;
            this.auxiliaryBombPrefab = auxiliaryBombPrefab;
            this.realBombPrefab = realBombPrefab;
            this.wallPrefab = wallPrefab;
            this.enemySprite = enemySprite;
            this.stunnedEnemySprite = stunnedEnemySprite;
            this.fieldSprite = fieldSprite;
            this.wallSprite = wallSprite;
            
            // Find managers in children
            gameManager = GetComponentInChildren<GameManager>();
            enemyManager = GetComponentInChildren<EnemyManager>();
            bombManager = GetComponentInChildren<BombManager>();
            boardManager = GetComponentInChildren<BoardManager>();
            itemManager = GetComponentInChildren<ItemManager>();
            wallManager = GetComponentInChildren<WallManager>();

            // Register all managers with the GameService
            GameService.Register(gameManager);
            GameService.Register(enemyManager);
            GameService.Register(bombManager);
            GameService.Register(boardManager);
            GameService.Register(itemManager);
            GameService.Register(wallManager);
            
            // Find item prefabs and UI
            ItemPrefabLibrary itemPrefabLibrary = GetComponentInChildren<ItemPrefabLibrary>();
            itemPrefabs = itemPrefabLibrary != null ? itemPrefabLibrary.GetPrefabDictionary() : new Dictionary<ItemType, GameObject>();
            itemSet = GameObject.Find("ItemSet")?.transform;
            
            // Find scene objects by name
            enemySet = GameObject.Find("EnemySet")?.transform;
            auxiliaryBombSet = GameObject.Find("AuxiliaryBombSet")?.transform;
            realBombSet = GameObject.Find("RealBombSet")?.transform;
            _1stBombText = GameObject.Find("1stBombLeftover")?.GetComponent<TMP_Text>();
            _2ndBombText = GameObject.Find("2ndBombLeftover")?.GetComponent<TMP_Text>();
            _3rdBombText = GameObject.Find("3rdBombLeftover")?.GetComponent<TMP_Text>();

            // Find bomb icon objects by name
            _1stBombIcon = GameObject.Find("1stBombIcon");
            _2ndBombIcon = GameObject.Find("2ndBombIcon");
            _3rdBombIcon = GameObject.Find("3rdBombIcon");
            _realBombIcon = GameObject.Find("RealBombIcon");
            
            // Find check UI objects by name
            _1stBombChecked = GameObject.Find("1stBombChecked");
            _2ndBombChecked = GameObject.Find("2ndBombChecked");
            _3rdBombChecked = GameObject.Find("3rdBombChecked");
            realBombChecked = GameObject.Find("RealBombChecked");
    
            resetButton = GameObject.Find("ResetButton")?.GetComponent<Button>();
            exitButton = GameObject.Find("ExitButton")?.GetComponent<Button>();
            
            // Find explode button and text
            GameObject explodeButtonObj = GameObject.Find("ExplodeButton");
            if (explodeButtonObj != null)
            {
                explodeButton = explodeButtonObj.GetComponent<Button>();
            }
            explodeButtonText = GameObject.Find("ExplodeButtonText")?.GetComponent<TMP_Text>();
            
            // Find UI for GameManager
            infoText = GameObject.Find("InfoText")?.GetComponent<TMP_Text>();
            stageText = GameObject.Find("StageText")?.GetComponent<TMP_Text>();
            timeText = GameObject.Find("TimeText")?.GetComponent<TMP_Text>();
            turnText = GameObject.Find("TurnText")?.GetComponent<TMP_Text>();

            // Connect Listeners (Once)
            if (explodeButton != null)
            {
                explodeButton.onClick.RemoveAllListeners();
                explodeButton.onClick.AddListener(() => GameService.Get<GameManager>()?.OnExplodeButtonClick());
            }
    
            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(() =>
                    GameService.Get<GameManager>()?.OnResetButtonClick());
            }
    
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(() => GameService.Get<GameManager>()?.OnExitButtonClick());
            }

            _isInitialized = true;
        }

        // 2. Per-Stage Logic (Reset dynamic state)
        
        // Load SaveData for initial bomb/item values
        SaveData saveData = JsonDataUtility.LoadGameData(1); // TODO: remove hardcoding on file number

        // Initialize all managers with their scene references first.
        gameManager.Initialize(stageId, commonData);
        // Corrected via hardcoding
        boardManager.Initialize(fieldSprite, 0.17f, -0.17f);
        enemyManager.Initialize(this.enemyPrefab, enemySet, this.enemySprite, this.stunnedEnemySprite);
        bombManager.Initialize(this.auxiliaryBombPrefab, this.realBombPrefab, 
            auxiliaryBombSet, realBombSet,
            _1stBombText, _2ndBombText, _3rdBombText,
            _1stBombIcon, _2ndBombIcon, _3rdBombIcon, _realBombIcon,
            _1stBombChecked, _2ndBombChecked, _3rdBombChecked,
            realBombChecked, explodeButtonText,
            saveData);
        itemManager.Initialize(itemPrefabs, itemSet, saveData.leftItem, itemIcon);
        wallManager.Initialize(wallPrefab, wallSprite);

        // Now that all managers are initialized, clear the stage.
        gameManager.ClearStage();

        // Set UI texts in GameManager
        gameManager.SetInfoText(infoText);
        gameManager.SetStageStatsUI(stageText, timeText, turnText);

        enemyManager = GameService.Get<EnemyManager>();
        enemyManager.SetEnemyPrefab(enemyPrefab);
        enemyManager.SetEnemySprite(enemySprite);
        
        // Create walls and enemies for this stage'
        gameManager.CreateWall();
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
