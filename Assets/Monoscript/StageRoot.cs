#define USE_EDITOR

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Entity;
using System.Collections.Generic;


public class StageRoot : MonoBehaviour
{
    // Manager Objects (found automatically)
    private GameManager _gameManager;
    private EnemyManager _enemyManager;
    private BombManager _bombManager;
    private BoardManager _boardManager;
    private ItemManager _itemManager;
    private WallManager _wallManager;
    private TreasureChestManager _treasureChestManager;

    // Prefabs and sprites (received from StageManager)
    private GameObject _enemyPrefab;
    private GameObject _auxiliaryBombPrefab;
    private GameObject _realBombPrefab;
    private GameObject _wallPrefab;
    private Sprite _enemySprite;
    private Sprite _stunnedEnemySprite;
    private Sprite _fieldSprite;
    private Sprite _wallSprite;

    // Scene objects (found by name)
    private Transform _enemySet;
    private Transform _auxiliaryBombSet;
    private Transform _realBombSet;
    private TMP_Text _1StBombLeftoverText;
    private TMP_Text _2NdBombLeftoverText;
    private TMP_Text _3RdBombLeftoverText;
    private TMP_Text _1StBombNameText;
    private TMP_Text _2NdBombNameText;
    private TMP_Text _3RdBombNameText;
    private TMP_Text _itemLeftoverText;

    // Bomb Icon objects (found by name)
    private GameObject _1StBombIcon;
    private GameObject _2NdBombIcon;
    private GameObject _3RdBombIcon;
    private GameObject _realBombIcon;

    // UI Buttons
    private Button _resetButton;
    private Button _exitButton;

    // Explode button and text
    private Button _explodeButton;

    // Cached references for optimization
    private bool _isInitialized;
    private Transform _itemSet;
    private Dictionary<ItemType, GameObject> _itemPrefabs;
    private TMP_Text _infoText;
    private TMP_Text _stageText;
    private TMP_Text _timeText;
    private TMP_Text _turnText;
    private TMP_Text _scoringText;

    // Difficulty mode
    private bool _realBombEasyMode; // if difficulty == 0, range of real bomb becomes wider.
    private bool _realBombHardMode; // if difficulty == 4, range of real bomb becomes narrower.
    private bool _enemyMoveDisable; // if difficulty is 0~1, enemy move is disabled.
    private bool _wallDisable; // if difficulty is 0~2, wall is disabled.

    // Canvas for UI-based game objects
    private RectTransform _canvasRectTransform;
    private readonly bool _useCanvasUI = true; // Set to true to use Canvas UI mode

    // Public accessor for GameManager (used by StageManager)
    public GameManager GameManager => _gameManager;


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
            _enemyPrefab = enemyPrefab;
            _auxiliaryBombPrefab = auxiliaryBombPrefab;
            _realBombPrefab = realBombPrefab;
            _wallPrefab = wallPrefab;
            _enemySprite = enemySprite;
            _stunnedEnemySprite = stunnedEnemySprite;
            _fieldSprite = fieldSprite;
            _wallSprite = wallSprite;

            // Find managers in children
            _gameManager = GetComponentInChildren<GameManager>();
            _enemyManager = GetComponentInChildren<EnemyManager>();
            _bombManager = GetComponentInChildren<BombManager>();
            _boardManager = GetComponentInChildren<BoardManager>();
            _itemManager = GetComponentInChildren<ItemManager>();
            _wallManager = GetComponentInChildren<WallManager>();
            _treasureChestManager = GetComponentInChildren<TreasureChestManager>();

            // Register all managers with the GameService
            GameService.Register(_gameManager);
            GameService.Register(_enemyManager);
            GameService.Register(_bombManager);
            GameService.Register(_boardManager);
            GameService.Register(_itemManager);
            GameService.Register(_wallManager);
            GameService.Register(_treasureChestManager);

            // Find item prefabs and UI
            ItemPrefabLibrary itemPrefabLibrary = GetComponentInChildren<ItemPrefabLibrary>();
            _itemPrefabs = itemPrefabLibrary != null
                ? itemPrefabLibrary.GetPrefabDictionary()
                : new Dictionary<ItemType, GameObject>();
            _itemSet = GameObject.Find("ItemSet")?.transform;

            // Find scene objects by name
            _enemySet = GameObject.Find("EnemySet")?.transform;
            _auxiliaryBombSet = GameObject.Find("AuxiliaryBombSet")?.transform;
            _realBombSet = GameObject.Find("RealBombSet")?.transform;
            _1StBombLeftoverText = GameObject.Find("1stBombLeftover")?.GetComponent<TMP_Text>();
            _2NdBombLeftoverText = GameObject.Find("2ndBombLeftover")?.GetComponent<TMP_Text>();
            _3RdBombLeftoverText = GameObject.Find("3rdBombLeftover")?.GetComponent<TMP_Text>();
            _1StBombNameText = GameObject.Find("1stBombName")?.GetComponent<TMP_Text>();
            _2NdBombNameText = GameObject.Find("2ndBombName")?.GetComponent<TMP_Text>();
            _3RdBombNameText = GameObject.Find("3rdBombName")?.GetComponent<TMP_Text>();
            _itemLeftoverText = GameObject.Find("ItemLeftover")?.GetComponent<TMP_Text>();

