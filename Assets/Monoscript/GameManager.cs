using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Entity;
using TMPro;

public partial class GameManager : MonoBehaviour
{
    // Manager references (set via Initialize)
    private EnemyManager enemyManager;
    private BombManager bombManager;
    private ItemManager itemManager;
    private BoardManager boardManager;
    // setting option from StageManager (common set)
    private float _walkDuration;
    private float _knockbackDuration;
    private Color _enemyColor;
    // setting option from JSON file
    private int _width;
    private int _height;
    private int _enemyNumber;
    private int _initial1stBomb;
    private int _initial2ndBomb;
    private int _initial3rdBomb;
    private int _initial4thBomb;
    private int _initial5thBomb;
    private int _initial6thBomb;
    private int _initialSkyblueBomb;
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
    // default item sprite for preview
    private Sprite _defaultItemSprite;
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
    private Sprite _defaultBombSprite;
    // Remove mode
    private bool _isRemoveMode = false;
    private GameObject _removeIndicator;  // X marker for remove mode
    // Stage stats UI
    private TMP_Text _stageText;
    private TMP_Text _timeText;
    private TMP_Text _turnText;
    private float _elapsedTime = 0f;

    public void ClearStage()
    {
        if (boardManager != null)
        {
            boardManager.ClearBoard();
        }
        if (bombManager != null)
        {
            bombManager.ClearBombs();
        }
        if (enemyManager != null)
        {
            enemyManager.ClearEnemies();
        }
        if (itemManager != null)
        {
            itemManager.ClearItems();
            itemManager.ResetItems();
        }
        HidePreview();
        HideItemPreview();
        HideRemovePreview();
    }

    public void Initialize(EnemyManager enemyManager, BombManager bombManager, ItemManager itemManager, int stageId, StageCommonData commonData)
    {
        StopAllCoroutines();
        _isTurnInProgress = false;
        
        ClearStage();
        SetStageState(stageId);
        SetCommonData(commonData);
        this.enemyManager = enemyManager;
        this.bombManager = bombManager;
        this.itemManager = itemManager;
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
    }
    // Set default item sprite for item preview
    public void SetDefaultItemSprite(Sprite sprite)
    {
        _defaultItemSprite = sprite;
    }

    public void SetBoardManager(BoardManager boardManager)
    {
        this.boardManager = boardManager;
    }

    public void SetBombManager(BombManager bombManager)
    {
        this.bombManager = bombManager;
    }

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

    public void SetDefaultBombSprite(Sprite sprite)
    {
        _defaultBombSprite = sprite;
    }

    private void SetCommonData(StageCommonData commonData)
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
        StageDifferentData differentData = JsonUtility.FromJson<StageDifferentData>(json);
        _stageId = stageId;
        _width = differentData.width;
        _height = differentData.height;
        _enemyNumber = differentData.enemyNumber;
        _initial1stBomb = differentData.initial1stBomb;
        _initial2ndBomb = differentData.initial2ndBomb;
        _initial3rdBomb = differentData.initial3rdBomb;
        _initial4thBomb = differentData.initial4thBomb;
        _initial5thBomb = differentData.initial5thBomb;
        _initial6thBomb = differentData.initial6thBomb;
        _initialSkyblueBomb = differentData.initialSkyblueBomb;
        _remainingTurns = differentData.remainingTurns;
        _boardSpritePath = differentData.boardSpritePath;
    }

    void Update()
    {
        if (_board == null || _currentState != GameState.Playing)
            return;
        _elapsedTime += Time.deltaTime;
        UpdateTimeText();

        // Show item preview if item is selected, otherwise bomb preview, otherwise remove preview, otherwise hide all
        if (itemManager != null && itemManager.HasItemSelected())
        {
            ExitRemoveMode();
            UpdateItemPreview();
            HidePreview(); // Hide bomb preview if item is selected
        }
        else if (bombManager != null && bombManager.HasBombSelected())
        {
            ExitRemoveMode();
            UpdateBombPreview();
            HideItemPreview(); // Hide item preview if bomb is selected
        }
        else if (_isRemoveMode)
        {
            HideItemPreview();
            HidePreview();
            UpdateRemovePreview();
        }
        else
        {
            HideItemPreview();
            HidePreview();
            HideRemovePreview();
        }

        if (Input.GetMouseButtonDown(0))
            MouseClickProcess();
        
        // --- Bomb Selection Input ---
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ExitRemoveMode();
            itemManager.ClearCurrentItemType();
            if (bombManager != null) bombManager.SetCurrentBombType(BombType.FirstBomb);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ExitRemoveMode();
            itemManager.ClearCurrentItemType();
            if (bombManager != null) bombManager.SetCurrentBombType(BombType.SecondBomb);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ExitRemoveMode();
            itemManager.ClearCurrentItemType();
            if (bombManager != null) bombManager.SetCurrentBombType(BombType.ThirdBomb);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ExitRemoveMode();
            itemManager.ClearCurrentItemType();
            if (bombManager != null) bombManager.SetCurrentBombType(BombType.FourthBomb);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ExitRemoveMode();
            itemManager.ClearCurrentItemType();
            if (bombManager != null) bombManager.SetCurrentBombType(BombType.FifthBomb);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            ExitRemoveMode();
            itemManager.ClearCurrentItemType();
            if (bombManager != null) bombManager.SetCurrentBombType(BombType.SixthBomb);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ExitRemoveMode();
            itemManager.ClearCurrentItemType();
            if (bombManager != null) bombManager.SetCurrentBombType(BombType.SkyblueBomb);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            ExitRemoveMode();
            itemManager.ClearCurrentItemType();
            if (bombManager != null) bombManager.SetCurrentBombType(BombType.RealBomb);
        }
    }
}
