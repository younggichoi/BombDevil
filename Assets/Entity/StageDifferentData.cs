namespace Entity
{
    [System.Serializable]
    public class StageDifferentData
    {
        public int stageId;
        public int width;
        public int height;
        public int enemyNumber;
        
        // Initial bomb counts per type
        public int initialBlueBomb;
        public int initialGreenBomb;
        public int initialPinkBomb;
        public int initialSkyblueBomb;
        
        // Turn limit for the stage
        public int remainingTurns;
        
        public string boardSpritePath;  // Resources path to board sprite (e.g., "Sprites/Boards/board_7x7")
        
        // Get initial bomb count by type
        public int GetInitialBombCount(BombType bombType)
        {
            switch (bombType)
            {
                case BombType.BlueBomb:
                    return initialBlueBomb;
                case BombType.GreenBomb:
                    return initialGreenBomb;
                case BombType.PinkBomb:
                    return initialPinkBomb;
                case BombType.SkyblueBomb:
                    return initialSkyblueBomb;
                default:
                    return 0;
            }
        }
        
        // Get total initial bomb count
        public int GetTotalInitialBombs()
        {
            return initialBlueBomb + initialGreenBomb + initialPinkBomb + initialSkyblueBomb;
        }
    }
}