            // Find Canvas for UI-based game objects
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                _canvasRectTransform = canvas.GetComponent<RectTransform>();
            }

            // Find bomb icon objects by name
            _1StBombIcon = GameObject.Find("1stBombIcon");
            _2NdBombIcon = GameObject.Find("2ndBombIcon");
            _3RdBombIcon = GameObject.Find("3rdBombIcon");
            _realBombIcon = GameObject.Find("RealBombIcon");

            _resetButton = GameObject.Find("ResetButton")?.GetComponent<Button>();
            _exitButton = GameObject.Find("ExitButton")?.GetComponent<Button>();

            // Find explode button and text
            GameObject explodeButtonObj = GameObject.Find("ExplodeButton");
            if (explodeButtonObj != null)
            {
                _explodeButton = explodeButtonObj.GetComponent<Button>();
            }

            // Find UI for GameManager
            _infoText = GameObject.Find("InfoText")?.GetComponent<TMP_Text>();
            _stageText = GameObject.Find("StageText")?.GetComponent<TMP_Text>();
            _timeText = GameObject.Find("TimeText")?.GetComponent<TMP_Text>();
            _turnText = GameObject.Find("TurnText")?.GetComponent<TMP_Text>();
            _scoringText = GameObject.Find("ScoringText")?.GetComponent<TMP_Text>();

            // Connect Listeners (Once)
            if (_explodeButton != null)
            {
                _explodeButton.onClick.RemoveAllListeners();
                _explodeButton.onClick.AddListener(() => GameService.Get<GameManager>()?.OnExplodeButtonClick());
            }

            if (_resetButton != null)
            {
                _resetButton.onClick.RemoveAllListeners();
                _resetButton.onClick.AddListener(() =>
                    GameService.Get<GameManager>()?.OnResetButtonClick());
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveAllListeners();
                _exitButton.onClick.AddListener(() => GameService.Get<GameManager>()?.OnExitButtonClick());
            }
            SaveData initData;

#if USE_EDITOR
            initData = GameManager.pendingSaveData;
            Debug.Log("StageRoot loaded settings from memory.");
#else
            initData = JsonDataUtility.LoadGameData(1); // TODO: remove hardcoding on file number
#endif
            Debug.Log("StageRoot loaded settings from file.");
            switch (initData.difficulty)
            {
                case 0:
                    _realBombEasyMode = true;
                    // _enemyMoveDisable = true;
                    _wallDisable = true;
                    break;
                case 1:
                    // _enemyMoveDisable = true;
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
            _enemySet.SetParent(_canvasRectTransform, false);
            _auxiliaryBombSet.SetParent(_canvasRectTransform, false);
            _realBombSet.SetParent(_canvasRectTransform, false);
            _itemSet.SetParent(_canvasRectTransform, false);
            wallSet.SetParent(_canvasRectTransform, false);
            treasureChestSet.SetParent(_canvasRectTransform, false);

            // Reset RectTransform positions to origin (items will be positioned individually)
            SetRectTransformToOrigin(_enemySet);
            SetRectTransformToOrigin(_auxiliaryBombSet);
            SetRectTransformToOrigin(_realBombSet);
            SetRectTransformToOrigin(_itemSet);
            SetRectTransformToOrigin(wallSet);
            SetRectTransformToOrigin(treasureChestSet);
        }
        else
        {
            // Legacy World Space mode
            _enemySet.position = new Vector3(centerX, centerY, 0);
            _auxiliaryBombSet.position = new Vector3(centerX, centerY, 0);
            _realBombSet.position = new Vector3(centerX, centerY, 0);
            _itemSet.position = new Vector3(centerX, centerY, 0);
            wallSet.position = new Vector3(centerX, centerY, 0);
            treasureChestSet.position = new Vector3(centerX, centerY, 0);
        }

        // Load SaveData for initial bomb/item values
        SaveData saveData = JsonDataUtility.LoadGameData(1); // TODO: remove hardcoding on file number

        // Initialize BombManager and ItemManager before GameManager
        _bombManager.Initialize(_auxiliaryBombPrefab, _realBombPrefab,
            _auxiliaryBombSet, _realBombSet,
            _1StBombLeftoverText, _2NdBombLeftoverText, _3RdBombLeftoverText,
            _1StBombNameText, _2NdBombNameText, _3RdBombNameText,
            _1StBombIcon, _2NdBombIcon, _3RdBombIcon, _realBombIcon,
            saveData, _realBombEasyMode, _realBombHardMode);
        _itemManager.Initialize(_itemPrefabs, _itemSet, itemIcon, _itemLeftoverText);

        // Initialize all managers with their scene references first.
        _gameManager.Initialize(stageId, commonData, centerX, centerY, _enemyMoveDisable, saveData.scoring);

        // Initialize BoardManager with Canvas UI or legacy World Space mode
        _boardManager.Initialize(_canvasRectTransform, _fieldSprite, centerX, centerY);
        _gameManager.SetCanvasRectTransform(_canvasRectTransform);

        _enemyManager.Initialize(_enemyPrefab, _enemySet, _enemySprite, _stunnedEnemySprite);
        _wallManager.Initialize(_wallPrefab, _wallSprite);
        _treasureChestManager.Initialize(treasureChestSet, treasureChestSprite, treasureChestPrefab);

        // Now that all managers are initialized, clear the stage.
        _gameManager.ClearStage();

        // Set UI texts in GameManager
        _gameManager.SetInfoText(_infoText);
        _gameManager.SetStageStatsUI(_stageText, _timeText, _turnText, _scoringText);

        _enemyManager = GameService.Get<EnemyManager>();
        _enemyManager.SetEnemyPrefab(enemyPrefab);
        _enemyManager.SetEnemySprite(enemySprite);

        // Create objects for this stage'
        if (!_wallDisable)
            _gameManager.CreateWall();
        _gameManager.CreateEnemy();
        _gameManager.CreateTreasureChest();
    }

    // Helper method to set RectTransform to origin
    private void SetRectTransformToOrigin(Transform objectTransform)
    {
        RectTransform rect = objectTransform.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = objectTransform.gameObject.AddComponent<RectTransform>();
        }

        rect.anchoredPosition = Vector2.zero;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }
}
