using System.Collections.Generic;

namespace Entity
{
    [System.Serializable]
    public class StageData
    {
        public int stageId;
        public int width;
        public int height;
        public int enemyNumber;
        public int wallNumber;
        
        // Turn limit for the stage
        public int remainingTurns;
        
        public List<TreasureChestData> treasureChest;
    }

    [System.Serializable]
    public struct TreasureChestData
    {
        public int durability;
        public int value;
    }
}
