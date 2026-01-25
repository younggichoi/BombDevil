using UnityEngine;
using Entity;

public partial class GameManager : MonoBehaviour
{

    private void CreateAuxiliaryBomb(int x, int y)
    {
        BombType? currentType = BombManager.GetCurrentBombType();
        if (currentType.HasValue && !BombManager.CheckBombAvailable(BombManager.GetCurrentIndex(), false))
            return;
        GameObject bomb = BombManager.PlantAuxiliaryBomb(x, y);
        if (bomb != null)
        {
            _board[x, y].Add(bomb);
            _allBombs.Add((new Vector2Int(x, y), false));
        }
    }

    private void CreateRealBomb(int x, int y)
    {
        GameObject bomb = BombManager.PlantRealBomb(x, y);
        if (bomb != null)
        {
            _board[x, y].Add(bomb);
            _allBombs.Add((new Vector2Int(x, y), true));
        }
    }

    private int GlobalToGridX(float x)
    {
        float cellSize = BoardManager.GetCellSize();
        return Mathf.FloorToInt((x - _centerX) / cellSize + _width / 2f);
    }

    private int GlobalToGridY(float y)
    {
        float cellSize = BoardManager.GetCellSize();
        return Mathf.FloorToInt((y - _centerY) / cellSize + _height / 2f);
    }
    
    // Remove a bomb or item at the specified position and restore to inventory
    private void RemoveObjectAt(int x, int y)
    {
        if (_board[x, y].Count == 0) return;

        // Find the last placed object in the cell to remove
        GameObject objectToRemove = _board[x, y][_board[x, y].Count - 1];
        
        if (objectToRemove != null)
        {
            AuxiliaryBomb auxBomb = objectToRemove.GetComponent<AuxiliaryBomb>();
            RealBomb realBomb = objectToRemove.GetComponent<RealBomb>();
            Item item = objectToRemove.GetComponent<Item>();

            // Only remove if it is a bomb or an item
            if (auxBomb == null && realBomb == null && item == null)
            {
                return;
            }
            
            // Remove from board
            _board[x, y].Remove(objectToRemove);
            Vector2Int pos = new Vector2Int(x, y);

            if (auxBomb != null)
            {
                // Find and remove the specific bomb from _allBombs list
                for (int i = _allBombs.Count - 1; i >= 0; i--)
                {
                    if (_allBombs[i].coord == pos && !_allBombs[i].isRealBomb)
                    {
                        _allBombs.RemoveAt(i);
                        break;
                    }
                }
                // Restore to inventory
                BombManager.RestoreBomb(auxBomb.GetBombType());
                ShowTempMessage("Bomb removed!", 0.5f, "Remove mode");
            }
            else if (realBomb != null)
            {
                // Find and remove the specific bomb from _allBombs list
                for (int i = _allBombs.Count - 1; i >= 0; i--)
                {
                    if (_allBombs[i].coord == pos && _allBombs[i].isRealBomb)
                    {
                        _allBombs.RemoveAt(i);
                        break;
                    }
                }
                // Restore to inventory
                BombManager.RestoreRealBomb();
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
                ItemManager.RestoreItem(item.Type);
                ShowTempMessage("Item removed!", 0.5f, "Remove mode");
            }
            
            // Destroy the object
            Destroy(objectToRemove);
        }
    }
}
