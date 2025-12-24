using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Entity;

public class GameManager : MonoBehaviour
{
    // Manager references (set via Initialize)
    private EnemyManager enemyManager;
    private BombManager bombManager;
    private BoardManager boardManager;
    
    // setting option from StageManager (common set)
    private int _knockbackDistance;
    private float _walkDuration;
    private float _knockbackDuration;
    private Color _enemyColor;
    private Color _auxiliaryBombColor;
    
    // setting option from JSON file
    private int _width;
    private int _height;
    private int _enemyNumber;
    private int _initialAuxiliaryBomb;
    private string _boardSpritePath;  // Resources path to board sprite
    
    // scoping board situation
    private GameObject[,] _board;
    
    // tracking auxiliary bomb order
    private List<Vector2Int> _auxiliaryBombs;
    
    // boundary coordinate
    private float _minX, _minY, _maxX, _maxY;
    
    // game state management
    private GameState _currentState = GameState.Playing;
    public event Action<GameState> OnGameStateChanged;

    public void Initialize(EnemyManager enemyManager, BombManager bombManager, int stageId, StageCommonData commonData)
    {
        // Load stage data first
        SetStageState(stageId);
        SetCommonData(commonData);
        
        this.enemyManager = enemyManager;
        this.bombManager = bombManager;
        _board = new GameObject[_width, _height];
        _auxiliaryBombs = new List<Vector2Int>();
        _currentState = GameState.Playing;
    }
    
    // Set BoardManager reference after it's initialized
    public void SetBoardManager(BoardManager boardManager)
    {
        this.boardManager = boardManager;
    }
    
    // Set common data from StageManager
    private void SetCommonData(StageCommonData commonData)
    {
        _walkDuration = commonData.walkDuration;
        _knockbackDuration = commonData.knockbackDuration;
        _knockbackDistance = commonData.knockbackDistance;
        _enemyColor = commonData.enemyColor;
        _auxiliaryBombColor = commonData.auxiliaryBombColor;
    }

    void Update()
    {
        // Skip if not initialized or not playing
        if (_board == null || _currentState != GameState.Playing)
            return;
            
        if (Input.GetMouseButtonDown(0))
            MouseClickProcess();
            
        CheckGameState();
    }
    
