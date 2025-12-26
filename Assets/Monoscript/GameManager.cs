using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Entity;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Manager references (set via Initialize)
    private EnemyManager enemyManager;
    private BombManager bombManager;
    private BoardManager boardManager;
    
    // setting option from StageManager (common set)
    private float _walkDuration;
    private float _knockbackDuration;
    private Color _enemyColor;
    
    // setting option from JSON file
    private int _width;
    private int _height;
    private int _enemyNumber;
    private int _initialBlueBomb;
    private int _initialGreenBomb;
    private int _initialPinkBomb;
    private int _remainingTurns;
    private int _stageId;
    private string _boardSpritePath;  // Resources path to board sprite
    
    // scoping board situation - List to allow multiple objects per cell
    private List<GameObject>[,] _board;
    
    // tracking auxiliary bomb order
    private List<Vector2Int> _auxiliaryBombs;
    
    // tracking real bomb order
    private List<Vector2Int> _realBombs;
    
    // combined bomb order (tracks both auxiliary and real bombs in placement order)
    private List<(Vector2Int coord, bool isRealBomb)> _allBombs;
    
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
    private Vector2Int _lastHoveredCell = new Vector2Int(-1, -1);
    private Sprite _defaultBombSprite;  // For ghost bomb visualization
    
    // Stage stats UI
    private TMP_Text _stageText;
    private TMP_Text _timeText;
    private TMP_Text _turnText;
    private float _elapsedTime = 0f;

    public void Initialize(EnemyManager enemyManager, BombManager bombManager, int stageId, StageCommonData commonData)
    {
        // Load stage data first
        SetStageState(stageId);
        SetCommonData(commonData);
        
        this.enemyManager = enemyManager;
        this.bombManager = bombManager;
        
        // Initialize board with empty lists
        _board = new List<GameObject>[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _board[x, y] = new List<GameObject>();
            }
        }
        
        _auxiliaryBombs = new List<Vector2Int>();
        _realBombs = new List<Vector2Int>();
        _allBombs = new List<(Vector2Int, bool)>();
        _realBombKillCount = 0;
        _totalEnemyCount = _enemyNumber;
        _elapsedTime = 0f;
        _currentState = GameState.Playing;
    }
    
    // Set stage stats UI references
    public void SetStageStatsUI(TMP_Text stageText, TMP_Text timeText, TMP_Text turnText)
    {
        _stageText = stageText;
        _timeText = timeText;
        _turnText = turnText;
        UpdateStageStatsUI();
    }
    
    // Update all stage stats UI
    private void UpdateStageStatsUI()
    {
        if (_stageText != null)
            _stageText.text = $"Stage {_stageId}";
        
        if (_turnText != null)
            _turnText.text = $"Turns: {_remainingTurns}";
        
        UpdateTimeText();
    }
    
    // Update time display (called every frame)
    private void UpdateTimeText()
    {
        if (_timeText != null)
        {
            int hours = (int)(_elapsedTime / 3600);
            int minutes = (int)((_elapsedTime % 3600) / 60);
            int seconds = (int)(_elapsedTime % 60);
            _timeText.text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }
    
    // Set BoardManager reference after it's initialized
    public void SetBoardManager(BoardManager boardManager)
    {
        this.boardManager = boardManager;
    }
    
    // Set InfoText reference
    public void SetInfoText(TMP_Text infoText)
    {
        _infoText = infoText;
        SetInfoMessage("Player's turn");
    }
    
    // Set info message (persistent)
    private void SetInfoMessage(string message)
    {
        if (_infoText != null)
            _infoText.text = message;
    }
    
    // Show temporary message for specified duration, then restore base message
    private void ShowTempMessage(string message, float duration, string restoreMessage)
    {
        if (_tempMessageCoroutine != null)
            StopCoroutine(_tempMessageCoroutine);
        _tempMessageCoroutine = StartCoroutine(TempMessageCoroutine(message, duration, restoreMessage));
    }
    
    private IEnumerator TempMessageCoroutine(string message, float duration, string restoreMessage)
    {
        SetInfoMessage(message);
        yield return new WaitForSeconds(duration);
        SetInfoMessage(restoreMessage);
        _tempMessageCoroutine = null;
    }
    
    // Public method for BombManager to show bomb selection message
    public void ShowBombSelectedMessage(string bombName)
    {
        ShowTempMessage($"{bombName} bomb is selected!", 1f, "Player's turn");
    }
    
    // Public method for BombManager to show no bomb left message
    public void ShowNoBombLeftMessage(string bombName)
    {
        ShowTempMessage($"There's no {bombName} bomb left!", 1f, "Player's turn");
    }
    
    // Set common data from StageManager
    private void SetCommonData(StageCommonData commonData)
    {
        _walkDuration = commonData.walkDuration;
        _knockbackDuration = commonData.knockbackDuration;
        _enemyColor = commonData.enemyColor;
    }

    void Update()
    {
        // Skip if not initialized or not playing
        if (_board == null || _currentState != GameState.Playing)
            return;
        
        // Track elapsed time
        _elapsedTime += Time.deltaTime;
        UpdateTimeText();
        
        // Update bomb preview on hover
        UpdateBombPreview();
            
        if (Input.GetMouseButtonDown(0))
            MouseClickProcess();
            
        CheckGameState();
    }
    
    // Update bomb hover preview
    private void UpdateBombPreview()
    {
        // Skip during turn processing
        if (_isTurnInProgress)
        {
            HidePreview();
            return;
        }
        
        // Check if a bomb type is selected
        if (!bombManager.HasBombSelected())
        {
            HidePreview();
            return;
        }
        
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        int x = GlobalToGridX(worldPos.x);
        int y = GlobalToGridY(worldPos.y);
        
        // Check if mouse is on the board and cell is empty
        if (x >= 0 && x < _width && y >= 0 && y < _height && _board[x, y].Count == 0)
        {
            Vector2Int currentCell = new Vector2Int(x, y);
            if (currentCell != _lastHoveredCell)
            {
                ShowPreview(x, y);
                _lastHoveredCell = currentCell;
            }
        }
        else
        {
            HidePreview();
            _lastHoveredCell = new Vector2Int(-1, -1);
        }
    }
    
    // Show bomb preview at specified cell
    private void ShowPreview(int x, int y)
    {
        BombType? bombType = bombManager.GetCurrentBombType();
        if (!bombType.HasValue) return;
        
        BombData bombData = bombManager.GetBombData(bombType.Value);
        if (bombData == null) return;
        
        float cellSize = boardManager.GetCellSize();
        Vector3 worldPos = boardManager.GridToWorld(x, y);
        
        // Create or update ghost bomb
        if (_ghostBomb == null)
        {
            _ghostBomb = new GameObject("GhostBomb");
            SpriteRenderer sr = _ghostBomb.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
        }
        
        _ghostBomb.transform.position = worldPos;
        _ghostBomb.transform.localScale = Vector3.one * cellSize;
        
        SpriteRenderer ghostSr = _ghostBomb.GetComponent<SpriteRenderer>();
        if (ghostSr != null)
        {
            // Use default bomb sprite if available, otherwise create a simple colored sprite
            if (_defaultBombSprite != null)
            {
                ghostSr.sprite = _defaultBombSprite;
            }
            else
            {
                // Create a simple square sprite
                ghostSr.sprite = CreateSquareSprite();
            }
            
            // Apply bomb color with transparency
            Color bombColor = bombData.GetColor();
            bombColor.a = 0.5f;
            ghostSr.color = bombColor;
        }
        _ghostBomb.SetActive(true);
        
        // Create range indicators
        int range = bombData.range;
        ShowRangeIndicators(x, y, range);
    }
    
    // Show range indicator tiles (full square area)
    private void ShowRangeIndicators(int centerX, int centerY, int range)
    {
        // Initialize list if needed
        if (_rangeIndicators == null)
            _rangeIndicators = new List<GameObject>();
        
        // Hide existing indicators
        foreach (var indicator in _rangeIndicators)
        {
            if (indicator != null)
                indicator.SetActive(false);
        }
        
        float cellSize = boardManager.GetCellSize();
        int indicatorIndex = 0;
        
        // Create indicators for all cells in the square range (excluding center)
        // For range 2, this covers a 5x5 area = 24 cells (excluding center)
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                // Skip center cell
                if (dx == 0 && dy == 0)
                    continue;
                
                int targetX = Mod(centerX + dx, _width);
                int targetY = Mod(centerY + dy, _height);
                
                Vector3 worldPos = boardManager.GridToWorld(targetX, targetY);
                
                // Create new indicator if needed
                while (indicatorIndex >= _rangeIndicators.Count)
                {
                    GameObject indicator = new GameObject($"RangeIndicator_{_rangeIndicators.Count}");
                    SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
                    sr.sprite = CreateSquareSprite();
                    sr.sortingOrder = 4;
                    _rangeIndicators.Add(indicator);
                }
                
                GameObject rangeObj = _rangeIndicators[indicatorIndex];
                rangeObj.transform.position = worldPos;
                rangeObj.transform.localScale = Vector3.one * cellSize;
                
                SpriteRenderer rangeSr = rangeObj.GetComponent<SpriteRenderer>();
                if (rangeSr != null)
                {
                    // Red color with transparency for attack range
                    rangeSr.color = new Color(1f, 0.3f, 0.3f, 0.4f);
                }
                rangeObj.SetActive(true);
                indicatorIndex++;
            }
        }
    }
    
    // Hide all preview elements
    private void HidePreview()
    {
        if (_ghostBomb != null)
            _ghostBomb.SetActive(false);
        
        if (_rangeIndicators != null)
        {
            foreach (var indicator in _rangeIndicators)
            {
                if (indicator != null)
                    indicator.SetActive(false);
            }
        }
    }
    
    // Create a simple square sprite for indicators
    private Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.white;
        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
    }
    
    // Set default bomb sprite for preview (called from StageRoot)
    public void SetDefaultBombSprite(Sprite sprite)
    {
        _defaultBombSprite = sprite;
    }
    
    // check win/lose conditions
    private void CheckGameState()
    {
        // Check win condition: all enemies eliminated by RealBomb
        if (GetEnemyCount() == 0 && _realBombKillCount == _totalEnemyCount)
        {
            SetGameState(GameState.Win);
            return;
        }
        
        // Check if all enemies are gone but not all killed by RealBomb (still counts as progress)
        // This allows the game to continue if there are still bombs to use
        
        // Check lose condition: no bombs left and no bombs placed, or RealBomb used but not all enemies killed
        bool noAuxiliaryBombs = bombManager.GetTotalLeftoverBombs() <= 0 && 
            bombManager.GetPlantedAuxiliaryBombCount() <= 0;
        bool noRealBombs = !bombManager.IsRealBombAvailable() && 
            bombManager.GetPlantedRealBombCount() <= 0;
        
        if (noAuxiliaryBombs && noRealBombs && GetEnemyCount() > 0)
        {
            SetGameState(GameState.Lose);
            return;
        }
    }
    
    // set game state and trigger event
    private void SetGameState(GameState newState)
    {
        if (_currentState == newState)
            return;
            
        _currentState = newState;
        OnGameStateChanged?.Invoke(_currentState);
        Debug.Log($"Game State Changed: {_currentState}");
    }
    
    // get current game state
    public GameState GetCurrentState()
    {
        return _currentState;
    }
    
    // count enemies on the board
    private int GetEnemyCount()
    {
        int count = 0;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                foreach (var obj in _board[x, y])
                {
                    if (obj != null && obj.GetComponent<Enemy>() != null)
                        count++;
                }
            }
        }
        return count;
    }

    // delete previous enemy
    // and create enemy on the board in random space
    public void CreateEnemy()
    {
        DeleteEnemy();
        
        int currentEnemy = 0;
        while (currentEnemy < _enemyNumber)
        {
            int x = UnityEngine.Random.Range(0, _width);
            int y = UnityEngine.Random.Range(0, _height);
            // Allow placing even if cell has objects (but check if empty for initial placement)
            if (_board[x, y].Count == 0)
            {
                GameObject enemy = enemyManager.CreateEnemy(x, y);
                _board[x, y].Add(enemy);
                currentEnemy++;
            }
        }
    }
    
    // delete previous enemy
    public void DeleteEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject e in enemies)
        {
            Destroy(e);
        }

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _board[x, y].RemoveAll(obj => obj == null || obj.GetComponent<Enemy>() != null);
            }
        }
    }

    // Button click handler - explode bombs in order then move enemies
    public void OnExplodeButtonClick()
    {
        // Skip if not playing or turn is already in progress
        if (_currentState != GameState.Playing || _isTurnInProgress)
            return;
        
        // Start the turn sequence coroutine
        StartCoroutine(ExecuteTurnSequence());
    }
    
    // Execute turn sequence with proper timing
    private IEnumerator ExecuteTurnSequence()
    {
        // Set flag to prevent button spam
        _isTurnInProgress = true;
        _realBombUsedThisTurn = false;
        
        // Stop any temp message and show exploding
        if (_tempMessageCoroutine != null)
        {
            StopCoroutine(_tempMessageCoroutine);
            _tempMessageCoroutine = null;
        }
        SetInfoMessage("Exploding...");
        
        // Explode all bombs in placement order (with waiting)
        yield return StartCoroutine(ExplodeAllBombsCoroutine());
        
        // Check if RealBomb was used and enemies still remain
        if (_realBombUsedThisTurn && GetEnemyCount() > 0)
        {
            // RealBomb failed to kill all enemies - Game Over
            yield return new WaitForSeconds(1f);
            SetInfoMessage("Game Over");
            SetGameState(GameState.Lose);
            
            // Wait 2 seconds then quit
            yield return new WaitForSeconds(2f);
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
            yield break;
        }
        
        // Show enemy turn message
        SetInfoMessage("Enemy's turn");
        
        // Add 1 second pause before enemy movement
        yield return new WaitForSeconds(1f);
        
        // Enemy turn: move all enemies in their direction
        yield return StartCoroutine(MoveAllEnemiesCoroutine());
        
        // Decrement remaining turns
        _remainingTurns--;
        if (_turnText != null)
            _turnText.text = $"Turns: {_remainingTurns}";
        
        // Check if out of turns
        if (_remainingTurns <= 0 && GetEnemyCount() > 0)
        {
            // Out of turns - Game Over
            yield return new WaitForSeconds(1f);
            SetInfoMessage("Game Over");
            SetGameState(GameState.Lose);
            
            // Wait 2 seconds then quit
            yield return new WaitForSeconds(2f);
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
            yield break;
        }
        
        // Reset explode button text to PASS
        bombManager.ResetExplodeButtonText();
        
        // Back to player turn
        SetInfoMessage("Player's turn");
        
        // Check game state after turn
        CheckGameState();
        
        // Clear flag - turn is complete
        _isTurnInProgress = false;
    }
    
    // Explode all bombs (auxiliary and real) in placement order (coroutine version)
    private IEnumerator ExplodeAllBombsCoroutine()
    {
        while (_allBombs.Count > 0)
        {
            var bombInfo = _allBombs[0];
            _allBombs.RemoveAt(0);
            Vector2Int bombCoordinate = bombInfo.coord;
            bool isRealBomb = bombInfo.isRealBomb;
            int x = bombCoordinate.x;
            int y = bombCoordinate.y;
            
            if (isRealBomb)
            {
                // Handle RealBomb explosion
                _realBombs.Remove(bombCoordinate);
                _realBombUsedThisTurn = true;
                
                GameObject bombObj = _board[x, y].Find(obj => obj != null && obj.GetComponent<RealBomb>() != null);
                if (bombObj == null)
                    continue;
                
                RealBomb bomb = bombObj.GetComponent<RealBomb>();
                int range = bomb.GetRange();
                
                // Remove bomb from board
                _board[x, y].Remove(bombObj);
                
                bomb.Explode();
                
                // Kill enemies in range
                KillEnemiesInRange(x, y, range);
                
                // Wait for effect
                yield return new WaitForSeconds(_knockbackDuration);
            }
            else
            {
                // Handle AuxiliaryBomb explosion
                _auxiliaryBombs.Remove(bombCoordinate);
                
                GameObject bombObj = _board[x, y].Find(obj => obj != null && obj.GetComponent<AuxiliaryBomb>() != null);
                if (bombObj == null)
                    continue;
                
                AuxiliaryBomb bomb = bombObj.GetComponent<AuxiliaryBomb>();
                int range = bomb.GetRange();
                int knockbackDistance = bomb.GetKnockbackDistance();
                
                // Remove bomb from board
                _board[x, y].Remove(bombObj);
                
                bomb.Explode();
                
                // Apply knockback with bomb's specific properties
                Knockback(x, y, range, knockbackDistance);
                
                // Wait for knockback animation to complete
                yield return new WaitForSeconds(_knockbackDuration);
            }
        }
    }
    
    // Move all enemies in their assigned direction (coroutine version)
    private IEnumerator MoveAllEnemiesCoroutine()
    {
        // Collect all enemies first to avoid modification during iteration
        List<(int x, int y, GameObject obj, Enemy enemy)> enemies = new List<(int, int, GameObject, Enemy)>();
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                foreach (var obj in _board[x, y])
                {
                    if (obj != null)
                    {
                        Enemy enemy = obj.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            enemies.Add((x, y, obj, enemy));
                        }
                    }
                }
            }
        }
        
        // Move each enemy
        foreach (var (x, y, obj, enemy) in enemies)
        {
            Direction dir = enemy.GetMoveDirection();
            
            // Move enemy visually
            enemy.MoveInDirection();
            
            // Update board position
            ReflectMoveInBoard(x, y, obj, dir, 1);
        }
        
        // Wait for walk animation to complete
        if (enemies.Count > 0)
        {
            yield return new WaitForSeconds(_walkDuration);
        }
    }
    
    // get boundary coordinate API
    public float GetMaxX()
    {
        return _maxX;
    }
    
    public float GetMinX()
    {
        return _minX;
    }
    
    public float GetMaxY()
    {
        return _maxY;
    }
    
    public float GetMinY()
    {
        return _minY;
    }
    
    // get private variable API
    public float getWalkDuration()
    {
        return _walkDuration;
    }

    public float getKnockbackDuration()
    {
        return _knockbackDuration;
    }

    public Color GetEnemyColor()
    {
        return _enemyColor;
    }


    public int GetWidth()
    {
        return _width;
    }

    public int GetHeight()
    {
        return _height;
    }

    public int GetEnemyNumber()
    {
        return _enemyNumber;
    }

    // Get initial bomb count by type
    public int GetInitialBombCount(BombType bombType)
    {
        switch (bombType)
        {
            case BombType.BlueBomb:
                return _initialBlueBomb;
            case BombType.GreenBomb:
                return _initialGreenBomb;
            case BombType.PinkBomb:
                return _initialPinkBomb;
            default:
                return 0;
        }
    }

    public string GetBoardSpritePath()
    {
        return _boardSpritePath;
    }



    // find enemy within range of (x,y) and push them (call Enemy.Knockback)
    // range: how far from bomb center enemies are affected (1 = adjacent, 2 = 2 tiles away)
    // knockbackDistance: how far enemies are pushed back
    private void Knockback(int x, int y, int range, int knockbackDistance)
    {
        // Check all 8 directions
        Direction[] directions = { Direction.Up, Direction.Down, Direction.Right, Direction.Left,
                                   Direction.UpRight, Direction.UpLeft, Direction.DownRight, Direction.DownLeft };
        int[,] dirOffsets = { {0, 1}, {0, -1}, {1, 0}, {-1, 0}, {1, 1}, {-1, 1}, {1, -1}, {-1, -1} };
        
        // Collect enemies to knockback (to avoid modification during iteration)
        List<(int targetX, int targetY, GameObject obj, Enemy enemy, Direction dir)> toKnockback = 
            new List<(int, int, GameObject, Enemy, Direction)>();
        
        // Apply knockback to enemies within range
        for (int r = 1; r <= range; r++)
        {
            for (int d = 0; d < directions.Length; d++)
            {
                int targetX = Mod(x + dirOffsets[d, 0] * r, _width);
                int targetY = Mod(y + dirOffsets[d, 1] * r, _height);
                
                foreach (var obj in _board[targetX, targetY])
                {
                    if (obj != null)
                    {
                        Enemy enemy = obj.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            toKnockback.Add((targetX, targetY, obj, enemy, directions[d]));
                        }
                    }
                }
            }
        }
        
        // Apply knockback
        foreach (var (targetX, targetY, obj, enemy, dir) in toKnockback)
        {
            enemy.Knockback(dir, knockbackDistance);
            ReflectMoveInBoard(targetX, targetY, obj, dir, knockbackDistance);
        }
    }
    
    // Kill all enemies within range of (x,y) - used by RealBomb
    private void KillEnemiesInRange(int x, int y, int range)
    {
        // Check all 8 directions
        int[,] dirOffsets = { {0, 1}, {0, -1}, {1, 0}, {-1, 0}, {1, 1}, {-1, 1}, {1, -1}, {-1, -1} };
        
        // Collect enemies to kill (to avoid modification during iteration)
        List<(int targetX, int targetY, GameObject obj)> toKill = new List<(int, int, GameObject)>();
        
        // Find enemies within range
        for (int r = 1; r <= range; r++)
        {
            for (int d = 0; d < 8; d++)
            {
                int targetX = Mod(x + dirOffsets[d, 0] * r, _width);
                int targetY = Mod(y + dirOffsets[d, 1] * r, _height);
                
                foreach (var obj in _board[targetX, targetY])
                {
                    if (obj != null && obj.GetComponent<Enemy>() != null)
                    {
                        toKill.Add((targetX, targetY, obj));
                    }
                }
            }
        }
        
        // Kill collected enemies
        foreach (var (targetX, targetY, obj) in toKill)
        {
            // Remove from board
            _board[targetX, targetY].Remove(obj);
            
            // Destroy the enemy
            Destroy(obj);
            
            // Increment RealBomb kill count
            _realBombKillCount++;
            
            Debug.Log($"RealBomb killed enemy at ({targetX}, {targetY}). Total kills: {_realBombKillCount}/{_totalEnemyCount}");
        }
    }
    
    // update board when object movement occurs
    private void ReflectMoveInBoard(int x, int y, GameObject obj, Direction direction, int distance)
    {
        if (obj == null)
            return;

        Vector3 target = GetWrappedTarget(direction, distance, new Vector3(x, y, 0), 0, _width, 0, _height);
        
        // Remove from old position
        _board[x, y].Remove(obj);
        
        // Add to new position
        _board[(int)target.x, (int)target.y].Add(obj);
    }
    
    // get destination from direction, distance, start point
    private static Vector3 GetTarget(Direction direction, int distance, Vector3 start)
    {
        switch (direction)
        {
            case Direction.Up:
                return start + new Vector3(0, distance, 0);
            
            case Direction.Down:
                return start - new Vector3(0, distance, 0);
            
            case Direction.Right:
                return start + new Vector3(distance, 0, 0);
            
            case Direction.Left:
                return start - new Vector3(distance, 0, 0);
            
            case Direction.UpRight:
                return start + new Vector3(distance, distance, 0);
            
            case Direction.UpLeft:
                return start + new Vector3(-distance, distance, 0);
            
            case Direction.DownRight:
                return start + new Vector3(distance, -distance, 0);
            
            case Direction.DownLeft:
                return start - new Vector3(distance, distance, 0);
        }

        return new Vector3();
    }
    
    // get destination wrt boundary condition
    private static Vector3 GetWrappedTarget(Direction direction, int distance, Vector3 start,
        float minX = float.NegativeInfinity, float maxX = float.PositiveInfinity,
        float minY = float.NegativeInfinity, float maxY = float.PositiveInfinity,
        float minZ = float.NegativeInfinity, float maxZ = float.PositiveInfinity)
    {

        Vector3 result = GetTarget(direction, distance, start);
        
        if (result.x < minX || result.x >= maxX)
            result.x = Mod(result.x - minX, maxX - minX) + minX;
        if (result.y < minY || result.y >= maxY)
            result.y = Mod(result.y - minY, maxY - minY) + minY;
        if (result.z < minZ || result.z >= maxZ)
            result.z = Mod(result.z - minZ, maxZ - minZ) + minZ;

        return result;
    }

    // when mouse click occurs on the board, plant auxiliary bomb in the cell
    // or select bomb type from UI area
    private void MouseClickProcess()
    {
        Vector3 screenPos = Input.mousePosition;
        
        // Check if clicking on bomb selection UI area (pixel coordinates)
        // X range: 1440~1780 for all bombs
        if (screenPos.x >= 1440 && screenPos.x <= 1780)
        {
            // Check Y ranges for each bomb type
            if (screenPos.y >= 890 && screenPos.y <= 990)
            {
                // BlueBomb selection
                bombManager.SetCurrentBombType(BombType.BlueBomb);
                return;
            }
            else if (screenPos.y >= 760 && screenPos.y <= 860)
            {
                // GreenBomb selection
                bombManager.SetCurrentBombType(BombType.GreenBomb);
                return;
            }
            else if (screenPos.y >= 630 && screenPos.y <= 730)
            {
                // PinkBomb selection  
                bombManager.SetCurrentBombType(BombType.PinkBomb);
                return;
            }
            else if (screenPos.y >= 500 && screenPos.y <= 600)
            {
                // RealBomb selection
                if (bombManager.IsRealBombAvailable())
                {
                    bombManager.SetCurrentBombType(BombType.RealBomb);
                }
                return;
            }
        }
        
        // Check if a bomb type is selected
        if (!bombManager.HasBombSelected())
        {
            // Only show message if clicking on board area
            Vector3 worldPosCheck = Camera.main.ScreenToWorldPoint(screenPos);
            int testX = GlobalToGridX(worldPosCheck.x);
            int testY = GlobalToGridY(worldPosCheck.y);
            if (testX >= 0 && testX < _width && testY >= 0 && testY < _height)
            {
                ShowTempMessage("No bomb has been selected!", 1f, "Player's turn");
            }
            return;
        }
        
        // Handle board click for bomb placement
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        int x = GlobalToGridX(worldPos.x);
        int y = GlobalToGridY(worldPos.y);

        if (x >= 0 && x < _width && y >= 0 && y < _height
            && _board[x, y].Count == 0)
        {
            BombType? currentType = bombManager.GetCurrentBombType();
            if (currentType == BombType.RealBomb)
            {
                CreateRealBomb(x, y);
            }
            else
            {
                CreateAuxiliaryBomb(x, y);
            }
        }
    }
    
    // if (x,y) is empty, fill the cell by an auxiliary bomb
    private void CreateAuxiliaryBomb(int x, int y)
    {
        if (_board[x, y].Count > 0)
            return;
        
        // Check if selected bomb is available
        BombType? currentType = bombManager.GetCurrentBombType();
        if (currentType.HasValue && !bombManager.CheckBombAvailable(currentType.Value))
            return;

        GameObject bomb = bombManager.PlantAuxiliaryBomb(x, y);
        if (bomb != null)
        {
            _board[x, y].Add(bomb);
            _auxiliaryBombs.Add(new Vector2Int(x, y));
            _allBombs.Add((new Vector2Int(x, y), false));
        }
    }
    
    // if (x,y) is empty, fill the cell by a real bomb
    private void CreateRealBomb(int x, int y)
    {
        if (_board[x, y].Count > 0)
            return;

        GameObject bomb = bombManager.PlantRealBomb(x, y);
        if (bomb != null)
        {
            _board[x, y].Add(bomb);
            _realBombs.Add(new Vector2Int(x, y));
            _allBombs.Add((new Vector2Int(x, y), true));
        }
    }
    
    // global coordinate -> board coordinate (with cell size scaling)
    private int GlobalToGridX(float x)
    {
        float cellSize = boardManager.GetCellSize();
        return Mathf.FloorToInt(x / cellSize + _width / 2f);
    }
    
    private int GlobalToGridY(float y)
    {
        float cellSize = boardManager.GetCellSize();
        return Mathf.FloorToInt(y / cellSize + _height / 2f);
    }

    private static float Mod(float x, int m)
    {
        return (x % m + m) % m;
    }
    
    private static float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }
    
    private static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
    
    // init setting from stage json file
    private void SetStageState(int stageId)
    {
        // Load from Resources/Json folder (works in both editor and build)
        TextAsset jsonFile = Resources.Load<TextAsset>("Json/Stage/stage" + stageId);
        if (jsonFile == null)
        {
            Debug.LogError($"Failed to load stage{stageId}.json from Resources/Json/Stage/");
            return;
        }
        
        StageDifferentData differentData = JsonUtility.FromJson<StageDifferentData>(jsonFile.text);
        _stageId = stageId;
        _width = differentData.width;
        _height = differentData.height;
        _enemyNumber = differentData.enemyNumber;
        _initialBlueBomb = differentData.initialBlueBomb;
        _initialGreenBomb = differentData.initialGreenBomb;
        _initialPinkBomb = differentData.initialPinkBomb;
        _remainingTurns = differentData.remainingTurns;
        _boardSpritePath = differentData.boardSpritePath;
    }
}
