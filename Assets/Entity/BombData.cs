using System;
using UnityEngine;
using System.Collections.Generic;

namespace Entity
{
    [Serializable]
    public class BombData
    {
        public string bombType;
        public string bombName;
        public int range;
        public int knockbackDistance;
        public string colorHex;
        public string fieldSpriteName;
        public string iconSpriteName;
        public Sprite fieldSprite;
        public Sprite iconSprite;
        // only for real bomb
        public List<string> animationSpriteNames;
        public List<Sprite> animationSprites;
        public List<Vector2> animationPositionCorrection;
        public float animationPeriod;
        
        // Get Color from hex string
        public Color GetColor()
        {
            if (ColorUtility.TryParseHtmlString(colorHex, out Color color))
            {
                return color;
            }
            return Color.white;
        }
        
        // Get BombType enum from string
        public BombType GetBombType()
        {
            if (Enum.TryParse(bombType, out BombType type))
            {
                return type;
            }
            return BombType.FirstBomb;
        }
    }
}
