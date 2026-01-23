using UnityEngine;

namespace Entity
{
    public class IngameCommonData
    {
        public float walkDuration;
        public float knockbackDuration;
        public Color enemyColor;

        public IngameCommonData(float walkDuration, float knockbackDuration, Color enemyColor)
        {
            this.walkDuration = walkDuration;
            this.knockbackDuration = knockbackDuration;
            this.enemyColor = enemyColor;
        }
    }
}
