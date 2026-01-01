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
    
    // Remove a bomb at the specified position and restore to inventory
    private void RemoveBombAt(int x, int y)
    {
        GameObject bombToRemove = null;
        bool isRealBomb = false;
        BombType? bombType = null;
        
        // Find the bomb in this cell
        foreach (var obj in _board[x, y])
        {
            if (obj == null) continue;
            
            AuxiliaryBomb auxBomb = obj.GetComponent<AuxiliaryBomb>();
            if (auxBomb != null)
            {
                bombToRemove = obj;
                bombType = auxBomb.GetBombType();
                isRealBomb = false;
                break;
            }
            
            RealBomb realBomb = obj.GetComponent<RealBomb>();
            if (realBomb != null)
            {
                bombToRemove = obj;
                isRealBomb = true;
                break;
            }
        }
        
        if (bombToRemove != null)
        {
            // Remove from board
            _board[x, y].Remove(bombToRemove);
            
            // Remove from _allBombs list
            Vector2Int pos = new Vector2Int(x, y);
            _allBombs.RemoveAll(b => b.coord == pos);
            
            // Restore to inventory
            if (isRealBomb)
            {
                bombManager.RestoreRealBomb();
            }
            else if (bombType.HasValue)
            {
                bombManager.RestoreBomb(bombType.Value);
            }
            
            // Destroy the bomb object
            Destroy(bombToRemove);
            
            ShowTempMessage("Bomb removed!", 0.5f, "Remove mode");
        }
    }
}
