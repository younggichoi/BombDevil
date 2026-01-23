using UnityEngine;
using Entity;

public class RealBomb : MonoBehaviour
{
    // Bomb properties (set via Initialize)
    private BombType _bombType;
    private int _range;
    
    // Initialize bomb with data from JSON
    public void Initialize(BombData bombData)
    {
        _bombType = bombData.GetBombType();
        _range = bombData.range;
        
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
    
    // Explode API - destroy the bomb object
    // Actual enemy killing is handled by GameManager
    public void Explode()
    {
        Destroy(gameObject);
    }
}
