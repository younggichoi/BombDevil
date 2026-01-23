using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    [System.Serializable]
    public struct ItemCount
    {
        public ItemType itemType;
        public int count;

        public ItemCount(ItemType type, int cnt)
        {
            itemType = type;
            count = cnt;
        }
    }

    [System.Serializable]
    public class SaveData
    {
        public int left1stBomb;
        public int left2ndBomb;
        public int left3rdBomb;
        public BombType firstBombType;
        public BombType secondBombType;
        public BombType thirdBombType;
        
        // List for serialization (direct support)
        public List<ItemCount> leftItem = new List<ItemCount>();
        public int scoring;

        // Difficulty of the game
        public int difficulty;
    }
}