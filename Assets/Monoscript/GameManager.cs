using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class GameManager : MonoBehaviour
{
    // singleton
    public static GameManager Instance;
    
    // setting option
    public int width;
    public int height;
    public int enemyNumber;
    public int knockbackDistance;
    public float walkDuration;
    public float knockbackDuration;
    public Color enemyColor;
    public int initialAuxiliaryBomb;
    public Color auxiliaryBombColor;
    public Color tileColor1;
    public Color tileColor2;

    public EnemyManager enemyManager;
    public BombManager bombManager;
    
    // scoping board situation
    private GameObject[,] _board;
    
    // tracking auxiliary bomb order
    private List<Vector2Int> _auxiliaryBombs;
    
    // boundary coordinate
    private static float _minX, _minY, _maxX, _maxY;

    void Awake()
    {
        Instance = this;
        _board = new GameObject[width, height];
        _auxiliaryBombs = new List<Vector2Int>();
        _minX = -width / 2.0f;
        _maxX = width / 2.0f;
        _minY = -height / 2.0f;
        _maxY = height / 2.0f;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            MouseClickProcess();
    }

    // delete previous enemy
    // and create enemy on the board in random space
    public void CreateEnemy()
    {
        DeleteEnemy();
        
        int currentEnemy = 0;
        while (currentEnemy < enemyNumber)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
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
    public static float GetMaxX()
    {
        return _maxX;
    }
    
    public static float GetMinX()
    {
        return _minX;
    }
    
    public static float GetMaxY()
    {
        return _maxY;
    }
    
    public static float GetMinY()
    {
        return _minY;
    }

    // find enemy nearby (x,y) and push them (call Enemy.Knockback)
    private void Knockback(int x, int y)
    {
        //Up
        if (_board[x, Mod(y + 1, height)]
                           && _board[x, Mod(y + 1, height)].GetComponent<Enemy>() != null)
        {
            _board[x, Mod(y + 1, height)].GetComponent<Enemy>().Knockback(Direction.Up, knockbackDistance);
            ReflectMoveInBoard(x, Mod(y + 1, height), Direction.Up, knockbackDistance);
        }
        //Down
        if (_board[x, Mod(y - 1, height)]
                  && _board[x, Mod(y - 1, height)].GetComponent<Enemy>() != null)
        {
            _board[x, Mod(y - 1, height)].GetComponent<Enemy>().Knockback(Direction.Down, knockbackDistance);
            ReflectMoveInBoard(x, Mod(y - 1, height), Direction.Down, knockbackDistance);
        }
        //Right
        if (_board[Mod(x + 1, width), y]
                          && _board[Mod(x + 1, width), y].GetComponent<Enemy>() != null)
        {
            _board[Mod(x + 1, width), y].GetComponent<Enemy>().Knockback(Direction.Right, knockbackDistance);
            ReflectMoveInBoard(Mod(x + 1, width), y, Direction.Right, knockbackDistance);
        }
        //Left
        if (_board[Mod(x - 1, width), y]
                  && _board[Mod(x - 1, width), y].GetComponent<Enemy>() != null)
        {
            _board[Mod(x - 1, width), y].GetComponent<Enemy>().Knockback(Direction.Left, knockbackDistance);
            ReflectMoveInBoard(Mod(x - 1, width), y, Direction.Left, knockbackDistance);
        }
        //UpRight
        if (_board[Mod(x + 1, width), Mod(y + 1, height)]
            && _board[Mod(x + 1, width), Mod(y + 1, height)].GetComponent<Enemy>() != null)
        {
            _board[Mod(x + 1, width), Mod(y + 1, height)].GetComponent<Enemy>().Knockback(Direction.UpRight, knockbackDistance);
            ReflectMoveInBoard(Mod(x + 1, width), Mod(y + 1, height), Direction.UpRight, knockbackDistance);
        }
        //DownLeft
        if (_board[Mod(x - 1, width), Mod(y - 1, height)]
            && _board[Mod(x - 1, width), Mod(y - 1, height)].GetComponent<Enemy>() != null)
        {
            _board[Mod(x - 1, width), Mod(y - 1, height)].GetComponent<Enemy>().Knockback(Direction.DownLeft, knockbackDistance);
            ReflectMoveInBoard(Mod(x - 1, width), Mod(y - 1, height), Direction.DownLeft, knockbackDistance);
        }
        //DownRight
        if (_board[Mod(x + 1, width), Mod(y - 1, height)]
            && _board[Mod(x + 1, width), Mod(y - 1, height)].GetComponent<Enemy>() != null)
        {
            _board[Mod(x + 1, width), Mod(y - 1, height)].GetComponent<Enemy>().Knockback(Direction.DownRight, knockbackDistance);
            ReflectMoveInBoard(Mod(x + 1, width), Mod(y - 1, height), Direction.DownRight, knockbackDistance);
        }
        //UpLeft
        if (_board[Mod(x - 1, width), Mod(y + 1, height)]
            && _board[Mod(x - 1, width), Mod(y + 1, height)].GetComponent<Enemy>() != null)
        {
            _board[Mod(x - 1, width), Mod(y + 1, height)].GetComponent<Enemy>().Knockback(Direction.UpLeft, knockbackDistance);
            ReflectMoveInBoard(Mod(x - 1, width), Mod(y + 1, height), Direction.UpLeft, knockbackDistance);
        }
    }
    
    // update board when object movement occurs
    private void ReflectMoveInBoard(int x, int y, Direction direction, int distance)
    {
        GameObject obj = _board[x, y];
        if (obj == null)
            return;

        Vector3 target = GetWrappedTarget(direction, distance, new Vector3(x, y, 0), 0, width, 0, height);
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

        if (x >= 0 && x < width && y >= 0 && y < height
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
    
    // global coordinate -> board coordinate
    private int GlobalToGridX(float x)
    {
        return Mathf.FloorToInt(x + width / 2f);
    }
    
    private int GlobalToGridY(float y)
    {   
        return Mathf.FloorToInt(y + height / 2f);
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
}
