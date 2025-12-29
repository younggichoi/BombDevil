using UnityEngine;
using System.Collections.Generic;

public partial class GameManager : MonoBehaviour
{
    // Wall representation: true if wall exists at (x, y)
    // Initialize and manage this array elsewhere as needed

    // Checks if the enemy at (x, y) is on a teleporter and teleports it to the other teleporter
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
        List<(int targetX, int targetY, GameObject obj)> target = new List<(int, int, GameObject)>();
        for (int r = 1; r <= range; r++)
        {
            for(int dx = -r; dx <= r; dx++)
            {
                for(int dy = -r; dy <= r; dy++)
                {
                    int targetX = Mod(x + dx, _width);
                    int targetY = Mod(y + dy, _height);
                    foreach (var obj in _board[targetX, targetY])
                    {
                        if (obj != null && obj.GetComponent<Enemy>() != null)
                        {
                            target.Add((targetX, targetY, obj));
                            Debug.Log($"Found enemy at ({targetX}, {targetY}) in range {r} of ({x}, {y})");
                        }
                    }
                }
            }
        }
        return target;
    }

    // Returns the vector from (fromX, fromY) to (toX, toY) in board units (not normalized)
    private Vector2Int GetDirectionAndDistance(int fromX, int fromY, int toX, int toY)
    {
        int dx = toX - fromX;
        int dy = toY - fromY;
        // Handle wrapping
        if(Mathf.Abs(dx) > _width / 2)
        {
            if(dx > 0)
                dx -= _width;
            else
                dx += _width;
        }
        if(Mathf.Abs(dy) > _height / 2)
        {
            if(dy > 0)
                dy -= _height;
            else
                dy += _height;
        }
        return new Vector2Int(dx, dy);
    }

    private void NormalBomb(int x, int y, int range, int distance)
    {
        List<(int targetX, int targetY, GameObject obj)> target = FindEnemiesInRange(x, y, range);
        foreach (var (targetX, targetY, obj) in target)
        {
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Bombs can knock back stunned enemies
                Vector2Int dirAndDist = GetDirectionAndDistance(x, y, targetX, targetY) * distance;
                HandleMoveInBoard(targetX, targetY, obj, dirAndDist); // 1 step in that direction for board update
            }
        }
    }

    /*private void PinkBomb(int x, int y, int range)
    {
        List<(int targetX, int targetY, GameObject obj)> target = FindEnemiesInRange(x, y, range);
        foreach (var (targetX, targetY, obj) in target)
        {
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                Vector2Int dirAndDist = GetDirectionAndDistance(x, y, targetX, targetY);
                Debug.Log($"PinkBomb knocking back enemy at ({targetX}, {targetY}) by {dirAndDist}");
                enemy.Knockback(dirAndDist);
                ReflectMoveInBoard(targetX, targetY, obj, dirAndDist); // 1 step in that direction for board update
            }
        }
    }*/

    private void SkyblueBomb(int x, int y, int range)
    {
        List<(int targetX, int targetY, GameObject obj)> target = FindEnemiesInRange(x, y, range);
        foreach (var (targetX, targetY, obj) in target)
        {
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                Vector2Int dirAndDist = GetDirectionAndDistance(x, y, targetX, targetY);
                Vector2Int knockbackDir;
                if(dirAndDist.x == 0 || dirAndDist.y == 0)
                {
                    return; // If straight line, do not knockback
                }
                else
                {
                    if(Mathf.Abs(dirAndDist.x) < Mathf.Abs(dirAndDist.y))
                    {
                        knockbackDir = new Vector2Int(dirAndDist.x > 0 ? 1 : -1, 0);
                    }
                    else if(Mathf.Abs(dirAndDist.x) > Mathf.Abs(dirAndDist.y))
                    {
                        knockbackDir = new Vector2Int(0, dirAndDist.y > 0 ? 1 : -1);
                    }
                    else
                    {
                        knockbackDir = new Vector2Int(dirAndDist.x > 0 ? 1 : -1, dirAndDist.y > 0 ? 1 : -1);
                    }
                    HandleMoveInBoard(targetX, targetY, obj, knockbackDir); // 1 step in that direction for board update
                }
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
    private void HandleMoveInBoard(int x, int y, GameObject obj, Vector2Int directionAndDistance)
    {
        if (obj == null)
            return;
        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy == null)
            return;

        // Remove enemy from all cells to ensure uniqueness
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                if (_board[i, j].Contains(obj))
                {
                    _board[i, j].Remove(obj);
                }
            }
        }

        Vector2Int? entryTeleporterPos;
        Vector2Int? exitTeleporterPos = HandleTeleporter(x, y, obj, directionAndDistance, out entryTeleporterPos);

        if (exitTeleporterPos.HasValue && entryTeleporterPos.HasValue)
        {
            // The enemy's path is interrupted by a teleporter.
            // 1. Calculate the movement vector to reach the teleporter entry.
            Vector2Int moveToEntry = GetDirectionAndDistance(x, y, entryTeleporterPos.Value.x, entryTeleporterPos.Value.y);
            
            // 2. Determine the remaining movement after reaching the teleporter.
            Vector2Int remainingMove = directionAndDistance - moveToEntry;

            // 3. Calculate the final grid position after exiting the second teleporter and completing the remaining move.
            Vector2Int finalPos = new Vector2Int(
                Mod(exitTeleporterPos.Value.x + remainingMove.x, _width),
                Mod(exitTeleporterPos.Value.y + remainingMove.y, _height)
            );

            // 4. Calculate the total displacement vector from the start to the final position, accounting for board wrap.
            Vector2Int totalMove = GetDirectionAndDistance(x, y, finalPos.x, finalPos.y);

            // 5. Apply a single knockback for the entire calculated path.
            enemy.Knockback(totalMove);
            _board[finalPos.x, finalPos.y].Add(obj);
            Debug.Log($"Enemy at ({x}, {y}) path via teleporter. Final position: {finalPos}. Total move vector: {totalMove}");
            return;
        }
        else {
            // No teleporter in path, perform a standard move.
            Debug.Log($"Enemy at ({x}, {y}) moving by {directionAndDistance}");
            enemy.Knockback(directionAndDistance);
            _board[Mod(x + directionAndDistance.x, _width), Mod(y + directionAndDistance.y, _height)].Add(obj);
        }
    }

    // Checks if a teleporter exists at the given position
    private bool IsTeleporterAt(Vector2Int pos)
    {
        // Example: Assume teleporters are stored in a List<Vector2Int> _teleporters
        // You may need to adjust this logic to match your actual teleporter storage
        if (_teleporters == null) return false;
        return _teleporters.Contains(pos);
    }

    // Finds the other teleporter position, given one teleporter's position
    private Vector2Int? FindOtherTeleporter(Vector2Int currentTeleporter)
    {
        // Example: Assume only two teleporters exist
        if (_teleporters == null || _teleporters.Count != 2) return null;
        if (_teleporters[0] == currentTeleporter)
            return _teleporters[1];
        if (_teleporters[1] == currentTeleporter)
            return _teleporters[0];
        return null;
    }

    private Vector2Int? HandleTeleporter (int x, int y, GameObject obj, Vector2Int directionAndDistance, out Vector2Int? entryPoint)
    {
        entryPoint = null;
        int steps = Mathf.Max(Mathf.Abs(directionAndDistance.x), Mathf.Abs(directionAndDistance.y));
        for (int step = 1; step <= steps; step++)
        {
            int currentX = Mod(x + directionAndDistance.x * step / steps, _width);
            int currentY = Mod(y + directionAndDistance.y * step / steps, _height);
            if (IsTeleporterAt(new Vector2Int(currentX, currentY)))
            {
                entryPoint = new Vector2Int(currentX, currentY);
                Vector2Int? otherTele = FindOtherTeleporter(new Vector2Int(currentX, currentY));
                if (otherTele.HasValue)
                {
                    return new Vector2Int(otherTele.Value.x, otherTele.Value.y);
                }
            }
        }
        return null;
    }
    
}
