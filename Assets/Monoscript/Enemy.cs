using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    // motion duration (get from GameManager)
    private float _walkDuration;
    private float _knockbackDuration;
    
    // cell size for scaling movement
    private float _cellSize;
    
    // direction attribute for this enemy (as Vector2)
    private Vector2 _moveDirection;
    
    // boundary coordinate (get from BoardManager)
    private float _minX, _maxX, _minY, _maxY;

    // initializing internal attribute
    public void Initialize(GameManager gameManager, BoardManager boardManager, Sprite sprite)
    {
        _walkDuration = gameManager.getWalkDuration();
        _knockbackDuration = gameManager.getKnockbackDuration();
        _cellSize = boardManager.GetCellSize();
        _minX = boardManager.GetMinX();
        _maxX = boardManager.GetMaxX();
        _minY = boardManager.GetMinY();
        _maxY = boardManager.GetMaxY();
        
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
    
    // walk API
    // directionAndDistance: the vector from current position to target cell (in board units, not normalized)
    public void Walk(Vector2 directionAndDistance)
    {
        StartCoroutine(Move(directionAndDistance, _walkDuration));
    }
    
    // set random direction (Up, Down, Left, Right) and apply rotation
    public void SetRandomDirection()
    {
        Vector2[] cardinalDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
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
        if (_moveDirection == Vector2.down)
            rotationZ = 0f;
        else if (_moveDirection == Vector2.up)
            rotationZ = 180f;
        else if (_moveDirection == Vector2.left)
            rotationZ = -90f;
        else if (_moveDirection == Vector2.right)
            rotationZ = 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
    }
    
    // get current direction
    public Vector2 GetMoveDirection()
    {
        return _moveDirection;
    }
    
    // move in the assigned direction and distance
    public void MoveInDirection()
    {
        Walk(_moveDirection);
    }
    
    // knockback API
    // directionAndDistance: the vector from current position to target cell (in board units, not normalized)
    public void Knockback(Vector2 directionAndDistance)
    {
        Debug.Log($"Enemy at {transform.position} knocked back by {directionAndDistance}");
        StartCoroutine(Move(directionAndDistance, _knockbackDuration));
    }
    
    // internal move API
    // directionAndDistance: the vector from current position to target cell (in board units, not normalized)
    private IEnumerator Move(Vector2 directionAndDistance, float duration)
    {
        Vector3 start = transform.position;
        Vector3 target = GetTarget(directionAndDistance, start);
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            transform.position = LerpWrap(start, target, t, _minX, _maxX, _minY, _maxY);
            yield return null;
        }
        transform.position = GetWrappedTarget(directionAndDistance, start, _minX, _maxX, _minY, _maxY);
    }

    // It can process boundary condition - wraps position within bounds
    private static Vector3 LerpWrap(Vector3 a, Vector3 b, float t,
        float minX = float.NegativeInfinity, float maxX = float.PositiveInfinity,
        float minY = float.NegativeInfinity, float maxY = float.PositiveInfinity,
        float minZ = float.NegativeInfinity, float maxZ = float.PositiveInfinity)
    {
        // Simple linear interpolation
        float x = a.x + (b.x - a.x) * t;
        float y = a.y + (b.y - a.y) * t;
        float z = a.z + (b.z - a.z) * t;
        
        // Wrap x within bounds
        float widthX = maxX - minX;
        if (widthX > 0)
        {
            while (x > maxX) x -= widthX;
            while (x < minX) x += widthX;
        }
        
        // Wrap y within bounds
        float widthY = maxY - minY;
        if (widthY > 0)
        {
            while (y > maxY) y -= widthY;
            while (y < minY) y += widthY;
        }
        
        // Wrap z within bounds
        float widthZ = maxZ - minZ;
        if (widthZ > 0)
        {
            while (z > maxZ) z -= widthZ;
            while (z < minZ) z += widthZ;
        }

        return new Vector3(x, y, z);
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

