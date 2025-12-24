namespace Entity
{
    [System.Serializable]
    public class StageDifferentData
    {
        public int stageId;
        public int width;
        public int height;
        public int enemyNumber;
        public int initialAuxiliaryBomb;
        public string boardSpritePath;  // Resources path to board sprite (e.g., "Sprites/Boards/board_7x7")
    }
}