using UnityEngine;
using Entity;

public partial class GameManager : MonoBehaviour
{
    private void MouseClickProcess()
    {
        Vector3 screenPos = Input.mousePosition;

        /*if (screenPos.x >= 1440 && screenPos.x <= 1780)
        {
            itemManager.ClearCurrentItemType();
            // Check for bomb selection based on Y coordinate
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
        }*/

        // If an item is selected, handle item placement
        if (itemManager != null && itemManager.HasItemSelected())
        {
            bombManager.ClearCurrentBombType();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            int x = GlobalToGridX(worldPos.x);
            int y = GlobalToGridY(worldPos.y);
            if (x >= 0 && x < _width && y >= 0 && y < _height && _board[x, y].Count == 0)
            {
                PlaceItemAt(x, y);
            }
            return;
        }
        // Otherwise, handle bomb selection/placement as before
        
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
        Vector3 bombWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
        int bx = GlobalToGridX(bombWorldPos.x);
        int by = GlobalToGridY(bombWorldPos.y);
        if (bx >= 0 && bx < _width && by >= 0 && by < _height && _board[bx, by].Count == 0)
        {
            BombType? currentType = bombManager.GetCurrentBombType();
            if (currentType == BombType.RealBomb)
            {
                CreateRealBomb(bx, by);
            }
            else
            {
                CreateAuxiliaryBomb(bx, by);
            }
        }
    }
}
