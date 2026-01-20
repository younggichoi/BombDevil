using UnityEngine;
using Entity;
using System.Collections.Generic;

public partial class GameManager : MonoBehaviour
{
    public float GetMaxX() => _maxX;
    public float GetMinX() => _minX;
    public float GetMaxY() => _maxY;
    public float GetMinY() => _minY;
    public float getWalkDuration() => _walkDuration;
    public float getKnockbackDuration() => _knockbackDuration;
    public Color GetEnemyColor() => _enemyColor;
    public int GetWidth() => _width;
    public int GetHeight() => _height;
    public int GetEnemyNumber() => _enemyNumber;
    public int GetInitialBombCount(BombType bombType)
    {
        switch (bombType)
        {
            case BombType.FirstBomb:
                return _initial1stBomb;
            case BombType.SecondBomb:
                return _initial2ndBomb;
            case BombType.ThirdBomb:
                return _initial3rdBomb;
            case BombType.FourthBomb:
                return _initial4thBomb;
            case BombType.FifthBomb:
                return _initial5thBomb;
            case BombType.SixthBomb:
                return _initial6thBomb;
            case BombType.SkyblueBomb:
                return _initialSkyblueBomb;
            default:
                return 0;
        }
    }

    public int GetInitialItemCount(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Teleporter:
                Debug.Log($"Initial Teleporter Count: {_initialTeleporter}");
                return _initialTeleporter;
            case ItemType.Megaphone:
                Debug.Log($"Initial Megaphone Count: {_initialMegaphone}");
                return _initialMegaphone;
            default:
                return 0;
        }
    }

    public int GetRemainingBombCount(BombType bombType)
    {
        return BombManager.GetLeftoverBomb(bombType);
    }

    public int GetRemainingItemCount(ItemType itemType)
    {
        return ItemManager.GetLeftoverItem(itemType);
    }
    
    public string GetBoardSpritePath() => _boardSpritePath;
    public GameState GetCurrentState() => _currentState;

    private static float Mod(float x, int m) => (x % m + m) % m;
    private static float Mod(float x, float m) => (x % m + m) % m;
    private static int Mod(int x, int m) => (x % m + m) % m;
    
    // Check if a bomb exists at the given cell (ignores enemies)
    private bool HasBombAt(int x, int y)
    {
        return HasObjectAt(x, y, typeof(AuxiliaryBomb)) || HasObjectAt(x, y, typeof(RealBomb));
    }

    private bool HasItemAt(int x, int y)
    {
        return HasObjectAt(x, y, typeof(Item));
    }

    private bool HasObjectAt(int x, int y, System.Type componentType)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height) return false;
        foreach (var obj in _board[x, y])
        {
            if (obj == null) continue;
            if (obj.GetComponent(componentType) != null) return true;
        }
        return false;
    }

    // Get all objects at a specific grid position (for collision detection)
    public List<GameObject> GetObjectsAt(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return new List<GameObject>();
        
        return _board[x, y];
    }

    // Vector2 version: directionAndDistance is in board units (not normalized)
    private static Vector3 GetTarget(Vector2 directionAndDistance, Vector3 start, float cellSize = 1f)
    {
        Vector2 move = directionAndDistance * cellSize;
        return start + new Vector3(move.x, move.y, 0);
    }

    private static Vector3 GetWrappedTarget(Vector2 directionAndDistance, Vector3 start,
        float minX = float.NegativeInfinity, float maxX = float.PositiveInfinity,
        float minY = float.NegativeInfinity, float maxY = float.PositiveInfinity,
        float minZ = float.NegativeInfinity, float maxZ = float.PositiveInfinity,
        float cellSize = 1f)
    {
        Vector3 result = GetTarget(directionAndDistance, start, cellSize);
        if (result.x < minX || result.x >= maxX)
            result.x = Mod(result.x - minX, maxX - minX) + minX;
        if (result.y < minY || result.y >= maxY)
            result.y = Mod(result.y - minY, maxY - minY) + minY;
        if (result.z < minZ || result.z >= maxZ)
            result.z = Mod(result.z - minZ, maxZ - minZ) + minZ;
        return result;
    }

    private void CheckGameState()
    {
        // Only check for win here. Lose condition is handled after all bombs have exploded in the turn sequence.
        if (GetEnemyCount() == 0 && _realBombKillCount == _totalEnemyCount)
        {
            SetGameState(GameState.Win);
            return;
        }
    }

    private void SetGameState(GameState newState)
    {
        if (_currentState == newState)
            return;
        _currentState = newState;
        OnGameStateChanged?.Invoke(_currentState);
        Debug.Log($"Game State Changed: {_currentState}");
    }
}
