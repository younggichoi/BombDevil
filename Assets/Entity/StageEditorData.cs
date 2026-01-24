using System.Collections.Generic;

namespace Entity
{
    [System.Serializable]
    public class StageEditorData : StageData
    {
        // Initial bomb counts per type
        public int initial1stBomb;
        public int initial2ndBomb;
        public int initial3rdBomb;
        
        // Initial item counts per type
        public int initialTeleporter;
        public int initialMegaphone;

        // Get initial bomb count by type
        public int GetInitialBombCount(int slotNum)
        {
            switch (slotNum)
            {
                case 0:
                    return initial1stBomb;
                case 1:
                    return initial2ndBomb;
                case 2:
                    return initial3rdBomb;
                default:
                    return 0;
            }
        }

        public int GetInitialItemCount(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Teleporter:
                    return initialTeleporter;
                case ItemType.Megaphone:
                    return initialMegaphone;
                default:
                    return 0;
            }
        }
        
        // Get total initial bomb count
        public int GetTotalInitialBombs()
        {
            return initial1stBomb + initial2ndBomb + initial3rdBomb;
        }
        
        // Get total initial item count
        public int GetTotalInitialItems()
        {
            return initialTeleporter + initialMegaphone;
        }
    }
}
