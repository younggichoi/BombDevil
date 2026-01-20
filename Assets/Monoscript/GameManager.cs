using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Entity;
using TMPro;

public partial class GameManager : MonoBehaviour
{
    // Manager references (set via Initialize)
    /*private EnemyManager enemyManager;
    private BombManager bombManager;
    private ItemManager itemManager;
    private BoardManager boardManager;
    private WallManager wallManager;*/
    // setting option from StageManager (common set)
    private float _walkDuration;
    private float _knockbackDuration;
    private Color _enemyColor;
    // setting option from JSON file
    private int _width;
    private int _height;
    private int _enemyNumber;
    private int _wallNumber;
    private int _scoring;
    private int _remainingTurns;
    private int _stageId;
    private string _boardSpritePath;
    // scoping board situation - List to allow multiple objects per cell
    private List<GameObject>[,] _board;
    // combined bomb order (tracks both auxiliary and real bombs in placement order)
    private List<(Vector2Int coord, bool isRealBomb)> _allBombs;
    // tracking wall order
    private List<Vector2Int> _walls;

    // --- Item system variables ---
    // tracking placed item order
    private List<Vector2Int> _placedItems;
    // combined item order (tracks all items in placement order)
    private List<(Vector2Int coord, ItemType itemType)> _allItems;
    private List<Vector2Int> _teleporters;
    // RealBomb kill tracking
    private int _realBombKillCount = 0;
    private int _totalEnemyCount = 0;
    // boundary coordinate
    private float _minX, _minY, _maxX, _maxY;
    // game state management
    private GameState _currentState = GameState.Playing;
    public event Action<GameState> OnGameStateChanged;
    // turn processing flag (prevents button spam during animations)
    private bool _isTurnInProgress = false;
    // RealBomb usage tracking for this turn
    private bool _realBombUsedThisTurn = false;
    // InfoText UI reference
    private TMP_Text _infoText;
    private Coroutine _tempMessageCoroutine;
    // Bomb preview system
    private GameObject _ghostBomb;
    private List<GameObject> _rangeIndicators;
    private List<GameObject> _enemyPredictionIndicators;  // Shows predicted enemy positions
    private Vector2Int _lastHoveredCell = new Vector2Int(-1, -1);
    // Stage stats UI
    private TMP_Text _stageText;
    private TMP_Text _timeText;
    private TMP_Text _turnText;
    private float _elapsedTime = 0f;

    private GameObject _enemyPrefab;
    private Sprite _enemySprite;

    public void ClearStage()
    {
        // GameService.Get<BoardManager>()?.ClearBoard();
        GameService.Get<BombManager>()?.ClearBombs();
        GameService.Get<EnemyManager>()?.ClearEnemies();
        GameService.Get<ItemManager>()?.ClearItems();
        HidePreview();
        HideItemPreview();
    }

    public void Initialize(int stageId, IngameCommonData commonData)
    {
        StopAllCoroutines();
        _isTurnInProgress = false;
        
        ClearStage();
        SetStageState(stageId);
        SetCommonData(commonData);
        _board = new List<GameObject>[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _board[x, y] = new List<GameObject>();
            }
        }
        _allBombs = new List<(Vector2Int, bool)>();
        _walls = new List<Vector2Int>();
        // --- Item system initialization ---
        _placedItems = new List<Vector2Int>();
        _allItems = new List<(Vector2Int, ItemType)>();
        _teleporters = new List<Vector2Int>();
        _realBombKillCount = 0;
        _totalEnemyCount = _enemyNumber;
        _elapsedTime = 0f;
        _currentState = GameState.Playing;

