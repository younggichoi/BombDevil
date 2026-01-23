using UnityEngine;
using Entity;

public partial class GameManager : MonoBehaviour
{
    public void CreateTreasureChest()
    {
        if (_treasureChests == null || _treasureChests.Count == 0)
            return;

        foreach (var chestData in _treasureChests)
        {
            // Find a random empty position
            int x, y;
            
            do
            {
                x = UnityEngine.Random.Range(0, _width);
                y = UnityEngine.Random.Range(0, _height);
            }
            while (_board[x, y].Count > 0);
            

            GameObject treasureChest = this.TreasureChestManager.CreateTreasureChest(x, y, chestData.durability, chestData.value);
            
            // Record in board
            _board[x, y].Add(treasureChest);
            
            Debug.Log($"Created treasure chest at ({x}, {y}) with durability={chestData.durability}, value={chestData.value}");
        }
    }    
}
