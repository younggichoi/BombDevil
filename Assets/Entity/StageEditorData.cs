namespace Entity
{
    [System.Serializable]
    public class StageEditorData : StageData
    {
        // Initial bomb counts per type
        public int initial1stBomb;
        public int initial2ndBomb;
        public int initial3rdBomb;
        public int initial4thBomb;
        public int initial5thBomb;
        public int initial6thBomb;
        public int initialSkyblueBomb;

        // Get initial bomb count by type
        public int GetInitialBombCount(BombType bombType)
        {
            switch (bombType)
            {
                case BombType.FirstBomb:
                    return initial1stBomb;
                case BombType.SecondBomb:
                    return initial2ndBomb;
                case BombType.ThirdBomb:
                    return initial3rdBomb;
                case BombType.FourthBomb:
                    return initial4thBomb;
                case BombType.FifthBomb:
                    return initial5thBomb;
                case BombType.SixthBomb:
                    return initial6thBomb;
                case BombType.SkyblueBomb:
                    return initialSkyblueBomb;
                default:
                    return 0;
            }
        }
        
        // Get total initial bomb count
        public int GetTotalInitialBombs()
        {
            return initial1stBomb + initial2ndBomb + initial3rdBomb + 
                   initial4thBomb + initial5thBomb + initial6thBomb + initialSkyblueBomb;
        }
    }
}
