using UnityEngine;
using Entity;

public partial class GameManager : MonoBehaviour
{
    private void MouseClickProcess()
    {
        Vector3 screenPos = Input.mousePosition;
        if (screenPos.x >= 1440 && screenPos.x <= 1780)
        {
            if (screenPos.y >= 890 && screenPos.y <= 990)
            {
                bombManager.SetCurrentBombType(BombType.BlueBomb);
                return;
            }
            else if (screenPos.y >= 760 && screenPos.y <= 860)
            {
                bombManager.SetCurrentBombType(BombType.GreenBomb);
                return;
            }
            else if (screenPos.y >= 630 && screenPos.y <= 730)
            {
                bombManager.SetCurrentBombType(BombType.PinkBomb);
                return;
            }
            else if (screenPos.y >= 500 && screenPos.y <= 600)
            {
                if (bombManager.IsRealBombAvailable())
                {
                    bombManager.SetCurrentBombType(BombType.RealBomb);
                }
                return;
            }
        }
        if (!bombManager.HasBombSelected())
        {
            Vector3 worldPosCheck = Camera.main.ScreenToWorldPoint(screenPos);
            int testX = GlobalToGridX(worldPosCheck.x);
            int testY = GlobalToGridY(worldPosCheck.y);
            if (testX >= 0 && testX < _width && testY >= 0 && testY < _height)
            {
                ShowTempMessage("No bomb has been selected!", 1f, "Player's turn");
            }
            return;
        }
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        int x = GlobalToGridX(worldPos.x);
        int y = GlobalToGridY(worldPos.y);
        if (x >= 0 && x < _width && y >= 0 && y < _height && _board[x, y].Count == 0)
        {
            BombType? currentType = bombManager.GetCurrentBombType();
            if (currentType == BombType.RealBomb)
            {
                CreateRealBomb(x, y);
            }
            else
            {
                CreateAuxiliaryBomb(x, y);
            }
        }
    }

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