        //TODO: this is only a temporary fix. Need to find the real reason for the bug.
        // GameService.Get<BoardManager>()?.Initialize(fieldSprite, 0.5f, -0.5f);
        GameService.Register(this);
    }
    // Set default item sprite for item preview
    // public void SetDefaultItemSprite(Sprite sprite)
    // {
    //     _defaultItemSprite = sprite;
    // }

    /*public void SetBoardManager(BoardManager boardManager)
    {
        this.boardManager = boardManager;
    }

    public void SetBombManager(BombManager bombManager)
    {
        this.bombManager = bombManager;
    }*/

    public void SetStageStatsUI(TMP_Text stageText, TMP_Text timeText, TMP_Text turnText)
    {
        _stageText = stageText;
        _timeText = timeText;
        _turnText = turnText;
        UpdateStageStatsUI();
    }

    public void SetInfoText(TMP_Text infoText)
    {
        _infoText = infoText;
        SetInfoMessage("Player's turn");
    }

    // public void SetDefaultBombSprite(Sprite sprite)
    // {
    //     _defaultBombSprite = sprite;
    // }

    public void SetEnemyPrefab(GameObject enemyPrefab)
    {
        _enemyPrefab = enemyPrefab;
    }

    public void SetEnemySprite(Sprite enemySprite)
    {
        _enemySprite = enemySprite;
    }

    private void SetCommonData(IngameCommonData commonData)
    {
        _walkDuration = commonData.walkDuration;
        _knockbackDuration = commonData.knockbackDuration;
        _enemyColor = commonData.enemyColor;
    }

    private void SetStageState(int stageId)
    { 
        string path = Path.Combine(Application.streamingAssetsPath, "Json/Stage/stage" + stageId + ".json");
        if (!File.Exists(path))
        {
            Debug.LogError($"Failed to load stage{stageId}.json from {path}");
            return;
        }
        string json = File.ReadAllText(path);
        StageData editorData = JsonUtility.FromJson<StageData>(json);
        _stageId = stageId;
        _width = editorData.width;
        _height = editorData.height;
        _enemyNumber = editorData.enemyNumber;
        _wallNumber = editorData.wallNumber;
        _remainingTurns = editorData.remainingTurns;
        _boardSpritePath = editorData.boardSpritePath;
    }

    void Update()
    {
        _bombTypeChanged = false; //TODO: bombTypeChanged is only a temporary measure; full fix required later
        if (_board == null || _currentState != GameState.Playing)
            return;
        _elapsedTime += Time.deltaTime;
        UpdateTimeText();

        var itemManager = GameService.Get<ItemManager>();
        var bombManager = GameService.Get<BombManager>();

        if (Input.GetMouseButtonDown(0))
            MouseClickProcess();

        if (Input.GetMouseButtonDown(1))
            MouseRightClickProcess();
        
        // --- Bomb Selection Input ---
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            itemManager?.UnselectItem();
            if (bombManager != null) bombManager.SetCurrentIndex(0);
            _bombTypeChanged = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            itemManager?.UnselectItem();
            if (bombManager != null) bombManager.SetCurrentIndex(1);
            _bombTypeChanged = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            itemManager?.UnselectItem();
            if (bombManager != null) bombManager.SetCurrentIndex(2);
            _bombTypeChanged = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            itemManager?.UnselectItem();
            if (bombManager != null) bombManager.SetCurrentIndex(3);
            _bombTypeChanged = true;
        }
        
        // Show item preview if item is selected, otherwise bomb preview, otherwise remove preview, otherwise hide all
        if (itemManager != null && itemManager.HasItemSelected())
        {
            UpdateItemPreview();
            HidePreview(); // Hide bomb preview if item is selected
        }
        else if (bombManager != null && bombManager.HasBombSelected())
        {
            UpdateBombPreview();
            HideItemPreview(); // Hide item preview if bomb is selected
        }
        else
        {
            HideItemPreview();
            HidePreview();
        }
    }

    private BoardManager _boardManager;
    private BoardManager BoardManager
    {
        get
        {
            if (_boardManager == null)
            {
                _boardManager = GameService.Get<BoardManager>();
            }
            return _boardManager;
        }
    }

    private EnemyManager _enemyManager;
    private EnemyManager EnemyManager
    {
        get
        {
            if (_enemyManager == null)
            {
                _enemyManager = GameService.Get<EnemyManager>();
            }
            return _enemyManager;
        }
    }

    private BombManager _bombManager;
    private BombManager BombManager
    {
        get
        {
            if (_bombManager == null)
            {
                _bombManager = GameService.Get<BombManager>();
            }
            return _bombManager;
        }
    }

    private ItemManager _itemManager;
    private ItemManager ItemManager
    {
        get
        {
            if (_itemManager == null)
            {
                _itemManager = GameService.Get<ItemManager>();
            }
            return _itemManager;
        }
    }

    private WallManager _wallManager;
    private WallManager WallManager
    {
        get
        {
            if (_wallManager == null)
            {
                _wallManager = GameService.Get<WallManager>();
            }

            return _wallManager;
        }
    }
}
