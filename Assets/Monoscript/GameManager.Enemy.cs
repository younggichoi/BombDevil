using UnityEngine;
using System.Collections.Generic;

public partial class GameManager : MonoBehaviour
{
    public void CreateEnemy()
    {
        DeleteEnemy();
        int currentEnemy = 0;
        while (currentEnemy < _enemyNumber)
        {
            int x = UnityEngine.Random.Range(0, _width);
            int y = UnityEngine.Random.Range(0, _height);
            if (_board[x, y].Count == 0)
            {
                GameObject enemy = enemyManager.CreateEnemy(x, y);
                _board[x, y].Add(enemy);
                currentEnemy++;
            }
        }
    }

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

    private List<(int targetX, int targetY, GameObject obj)> FindEnemiesInRange(int x, int y, int range)
    {
        int[,] dirOffsets = { {0, 1}, {0, -1}, {1, 0}, {-1, 0}, {1, 1}, {-1, 1}, {1, -1}, {-1, -1} };
        List<(int targetX, int targetY, GameObject obj)> target = new List<(int, int, GameObject)>();
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
                        target.Add((targetX, targetY, obj));
                    }
                }
            }
        }
        return target;
    }

    // Returns the vector from (fromX, fromY) to (toX, toY) in board units (not normalized)
    private Vector2 GetDirectionAndDistance(int fromX, int fromY, int toX, int toY)
    {
        int dx = toX - fromX;
        int dy = toY - fromY;
        return new Vector2(dx, dy);
    }

    private void Knockback(int x, int y, int range, int distance)
    {
        List<(int targetX, int targetY, GameObject obj)> target = FindEnemiesInRange(x, y, range);
        foreach (var (targetX, targetY, obj) in target)
        {
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                Vector2 dirAndDist = GetDirectionAndDistance(x, y, targetX, targetY) * distance;
                enemy.Knockback(dirAndDist);
                ReflectMoveInBoard(targetX, targetY, obj, dirAndDist); // 1 step in that direction for board update
            }
        }
    }

    private void PinkBomb(int x, int y, int range)
    {
        List<(int targetX, int targetY, GameObject obj)> target = FindEnemiesInRange(x, y, range);
        foreach (var (targetX, targetY, obj) in target)
        {
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                Vector2 dirAndDist = GetDirectionAndDistance(x, y, targetX, targetY);
                enemy.Walk(dirAndDist);
                ReflectMoveInBoard(targetX, targetY, obj, dirAndDist); // 1 step in that direction for board update
            }
        }
    }

    private void KillEnemiesInRange(int x, int y, int range)
    {
        List<(int targetX, int targetY, GameObject obj)> target = FindEnemiesInRange(x, y, range);
        foreach (var (targetX, targetY, obj) in target)
        {
            _board[targetX, targetY].Remove(obj);
            Destroy(obj);
            _realBombKillCount++;
            Debug.Log($"RealBomb killed enemy at ({targetX}, {targetY}). Total kills: {_realBombKillCount}/{_totalEnemyCount}");
        }
    }

    // directionAndDistance: the vector from (x, y) to the new cell (in board units)
    private void ReflectMoveInBoard(int x, int y, GameObject obj, Vector2 directionAndDistance)
    {
        if (obj == null)
            return;
        Vector3 target = GetWrappedTarget(directionAndDistance, new Vector3(x, y, 0), 0, _width, 0, _height);
        _board[x, y].Remove(obj);
        _board[(int)target.x, (int)target.y].Add(obj);
    }
}
