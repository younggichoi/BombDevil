using UnityEngine;
using Entity;

public partial class GameManager : MonoBehaviour
{

    private void CreateAuxiliaryBomb(int x, int y)
    {
        // Check if a bomb already exists at this position (enemies are fine)
        if (HasBombAt(x, y))
            return;
        BombType? currentType = bombManager.GetCurrentBombType();
        if (currentType.HasValue && !bombManager.CheckBombAvailable(currentType.Value))
            return;
        GameObject bomb = bombManager.PlantAuxiliaryBomb(x, y);
        if (bomb != null)
        {
            _board[x, y].Add(bomb);
            _allBombs.Add((new Vector2Int(x, y), false));
        }
    }

    private void CreateRealBomb(int x, int y)
    {
        // Check if a bomb already exists at this position (enemies are fine)
        if (HasBombAt(x, y))
            return;
        GameObject bomb = bombManager.PlantRealBomb(x, y);
        if (bomb != null)
        {
            _board[x, y].Add(bomb);
            _allBombs.Add((new Vector2Int(x, y), true));
        }
    }

    private int GlobalToGridX(float x)
    {
        float cellSize = boardManager.GetCellSize();
        return Mathf.FloorToInt(x / cellSize + _width / 2f);
    }

    private int GlobalToGridY(float y)
    {
        float cellSize = boardManager.GetCellSize();
        return Mathf.FloorToInt(y / cellSize + _height / 2f);
    }
    
    // Remove a bomb or item at the specified position and restore to inventory
    private void RemoveObjectAt(int x, int y)
    {
        GameObject objectToRemove = null;
        
        // Check for bombs first
        foreach (var obj in _board[x, y])
        {
            if (obj == null) continue;
            
            if (obj.GetComponent<AuxiliaryBomb>() != null || obj.GetComponent<RealBomb>() != null)
            {
                objectToRemove = obj;
                break;
            }
        }

        // If no bomb, check for items
        if (objectToRemove == null)
        {
            foreach (var obj in _board[x, y])
            {
                if (obj == null) continue;
                if (obj.GetComponent<Item>() != null)
                {
                    objectToRemove = obj;
                    break;
                }
            }
        }
        
        if (objectToRemove != null)
        {
            // Remove from board
            _board[x, y].Remove(objectToRemove);
            Vector2Int pos = new Vector2Int(x, y);

            AuxiliaryBomb auxBomb = objectToRemove.GetComponent<AuxiliaryBomb>();
            RealBomb realBomb = objectToRemove.GetComponent<RealBomb>();
            Item item = objectToRemove.GetComponent<Item>();

            if (auxBomb != null)
            {
                // Remove from _allBombs list
                _allBombs.RemoveAll(b => b.coord == pos);
                // Restore to inventory
                bombManager.RestoreBomb(auxBomb.GetBombType());
                ShowTempMessage("Bomb removed!", 0.5f, "Remove mode");
            }
            else if (realBomb != null)
            {
                // Remove from _allBombs list
                _allBombs.RemoveAll(b => b.coord == pos);
                // Restore to inventory
                bombManager.RestoreRealBomb();
                ShowTempMessage("Bomb removed!", 0.5f, "Remove mode");
            }
            else if (item != null)
            {
                // Remove from item lists
                _placedItems.Remove(pos);
                _allItems.RemoveAll(i => i.coord == pos && i.itemType == item.Type);
                if (item.Type == ItemType.Teleporter)
                {
                    _teleporters.Remove(pos);
                }

                // Restore to inventory
                itemManager.RestoreItem(item.Type);
                ShowTempMessage("Item removed!", 0.5f, "Remove mode");
            }
            
            // Destroy the object
            Destroy(objectToRemove);
        }
    }
}
