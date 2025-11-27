using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    
    // motion duration (get from GameManager)
    private float _walkDuration;
    private float _knockbackDuration;
    
    // boundary coordinate (get from GameManager)
    private float _minX, _maxX, _minY, _maxY;

    // initializing internal attribute
    void Awake()
    {
        _walkDuration = GameManager.Instance.walkDuration;
        _knockbackDuration = GameManager.Instance.knockbackDuration;
        _minX = GameManager.GetMinX();
        _maxX = GameManager.GetMaxX();
        _minY = GameManager.GetMinY();
        _maxY = GameManager.GetMaxY();
    }
    
    // walk API
    public void Walk(Direction direction)
    {
        StartCoroutine(Move(direction, 1, _walkDuration));
    }
    
    // knockback API
    public void Knockback(Direction direction, int distance)
    {
        StartCoroutine(Move(direction, distance, _knockbackDuration));
    }
    
    // internal move API
    private IEnumerator Move(Direction direction, int distance, float duration)
    {
        Vector3 start = transform.position;
        Vector3 target = GetTarget(direction, distance, start);
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            transform.position = LerpWrap(start, target, t, _minX, _maxX, _minY, _maxY);
            yield return null;
        }

        transform.position = GetWrappedTarget(direction, distance, start, _minX, _maxX, _minY, _maxY);
    }

    // It can process boundary condition
    private static Vector3 LerpWrap(Vector3 a, Vector3 b, float t,
        float minX = float.NegativeInfinity, float maxX = float.PositiveInfinity,
        float minY = float.NegativeInfinity, float maxY = float.PositiveInfinity,
        float minZ = float.NegativeInfinity, float maxZ = float.PositiveInfinity)
    {
        float x = a.x + (b.x - a.x) * t;
        float y = a.y + (b.y - a.y) * t;
        float z = a.z + (b.z - a.z) * t;
        
        if (x > maxX)
            x = minX + (b.x - a.x) * t + maxX - a.x;
        else if (x < minX)
            x = maxX + (b.x - a.x) * t - minX + a.x;
        
        if (y > maxY)
            y = minY + (b.y - a.y) * t + maxY - a.y;
        else if (y < minY)
            y = maxY + (b.y - a.y) * t - minY + a.y;
        
        if (z > maxZ)
            z = minZ + (b.z - a.z) * t + maxX - a.z;
        else if (x < minX)
            z = maxX + (b.z - a.z) * t - minX + a.z;

        return new Vector3(x, y, z);
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
        
        if (result.x < minX || result.x > maxX)
            result.x = Mod(result.x - minX, maxX - minX) + minX;
        if (result.y < minY || result.y > maxY)
            result.y = Mod(result.y - minY, maxY - minY) + minY;
        if (result.z < minZ || result.z > maxZ)
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
