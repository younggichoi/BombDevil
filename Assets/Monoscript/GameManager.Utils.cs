using UnityEngine;
using Entity;

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
    public string GetBoardSpritePath() => _boardSpritePath;
    public GameState GetCurrentState() => _currentState;

    private static float Mod(float x, int m) => (x % m + m) % m;
    private static float Mod(float x, float m) => (x % m + m) % m;
    private static int Mod(int x, int m) => (x % m + m) % m;


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
        if (GetEnemyCount() == 0 && _realBombKillCount == _totalEnemyCount)
        {
            SetGameState(GameState.Win);
            return;
        }
        bool noAuxiliaryBombs = bombManager.GetTotalLeftoverBombs() <= 0 && bombManager.GetPlantedAuxiliaryBombCount() <= 0;
        bool noRealBombs = !bombManager.IsRealBombAvailable() && bombManager.GetPlantedRealBombCount() <= 0;
        if (noAuxiliaryBombs && noRealBombs && GetEnemyCount() > 0)
        {
            SetGameState(GameState.Lose);
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