    // check win/lose conditions
    private void CheckGameState()
    {
        // Check win condition: all enemies eliminated
        if (GetEnemyCount() == 0)
        {
            SetGameState(GameState.Win);
            return;
        }
        
        // Check lose condition: no bombs left and no bombs placed
        if (bombManager.GetLeftoverAuxiliaryBomb() <= 0 && 
            bombManager.GetPlantedAuxiliaryBombCount() <= 0)
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
                if (_board[x, y] != null && _board[x, y].GetComponent<Enemy>() != null)
                    count++;
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
            if (_board[x, y] == null)
            {
                _board[x, y] = enemyManager.CreateEnemy(x, y);
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
                if (_board[x, y] && _board[x, y].GetComponent<Enemy>() != null)
                    _board[x, y] = null;
            }
        }
    }

    // explode all auxiliary bomb and push enemy
    public void ExplodeAuxiliaryBomb()
    {
        while (_auxiliaryBombs.Count > 0)
        {
            Vector2Int bombCoordinate = _auxiliaryBombs[0];
            _auxiliaryBombs.RemoveAt(0);
            int x = bombCoordinate.x;
            int y = bombCoordinate.y;
            GameObject bomb = _board[x, y];

            bomb.GetComponent<AuxiliaryBomb>().Explode();

            Knockback(x, y);
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
    public int getKnockbackDistance()
    {
        return _knockbackDistance;
    }

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

    public Color GetAuxiliaryBombColor()
    {
        return _auxiliaryBombColor;
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

    public int GetInitialAuxiliaryBomb()
    {
        return _initialAuxiliaryBomb;
    }

    public string GetBoardSpritePath()
    {
        return _boardSpritePath;
    }



    // find enemy nearby (x,y) and push them (call Enemy.Knockback)
    private void Knockback(int x, int y)
    {
        //Up
        if (_board[x, Mod(y + 1, _height)]
                           && _board[x, Mod(y + 1, _height)].GetComponent<Enemy>() != null)
        {
            _board[x, Mod(y + 1, _height)].GetComponent<Enemy>().Knockback(Direction.Up, _knockbackDistance);
            ReflectMoveInBoard(x, Mod(y + 1, _height), Direction.Up, _knockbackDistance);
        }
        //Down
        if (_board[x, Mod(y - 1, _height)]
                  && _board[x, Mod(y - 1, _height)].GetComponent<Enemy>() != null)
        {
            _board[x, Mod(y - 1, _height)].GetComponent<Enemy>().Knockback(Direction.Down, _knockbackDistance);
            ReflectMoveInBoard(x, Mod(y - 1, _height), Direction.Down, _knockbackDistance);
        }
        //Right
        if (_board[Mod(x + 1, _width), y]
                          && _board[Mod(x + 1, _width), y].GetComponent<Enemy>() != null)
        {
            _board[Mod(x + 1, _width), y].GetComponent<Enemy>().Knockback(Direction.Right, _knockbackDistance);
            ReflectMoveInBoard(Mod(x + 1, _width), y, Direction.Right, _knockbackDistance);
        }
        //Left
        if (_board[Mod(x - 1, _width), y]
                  && _board[Mod(x - 1, _width), y].GetComponent<Enemy>() != null)
        {
            _board[Mod(x - 1, _width), y].GetComponent<Enemy>().Knockback(Direction.Left, _knockbackDistance);
            ReflectMoveInBoard(Mod(x - 1, _width), y, Direction.Left, _knockbackDistance);
        }
        //UpRight
        if (_board[Mod(x + 1, _width), Mod(y + 1, _height)]
            && _board[Mod(x + 1, _width), Mod(y + 1, _height)].GetComponent<Enemy>() != null)
        {
            _board[Mod(x + 1, _width), Mod(y + 1, _height)].GetComponent<Enemy>().Knockback(Direction.UpRight, _knockbackDistance);
            ReflectMoveInBoard(Mod(x + 1, _width), Mod(y + 1, _height), Direction.UpRight, _knockbackDistance);
        }
        //DownLeft
        if (_board[Mod(x - 1, _width), Mod(y - 1, _height)]
            && _board[Mod(x - 1, _width), Mod(y - 1, _height)].GetComponent<Enemy>() != null)
        {
            _board[Mod(x - 1, _width), Mod(y - 1, _height)].GetComponent<Enemy>().Knockback(Direction.DownLeft, _knockbackDistance);
            ReflectMoveInBoard(Mod(x - 1, _width), Mod(y - 1, _height), Direction.DownLeft, _knockbackDistance);
        }
        //DownRight
        if (_board[Mod(x + 1, _width), Mod(y - 1, _height)]
            && _board[Mod(x + 1, _width), Mod(y - 1, _height)].GetComponent<Enemy>() != null)
        {
            _board[Mod(x + 1, _width), Mod(y - 1, _height)].GetComponent<Enemy>().Knockback(Direction.DownRight, _knockbackDistance);
            ReflectMoveInBoard(Mod(x + 1, _width), Mod(y - 1, _height), Direction.DownRight, _knockbackDistance);
        }
        //UpLeft
        if (_board[Mod(x - 1, _width), Mod(y + 1, _height)]
            && _board[Mod(x - 1, _width), Mod(y + 1, _height)].GetComponent<Enemy>() != null)
        {
            _board[Mod(x - 1, _width), Mod(y + 1, _height)].GetComponent<Enemy>().Knockback(Direction.UpLeft, _knockbackDistance);
            ReflectMoveInBoard(Mod(x - 1, _width), Mod(y + 1, _height), Direction.UpLeft, _knockbackDistance);
        }
    }
    
    // update board when object movement occurs
    private void ReflectMoveInBoard(int x, int y, Direction direction, int distance)
    {
        GameObject obj = _board[x, y];
        if (obj == null)
            return;

        Vector3 target = GetWrappedTarget(direction, distance, new Vector3(x, y, 0), 0, _width, 0, _height);
        if (_board[(int)target.x, (int)target.y] != null)
        {
            // collision
        }

        _board[(int)target.x, (int)target.y] = obj;
        _board[x, y] = null;

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
    private void MouseClickProcess()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        int x = GlobalToGridX(worldPos.x);
        int y = GlobalToGridY(worldPos.y);

        if (x >= 0 && x < _width && y >= 0 && y < _height
            && _board[x, y] == null)
        {
            CreateAuxiliaryBomb(x, y);
        }
    }
    
    // if (x,y) is empty, fill the cell by an auxiliary bomb
    private void CreateAuxiliaryBomb(int x, int y)
    {
        if (_board[x, y] != null)
            return;

        _board[x, y] = bombManager.PlantAuxiliaryBomb(x, y);
        _auxiliaryBombs.Add(new Vector2Int(x, y));
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
        _width = differentData.width;
        _height = differentData.height;
        _enemyNumber = differentData.enemyNumber;
        _initialAuxiliaryBomb = differentData.initialAuxiliaryBomb;
        _boardSpritePath = differentData.boardSpritePath;
    }
}
