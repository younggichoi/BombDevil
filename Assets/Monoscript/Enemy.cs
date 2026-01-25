using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Path segment for multi-step movement (e.g., through teleporters)
public struct PathSegment
{
    public Vector2Int move;         // Movement vector (grid units)
    public bool isInstant;          // If true, instantly teleport to teleportTo position
    public Vector2Int? teleportTo;  // Destination grid position for instant teleport

    public PathSegment(Vector2Int move, bool isInstant = false, Vector2Int? teleportTo = null)
    {
        this.move = move;
        this.isInstant = isInstant;
        this.teleportTo = teleportTo;
    }
}

public class Enemy : MonoBehaviour
{
    private static int _nextId = 1;
    public int EnemyId { get; private set; }
    private GameManager _gameManager;
    // motion duration (get from GameManager)
    private float _walkDuration;
    private float _knockbackDuration;
    // cell size for scaling movement
    private float _cellSize;
    // direction attribute for this enemy (as Vector2Int)
    private Vector2Int _moveDirection;
    // boundary coordinate (get from BoardManager)
    private float _minX, _maxX, _minY, _maxY;

    // Stun state
    private bool _isStunned = false;
    public bool IsStunned => _isStunned;
    private Sprite _stunnedSprite;

    // initializing internal attribute
    public void Initialize(Sprite sprite, Sprite stunnedSprite, int? forcedId = null)
    {
        var gameManager = GameService.Get<GameManager>();
        var boardManager = GameService.Get<BoardManager>();

        _walkDuration = gameManager.getWalkDuration();
        _knockbackDuration = gameManager.getKnockbackDuration();
        _cellSize = boardManager.GetCellSize();
        _stunnedSprite = stunnedSprite;
        _minX = boardManager.GetMinX();
        _maxX = boardManager.GetMaxX();
        _minY = boardManager.GetMinY();
        _maxY = boardManager.GetMaxY();
        _isStunned = false;
        if (forcedId.HasValue) {
            EnemyId = forcedId.Value;
        } else {
            EnemyId = _nextId++;
        }
        // Set sprite if provided
        if (sprite != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = sprite;
            }
        }
    }

    public Vector2Int GetNextPosition()
    {
        var boardManager = GameService.Get<BoardManager>();
        if (boardManager == null) return Vector2Int.zero;

        Vector2Int currentPos = boardManager.WorldToGrid(transform.position);
        Vector2Int nextPos = currentPos + _moveDirection;

        if (!boardManager.IsWithinBounds(nextPos.x, nextPos.y))
        {
            // Wrap around logic
            if (nextPos.x < 0) nextPos.x = boardManager.GetWidth() - 1;
            else if (nextPos.x >= boardManager.GetWidth()) nextPos.x = 0;

            if (nextPos.y < 0) nextPos.y = boardManager.GetHeight() - 1;
            else if (nextPos.y >= boardManager.GetHeight()) nextPos.y = 0;
        }
        return nextPos;
    }

    // Set stunned state
    public void SetStunned(bool stunned)
    {
        // Once stunned, cannot be unstunned
        if (!_isStunned) {
            _isStunned = true;
            GetComponent<Image>().sprite = _stunnedSprite;
            // Hardcoded correction for the size and rotation difference between the normal and stunned sprites
            RectTransform rectTr = GetComponent<RectTransform>();

            rectTr.sizeDelta *= 1.25f;
            rectTr.Rotate(0, 0, 180f);
        }
    }

    // walk API
    // directionAndDistance: the vector from current position to target cell (in board units, not normalized)
    public void Walk(Vector2Int directionAndDistance)
    {
        if (_isStunned) return;
        StartCoroutine(Move(directionAndDistance, _walkDuration));
    }

    // set random direction (Up, Down, Left, Right) and apply rotation
    public void SetRandomDirection()
    {
        if (_isStunned) return;
        Vector2Int[] cardinalDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        int randomIndex = Random.Range(0, cardinalDirections.Length);
        _moveDirection = cardinalDirections[randomIndex];
        // Apply rotation based on direction
        ApplyDirectionRotation();
    }

    // Apply rotation based on move direction
    // Default sprite faces Down (0 degrees)
    private void ApplyDirectionRotation()
    {
        float rotationZ = 0f;
        if (_moveDirection == Vector2Int.down)
            rotationZ = 0f;
        else if (_moveDirection == Vector2Int.up)
            rotationZ = 180f;
        else if (_moveDirection == Vector2Int.left)
            rotationZ = -90f;
        else if (_moveDirection == Vector2Int.right)
            rotationZ = 90f;
        else if (_moveDirection == new Vector2Int(1, 1)) // Up-Right
            rotationZ = 135f;
        else if (_moveDirection == new Vector2Int(-1, 1)) // Up-Left
            rotationZ = -135f;
        else if (_moveDirection == new Vector2Int(-1, -1)) // Down-Left
            rotationZ = -45f;
        else if (_moveDirection == new Vector2Int(1, -1)) // Down-Right
            rotationZ = 45f;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    // get current direction
    public Vector2Int GetMoveDirection()
    {
        return _moveDirection;
    }

    // move in the assigned direction and distance
    public void MoveInDirection()
    {
        if (_isStunned) return;
        Walk(_moveDirection);
    }

    // knockback API
    // directionAndDistance: the vector from current position to target cell (in board units, not normalized)
    public void Knockback(Vector2Int directionAndDistance)
    {
        // Stun is permanent; do not remove stun here
        StartCoroutine(Move(directionAndDistance, _knockbackDuration));
    }

    // Path-based knockback for teleporter traversal
    // Executes each path segment sequentially, showing visual teleporter pass-through
    public void KnockbackPath(List<PathSegment> path)
    {
        StartCoroutine(MoveAlongPath(path, _knockbackDuration));
    }

    // Move along a path of segments (for teleporter traversal)
    private IEnumerator MoveAlongPath(List<PathSegment> path, float totalDuration)
    {
        if (path == null || path.Count == 0)
            yield break;

        // Calculate total movement distance to distribute duration proportionally
        float totalDistance = 0f;
        foreach (var segment in path)
        {
            if (!segment.isInstant)
            {
                totalDistance += Mathf.Max(Mathf.Abs(segment.move.x), Mathf.Abs(segment.move.y));
            }
        }

        foreach (var segment in path)
        {
            if (segment.isInstant && segment.teleportTo.HasValue)
            {
                // Instant teleport: move to exit teleporter position
                var boardManager = GameService.Get<BoardManager>();
                if (boardManager != null)
                {
                    Vector3 teleportPos = boardManager.GridToWorld(segment.teleportTo.Value.x, segment.teleportTo.Value.y);
                    teleportPos.z = transform.position.z;
                    transform.position = teleportPos;
                }
            }
            else if (segment.move != Vector2Int.zero)
            {
                // Normal movement segment
                float segmentDistance = Mathf.Max(Mathf.Abs(segment.move.x), Mathf.Abs(segment.move.y));
                float segmentDuration = totalDistance > 0 ? totalDuration * (segmentDistance / totalDistance) : totalDuration;
                
                Vector3 start = transform.position;
                Vector3 target = GetTarget(segment.move, start, _cellSize);
                float time = 0f;
                while (time < segmentDuration)
                {
                    time += Time.deltaTime;
                    float t = time / segmentDuration;
                    transform.position = LerpWrap(start, target, t, _minX, _maxX, _minY, _maxY);
                    yield return null;
                }
                Vector3 finalPosition = GetWrappedTarget(segment.move, start, _minX, _maxX, _minY, _maxY, _cellSize);
                finalPosition.z = start.z;
                transform.position = finalPosition;
            }
        }
        
        // Check for collision after all movement completes
        CheckAndStunCollision();
    }

    // set direction towards megaphone and apply rotation
    public void SetDirectionTowards(Vector3 targetPosition)
    {
        if (_isStunned) return;

        Vector3 direction = targetPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Snap angle to 8 directions (45-degree increments)
        angle = Mathf.Round(angle / 45f) * 45f;

        if (angle == 0) _moveDirection = Vector2Int.right;
        else if (angle == 45) _moveDirection = new Vector2Int(1, 1);
        else if (angle == 90) _moveDirection = Vector2Int.up;
        else if (angle == 135) _moveDirection = new Vector2Int(-1, 1);
        else if (angle == 180 || angle == -180) _moveDirection = Vector2Int.left;
        else if (angle == -135) _moveDirection = new Vector2Int(-1, -1);
        else if (angle == -90) _moveDirection = Vector2Int.down;
        else if (angle == -45) _moveDirection = new Vector2Int(1, -1);
        
        ApplyDirectionRotation();
    }

    public void SetDirection(Vector2Int direction)
    {
        _moveDirection = direction;
        ApplyDirectionRotation();
    }

    // internal move API
    // directionAndDistance: the vector from current position to target cell (in board units, not normalized)
    private IEnumerator Move(Vector2Int directionAndDistance, float duration)
    {
        Vector3 start = transform.position;
        Vector3 target = GetTarget(directionAndDistance, start, _cellSize);
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            transform.position = LerpWrap(start, target, t, _minX, _maxX, _minY, _maxY);
            yield return null;
        }
        Vector3 finalPosition = GetWrappedTarget(directionAndDistance, start, _minX, _maxX, _minY, _maxY, _cellSize);
        finalPosition.z = start.z;
        transform.position = finalPosition;
        
        // Check for collision after movement completes
        CheckAndStunCollision();
    }

    // It can process boundary condition - wraps position within bounds
    private static Vector3 LerpWrap(Vector3 a, Vector3 b, float t,
        float minX = float.NegativeInfinity, float maxX = float.PositiveInfinity,
        float minY = float.NegativeInfinity, float maxY = float.PositiveInfinity)
    {
        // Simple linear interpolation
        float x = a.x + (b.x - a.x) * t;
        float y = a.y + (b.y - a.y) * t;
        float z = a.z;

        // Wrap x within bounds
        float widthX = maxX - minX;
        if (widthX > 0)
        {
            while (x >= maxX) x -= widthX;
            while (x < minX) x += widthX;
        }

        // Wrap y within bounds
        float widthY = maxY - minY;
        if (widthY > 0)
        {
            while (y >= maxY) y -= widthY;
            while (y < minY) y += widthY;
        }

        return new Vector3(x, y, z);
    }

    // Vector2Int version: directionAndDistance is in board units (not normalized)
    private static Vector3 GetTarget(Vector2Int directionAndDistance, Vector3 start, float cellSize = 1f)
    {
        Vector2 move = new Vector2(directionAndDistance.x, directionAndDistance.y) * cellSize;
        return start + new Vector3(move.x, move.y, 0);
    }

    private static Vector3 GetWrappedTarget(Vector2Int directionAndDistance, Vector3 start,
        float minX = float.NegativeInfinity, float maxX = float.PositiveInfinity,
        float minY = float.NegativeInfinity, float maxY = float.PositiveInfinity,
        float cellSize = 1f)
    {
        Vector3 result = GetTarget(directionAndDistance, start, cellSize);
        if (result.x < minX || result.x >= maxX)
            result.x = Mod(result.x - minX, maxX - minX) + minX;
        if (result.y < minY || result.y >= maxY)
            result.y = Mod(result.y - minY, maxY - minY) + minY;
        return result;
    }

    // Check if multiple enemies are at the same position and stun them
    private void CheckAndStunCollision()
    {
        var boardManager = GameService.Get<BoardManager>();
        var gameManager = GameService.Get<GameManager>();
        
        if (boardManager == null || gameManager == null)
            return;
        
        // Get current grid position
        Vector2Int gridPos = boardManager.WorldToGrid(transform.position);
        
        // Get all enemies at this position from GameManager's board
        var enemies = new List<Enemy>();
        var objectsAtPos = gameManager.GetObjectsAt(gridPos.x, gridPos.y);
        
        if (objectsAtPos != null)
        {
            foreach (var obj in objectsAtPos)
            {
                if (obj != null)
                {
                    Enemy enemy = obj.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemies.Add(enemy);
                    }
                }
            }
        }
        
        // If 2 or more enemies at the same position, stun them all
        if (enemies.Count >= 2)
        {
            foreach (var enemy in enemies)
            {
                enemy.SetStunned(true);
            }
        }
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

