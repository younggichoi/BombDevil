using UnityEngine;
using Entity;

public partial class GameManager : MonoBehaviour
{

    private void CreateAuxiliaryBomb(int x, int y)
    {
        if (_board[x, y].Count > 0)
            return;
        BombType? currentType = bombManager.GetCurrentBombType();
        if (currentType.HasValue && !bombManager.CheckBombAvailable(currentType.Value))
            return;
        GameObject bomb = bombManager.PlantAuxiliaryBomb(x, y);
        if (bomb != null)
        {
            _board[x, y].Add(bomb);
            _auxiliaryBombs.Add(new Vector2Int(x, y));
            _allBombs.Add((new Vector2Int(x, y), false));
        }
    }

    private void CreateRealBomb(int x, int y)
    {
        if (_board[x, y].Count > 0)
            return;
        GameObject bomb = bombManager.PlantRealBomb(x, y);
        if (bomb != null)
        {
            _board[x, y].Add(bomb);
            _realBombs.Add(new Vector2Int(x, y));
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
}
