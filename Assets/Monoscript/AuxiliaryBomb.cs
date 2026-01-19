using UnityEngine;
using Entity;

public class AuxiliaryBomb : MonoBehaviour
{
    // Bomb properties (set via Initialize)
    private BombType _bombType;
    private int _range;
    private int _knockbackDistance;
    
    // Initialize bomb with data from JSON
    public void Initialize(BombData bombData)
    {
        _bombType = bombData.GetBombType();
        _range = bombData.range;
        _knockbackDistance = bombData.knockbackDistance;
        
        // Apply color from bomb data
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (bombData.fieldSprite != null)
            {
                sr.sprite = bombData.fieldSprite;
            } else
            {
                sr.color = bombData.GetColor();
            }
        }
    }
    
    // Get bomb type
    public BombType GetBombType()
    {
        return _bombType;
    }
    
    // Get explosion range
    public int GetRange()
    {
        return _range;
    }
    
    // Get knockback distance
    public int GetKnockbackDistance()
    {
        return _knockbackDistance;
    }
    
    // Explode API
    // plan to add exploding motion (when the art is complete)
    public void Explode()
    {
        Destroy(gameObject);
    }
}
