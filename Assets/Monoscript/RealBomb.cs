using UnityEngine;
using Entity;
using UnityEngine.UI;
using System.Collections.Generic;

public class RealBomb : MonoBehaviour
{
    // Bomb properties (set via Initialize)
    private BombType _bombType;
    private List<Sprite> _animationSprites;
    private List<Vector2> _animationPositionCorrection;
    private int _range;
    private Vector2 _position;
    private Vector2 _correctedPosition;

    // animation properties
    private int _animationIndex;
    private float _animationTimerInterval;
    private float _animationTimer;
    
    // Initialize bomb with data from JSON
    public void Initialize(BombData bombData)
    {
        _bombType = bombData.GetBombType();
        _range = bombData.range;
        _animationSprites = bombData.animationSprites;
        _animationPositionCorrection = bombData.animationPositionCorrection;
        _animationIndex = 5;
        _animationTimerInterval = bombData.animationPeriod / _animationSprites.Count;
        _animationTimer = 0;
        RectTransform rectTransform = GetComponent<RectTransform>();
        _position = rectTransform.anchoredPosition;
        rectTransform.localScale = new Vector3(0.2f, 0.2f, 1f);

        SoundManager.Instance.PlayRealBombSound();

        UpdateImage();
    }

    private void Update()
    {
        _animationTimer += Time.deltaTime;
        if (_animationTimer >= _animationTimerInterval)
        {
            _animationTimer = 0;
            _animationIndex++;
            if (_animationIndex >= _animationSprites.Count)
            {
                _animationIndex = 0;
            }
            UpdateImage();
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
        SoundManager.Instance.StopRealBombSound();
        SoundManager.Instance.PlaySFX(SoundManager.Instance.boomClip); 
        Destroy(gameObject);
    }

    private void UpdateImage()
    {
        Image image = GetComponent<Image>();
        image.sprite = _animationSprites[_animationIndex];

        image.SetNativeSize();
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = _position + _animationPositionCorrection[_animationIndex];
    }
}
