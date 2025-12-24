using UnityEngine;

namespace Entity
{
    public class StageCommonData
    {
        public float walkDuration;
        public float knockbackDuration;
        public int knockbackDistance;
        public Color enemyColor;
        public Color auxiliaryBombColor;

        public StageCommonData(float walkDuration, float knockbackDuration,
            int knockbackDistance, Color enemyColor, Color auxiliaryBombColor)
        {
            this.walkDuration = walkDuration;
            this.knockbackDuration = knockbackDuration;
            this.knockbackDistance = knockbackDistance;
            this.enemyColor = enemyColor;
            this.auxiliaryBombColor = auxiliaryBombColor;
        }
    }
}
