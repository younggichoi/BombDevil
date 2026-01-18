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
        
        public string boardSpritePath;  // Resources path to board sprite (e.g., "Sprites/Boards/board_7x7")
    }
}
