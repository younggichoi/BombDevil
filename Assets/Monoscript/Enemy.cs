using System.Collections;
using UnityEngine;

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

    // initializing internal attribute
    public void Initialize(Sprite sprite, int? forcedId = null)
    {
        var gameManager = GameService.Get<GameManager>();
        var boardManager = GameService.Get<BoardManager>();

        _walkDuration = gameManager.getWalkDuration();
        _knockbackDuration = gameManager.getKnockbackDuration();
        _cellSize = boardManager.GetCellSize();
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
        if (stunned)
            _isStunned = true;
        // Optionally, add visual feedback for stun here
        // e.g., change color, play animation, etc.
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
        Debug.Log($"Enemy at {transform.position} knocked back by {directionAndDistance}");
        // Stun is permanent; do not remove stun here
        StartCoroutine(Move(directionAndDistance, _knockbackDuration));
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

