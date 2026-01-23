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
    private TreasureChestManager treasureChestManager;
    
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
    private TMP_Text _1stBombLeftoverText;
    private TMP_Text _2ndBombLeftoverText;
    private TMP_Text _3rdBombLeftoverText;
    private TMP_Text _1stBombNameText;
    private TMP_Text _2ndBombNameText;
    private TMP_Text _3rdBombNameText;
    private TMP_Text _itemLeftoverText;

    // Bomb Icon objects (found by name)
    private GameObject _1stBombIcon;
    private GameObject _2ndBombIcon;
    private GameObject _3rdBombIcon;
    private GameObject _realBombIcon;

    // UI Buttons
    private Button resetButton;
    private Button exitButton;
    
    // Explode button and text
    private Button explodeButton;

    // Cached references for optimization
    private bool _isInitialized = false;
    private Transform itemSet;
    private Dictionary<ItemType, GameObject> itemPrefabs;
    private TMP_Text infoText;
    private TMP_Text stageText;
    private TMP_Text timeText;
    private TMP_Text turnText;
    private TMP_Text scoringText;

    // Difficulty mode
    private bool _realBombEasyMode = false; // if difficulty == 0, range of real bomb becomes wider.
    private bool _realBombHardMode = false; // if difficulty == 4, range of real bomb becomes narrower.
    private bool _enemyMoveDisable = false; // if difficulty is 0~1, enemy move is disabled.
    private bool _wallDisable = false; // if difficulty is 0~2, wall is disabled.
    
    // Canvas for UI-based game objects
    private RectTransform _canvasRectTransform;
    private bool _useCanvasUI = true; // Set to true to use Canvas UI mode
    
    // Public accessor for GameManager (used by StageManager)
    public GameManager GameManager => gameManager;
    

    public void Install(int stageId, IngameCommonData commonData, 
        GameObject enemyPrefab, GameObject auxiliaryBombPrefab, GameObject realBombPrefab, 
        GameObject wallPrefab, GameObject treasureChestPrefab, GameObject itemIcon, 
        Sprite enemySprite, Sprite stunnedEnemySprite, Sprite fieldSprite, 
        Sprite wallSprite, Sprite treasureChestSprite, float centerX, float centerY)
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
            treasureChestManager = GetComponentInChildren<TreasureChestManager>();

            // Register all managers with the GameService
            GameService.Register(gameManager);
            GameService.Register(enemyManager);
            GameService.Register(bombManager);
            GameService.Register(boardManager);
            GameService.Register(itemManager);
            GameService.Register(wallManager);
            GameService.Register(treasureChestManager);
            
            // Find item prefabs and UI
            ItemPrefabLibrary itemPrefabLibrary = GetComponentInChildren<ItemPrefabLibrary>();
            itemPrefabs = itemPrefabLibrary != null ? itemPrefabLibrary.GetPrefabDictionary() : new Dictionary<ItemType, GameObject>();
            itemSet = GameObject.Find("ItemSet")?.transform;
            
            // Find scene objects by name
            enemySet = GameObject.Find("EnemySet")?.transform;
            auxiliaryBombSet = GameObject.Find("AuxiliaryBombSet")?.transform;
            realBombSet = GameObject.Find("RealBombSet")?.transform;
            _1stBombLeftoverText = GameObject.Find("1stBombLeftover")?.GetComponent<TMP_Text>();
            _2ndBombLeftoverText = GameObject.Find("2ndBombLeftover")?.GetComponent<TMP_Text>();
            _3rdBombLeftoverText = GameObject.Find("3rdBombLeftover")?.GetComponent<TMP_Text>();
            _1stBombNameText = GameObject.Find("1stBombName")?.GetComponent<TMP_Text>();
            _2ndBombNameText = GameObject.Find("2ndBombName")?.GetComponent<TMP_Text>();
            _3rdBombNameText = GameObject.Find("3rdBombName")?.GetComponent<TMP_Text>();
            _itemLeftoverText = GameObject.Find("ItemLeftover")?.GetComponent<TMP_Text>();

            // Find Canvas for UI-based game objects
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                _canvasRectTransform = canvas.GetComponent<RectTransform>();
            }

            // Find bomb icon objects by name
            _1stBombIcon = GameObject.Find("1stBombIcon");
            _2ndBombIcon = GameObject.Find("2ndBombIcon");
            _3rdBombIcon = GameObject.Find("3rdBombIcon");
            _realBombIcon = GameObject.Find("RealBombIcon");
    
            resetButton = GameObject.Find("ResetButton")?.GetComponent<Button>();
            exitButton = GameObject.Find("ExitButton")?.GetComponent<Button>();
            
            // Find explode button and text
            GameObject explodeButtonObj = GameObject.Find("ExplodeButton");
            if (explodeButtonObj != null)
            {
                explodeButton = explodeButtonObj.GetComponent<Button>();
            }
            
            // Find UI for GameManager
            infoText = GameObject.Find("InfoText")?.GetComponent<TMP_Text>();
            stageText = GameObject.Find("StageText")?.GetComponent<TMP_Text>();
            timeText = GameObject.Find("TimeText")?.GetComponent<TMP_Text>();
            turnText = GameObject.Find("TurnText")?.GetComponent<TMP_Text>();
            scoringText = GameObject.Find("ScoringText")?.GetComponent<TMP_Text>();

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

            SaveData initData = JsonDataUtility.LoadGameData(1); // TODO: remove hardcoding on file number
            switch (initData.difficulty)
            {
                case 0:
                    _realBombEasyMode = true;
                    _enemyMoveDisable = true;
                    _wallDisable = true;
                    break;
                case 1:
                    _enemyMoveDisable = true;
                    _wallDisable = true;
                    break;
                case 2:
                    _wallDisable = true;
                    break;
                case 3:
                    break;
                case 4:
                    _realBombHardMode = true;
                    break;
            }
            _isInitialized = true;

        }

        // 2. Per-Stage Logic (Reset dynamic state)


        // set the center position of parent object
        Transform wallSet = GameObject.Find("WallSet").transform;
        Transform treasureChestSet = GameObject.Find("TreasureChestSet").transform;
        
        // For Canvas UI mode, Set objects should be children of Canvas and positioned at origin
        // The individual items will be positioned relative to Canvas using GridToCanvasPosition
        if (_useCanvasUI && _canvasRectTransform != null)
        {
            // Move Set objects under Canvas and reset their positions
            enemySet.SetParent(_canvasRectTransform, false);
            auxiliaryBombSet.SetParent(_canvasRectTransform, false);
            realBombSet.SetParent(_canvasRectTransform, false);
            itemSet.SetParent(_canvasRectTransform, false);
            wallSet.SetParent(_canvasRectTransform, false);
            treasureChestSet.SetParent(_canvasRectTransform, false);
            
            // Reset RectTransform positions to origin (items will be positioned individually)
            SetRectTransformToOrigin(enemySet);
            SetRectTransformToOrigin(auxiliaryBombSet);
            SetRectTransformToOrigin(realBombSet);
            SetRectTransformToOrigin(itemSet);
            SetRectTransformToOrigin(wallSet);
            SetRectTransformToOrigin(treasureChestSet);
        }
        else
        {
            // Legacy World Space mode
            enemySet.position = new Vector3(centerX, centerY, 0);
            auxiliaryBombSet.position = new Vector3(centerX, centerY, 0);
            realBombSet.position = new Vector3(centerX, centerY, 0);
            itemSet.position = new Vector3(centerX, centerY, 0);
            wallSet.position = new Vector3(centerX, centerY, 0);
            treasureChestSet.position = new Vector3(centerX, centerY, 0);
        }
        
        // Load SaveData for initial bomb/item values
        SaveData saveData = JsonDataUtility.LoadGameData(1); // TODO: remove hardcoding on file number

        // Initialize all managers with their scene references first.
        gameManager.Initialize(stageId, commonData, centerX, centerY, _enemyMoveDisable, saveData.scoring);
        
        // Initialize BoardManager with Canvas UI or legacy World Space mode
        boardManager.Initialize(_canvasRectTransform, fieldSprite, centerX, centerY);
        gameManager.SetCanvasRectTransform(_canvasRectTransform);

        enemyManager.Initialize(this.enemyPrefab, enemySet, this.enemySprite, this.stunnedEnemySprite);
        bombManager.Initialize(this.auxiliaryBombPrefab, this.realBombPrefab, 
            auxiliaryBombSet, realBombSet,
            _1stBombLeftoverText, _2ndBombLeftoverText, _3rdBombLeftoverText,
            _1stBombNameText, _2ndBombNameText, _3rdBombNameText,
            _1stBombIcon, _2ndBombIcon, _3rdBombIcon, _realBombIcon,
            saveData, _realBombEasyMode, _realBombHardMode);
        itemManager.Initialize(itemPrefabs, itemSet, saveData.leftItem, itemIcon, _itemLeftoverText);
        wallManager.Initialize(wallPrefab, wallSprite);
        treasureChestManager.Initialize(treasureChestSet, treasureChestSprite, treasureChestPrefab);

        // Now that all managers are initialized, clear the stage.
        gameManager.ClearStage();

        // Set UI texts in GameManager
        gameManager.SetInfoText(infoText);
        gameManager.SetStageStatsUI(stageText, timeText, turnText, scoringText);

        enemyManager = GameService.Get<EnemyManager>();
        enemyManager.SetEnemyPrefab(enemyPrefab);
        enemyManager.SetEnemySprite(enemySprite);
        
        // Create objects for this stage'
        if (!_wallDisable)
            gameManager.CreateWall();
        gameManager.CreateEnemy();
        gameManager.CreateTreasureChest();
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
    
    // Helper method to set RectTransform to origin
    private void SetRectTransformToOrigin(Transform transform)
    {
        RectTransform rect = transform.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = transform.gameObject.AddComponent<RectTransform>();
        }
        rect.anchoredPosition = Vector2.zero;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }
}
