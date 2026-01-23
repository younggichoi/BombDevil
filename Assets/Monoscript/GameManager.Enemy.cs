using System;
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
                GameObject enemy = EnemyManager.CreateEnemy(x, y);
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

        // for (int r = 1; r <= range; r++)
        // {
        //     for(int dx = -r; dx <= r; dx++)
        //     {
        //         for(int dy = -r; dy <= r; dy++)
        //         {
        //             int targetX = Mod(x + dx, _width);
        //             int targetY = Mod(y + dy, _height);
        //             foreach (var obj in _board[targetX, targetY])
        //             {
        //                 if (obj != null && obj.GetComponent<Enemy>() != null)
        //                 {
        //                     target.Add((targetX, targetY, obj));
        //                 }
        //             }
        //         }
        //     }
        // }

        // optimization: remove redundant loop
        for(int dx = -range; dx <= range; dx++)
        {
            for(int dy = -range; dy <= range; dy++)
            {
                int targetX = Mod(x + dx, _width);
                int targetY = Mod(y + dy, _height);
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
                HandleMoveInBoard(targetX, targetY, obj, dirAndDist, true); // 1 step in that direction for board update
            }
        }
    }

    // SkyblueBomb: Creates cross-shaped axes through bomb center
    // Enemies NOT on the axes move 1 cell AWAY from the nearest axis
    private void SkyblueBomb(int bombX, int bombY, int range)
    {
        // First, collect all enemies and their knockback directions
        List<(int ex, int ey, GameObject obj, Vector2Int knockbackDir)> enemiesToMove = 
            new List<(int, int, GameObject, Vector2Int)>();
        
        for (int ex = 0; ex < _width; ex++)
        {
            for (int ey = 0; ey < _height; ey++)
            {
                foreach (var obj in _board[ex, ey])
                {
                    if (obj == null) continue;
                    Enemy enemy = obj.GetComponent<Enemy>();
                    if (enemy == null) continue;
                    
                    // Calculate distance to axes (with wrapping)
                    int dx = GetWrappedDistance(ex, bombX, _width);  // Distance to vertical axis
                    int dy = GetWrappedDistance(ey, bombY, _height); // Distance to horizontal axis
                    
                    // Skip if enemy is on either axis
                    if (dx == 0 || dy == 0) continue;
                    
                    Vector2Int knockbackDir;
                    
                    // Move away from the NEAREST axis
                    knockbackDir = Vector2Int.zero;
                    if (Mathf.Abs(dx) == 1)
                    {
                        knockbackDir += new Vector2Int(dx, 0);
                    }
                    if (Mathf.Abs(dy) == 1)
                    {
                        knockbackDir += new Vector2Int(0, dy);
                    }
                    
                    Debug.Log($"SkyblueBomb: Enemy at ({ex}, {ey}), dx={dx}, dy={dy}, knockback={knockbackDir}");
                    enemiesToMove.Add((ex, ey, obj, knockbackDir));
                }
            }
        }
        
        // Then, process all knockbacks (separate loop to avoid collection modification during iteration)
        foreach (var (ex, ey, obj, knockbackDir) in enemiesToMove)
        {
            HandleMoveInBoard(ex, ey, obj, knockbackDir, true);
        }
    }
    
    // Helper: Calculate shortest distance considering board wrapping
    private int GetWrappedDistance(int from, int to, int size)
    {
        int direct = from - to;
        if (Mathf.Abs(direct) > size / 2)
        {
            // Wrap is shorter
            if (direct > 0)
                direct -= size;
            else
                direct += size;
        }
        return direct;
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
    private void HandleMoveInBoard(int x, int y, GameObject obj, Vector2Int directionAndDistance, bool isKnockback)
    {
        if (obj == null)
            return;
        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy == null)
            return;

        // // Remove enemy from all cells to ensure uniqueness
        // for (int i = 0; i < _width; i++)
        // {
        //     for (int j = 0; j < _height; j++)
        //     {
        //         if (_board[i, j].Contains(obj))
        //         {
        //             _board[i, j].Remove(obj);
        //         }
        //     }
        // }

        // optimization: remove enemy from current cell
        _board[x, y].Remove(obj);
        
        //Handle teleporters and walls
        Vector2Int? entryTeleporterPos = null;
        Vector2Int? exitTeleporterPos = null;
        int steps = Mathf.Max(Mathf.Abs(directionAndDistance.x), Mathf.Abs(directionAndDistance.y));
        Vector2Int normalizedDir = new Vector2Int(0, 0);
        if (steps > 0)
        {
            normalizedDir = directionAndDistance / steps;
        }
        for (int step = 1; step <= steps; step++)
        {
            int currentX = Mod(x + normalizedDir.x * step, _width);
            int currentY = Mod(y + normalizedDir.y * step, _height);
            if (IsTeleporterAt(new Vector2Int(currentX, currentY)))
            {
                entryTeleporterPos = new Vector2Int(currentX, currentY);
                Vector2Int? otherTele = FindOtherTeleporter(new Vector2Int(currentX, currentY));
                if (otherTele.HasValue)
                {
                    exitTeleporterPos = new Vector2Int(otherTele.Value.x, otherTele.Value.y);
                }
            }
            if(HasObjectAt(currentX, currentY, typeof(Wall)))
            {
                Vector2Int finalDirection = new Vector2Int(currentX - x - normalizedDir.x, currentY - y - normalizedDir.y);
                enemy.Knockback(finalDirection);
                _board[Mod(x + finalDirection.x, _width), Mod(y + finalDirection.y, _height)].Add(obj);
                if (!isKnockback) //그냥 이동하는 경우 방향을 바꿈
                {
                    enemy.SetDirection(-normalizedDir);
                }
                return;
            }
        }
        
        if (exitTeleporterPos.HasValue && entryTeleporterPos.HasValue)
        {
            // The enemy's path is interrupted by a teleporter.
            // Build path segments for visual teleporter traversal
            
            // 1. Calculate the movement vector to reach the teleporter entry.
            Vector2Int moveToEntry = GetDirectionAndDistance(x, y, entryTeleporterPos.Value.x, entryTeleporterPos.Value.y);
            
            // 2. Determine the remaining movement after reaching the teleporter.
            Vector2Int remainingMove = directionAndDistance - moveToEntry;

            // 3. Calculate the final grid position after exiting the second teleporter and completing the remaining move.
            Vector2Int finalPos = new Vector2Int(
                Mod(exitTeleporterPos.Value.x + remainingMove.x, _width),
                Mod(exitTeleporterPos.Value.y + remainingMove.y, _height)
            );

            // 4. Build path segments for visual animation
            var path = new System.Collections.Generic.List<PathSegment>();
            
            // Segment 1: Move from current position to teleporter entry
            if (moveToEntry != Vector2Int.zero)
            {
                path.Add(new PathSegment(moveToEntry, false, null));
            }
            
            // Segment 2: Instant teleport to exit teleporter
            path.Add(new PathSegment(Vector2Int.zero, true, exitTeleporterPos.Value));
            
            // Segment 3: Move from exit teleporter to final position  
            if (remainingMove != Vector2Int.zero)
            {
                path.Add(new PathSegment(remainingMove, false, null));
            }

            // 5. Apply path-based knockback for visual teleporter traversal
            enemy.KnockbackPath(path);
            _board[finalPos.x, finalPos.y].Add(obj);
            Debug.Log($"Enemy at ({x}, {y}) path via teleporter. Entry: {entryTeleporterPos.Value}, Exit: {exitTeleporterPos.Value}, Final: {finalPos}");
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

    /*private Vector2Int? HandleTeleporter (int x, int y, GameObject obj, Vector2Int directionAndDistance, out Vector2Int? entryPoint)
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

    private Vector2Int? HandleWall(int x, int y, GameObject obj, Vector2Int directionAndDistance,
        out Vector2Int? stopPoint)
    {
        stopPoint = null;
        int steps = Mathf.Max(Mathf.Abs(directionAndDistance.x), Mathf.Abs(directionAndDistance.y));
        for (int step = 1; step <= steps; step++)
        {
            int currentX = Mod(x + directionAndDistance.x * step / steps, _width);
            int currentY = Mod(y + directionAndDistance.y * step / steps, _height);
            if (HasObjectAt(currentX, currentY, typeof(Wall)))
            {
                stopPoint = new Vector2Int(currentX - directionAndDistance.x * step / steps,
                    currentY - directionAndDistance.y * step / steps);
            }
        }
    }*/
}
