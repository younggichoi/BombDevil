using UnityEngine;
using Entity;

public partial class GameManager : MonoBehaviour
{
    private void MouseClickProcess()
    {
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;  // 2D 게임이므로 z를 0으로 설정
        
        // Remove mode - click on bomb or item to remove it
        if (_isRemoveMode)
        {
            int rx = GlobalToGridX(worldPos.x);
            int ry = GlobalToGridY(worldPos.y);
            if (rx >= 0 && rx < _width && ry >= 0 && ry < _height)
            {
                if (HasBombAt(rx, ry) || HasItemAt(rx, ry))
                {
                    RemoveObjectAt(rx, ry);
                    return;
                }
            }
        }
        
        // Raycast로 클릭된 오브젝트 감지
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        
        if (hit.collider != null)
        {
            string objectName = hit.collider.gameObject.name;
            
            // 폭탄 선택 처리 (스프라이트에 Collider2D 필요)
            switch (objectName)
            {
                case "1stBomb":
                    ExitRemoveMode();
                    itemManager.ClearCurrentItemType();
                    bombManager.SetCurrentBombType(BombType.FirstBomb);
                    return;
                case "2ndBomb":
                    ExitRemoveMode();
                    itemManager.ClearCurrentItemType();
                    bombManager.SetCurrentBombType(BombType.SecondBomb);
                    return;
                case "3rdBomb":
                    ExitRemoveMode();
                    itemManager.ClearCurrentItemType();
                    bombManager.SetCurrentBombType(BombType.ThirdBomb);
                    return;
                case "4thBomb":
                    ExitRemoveMode();
                    itemManager.ClearCurrentItemType();
                    bombManager.SetCurrentBombType(BombType.FourthBomb);
                    return;
                case "5thBomb":
                    ExitRemoveMode();
                    itemManager.ClearCurrentItemType();
                    bombManager.SetCurrentBombType(BombType.FifthBomb);
                    return;
                case "6thBomb":
                    ExitRemoveMode();
                    itemManager.ClearCurrentItemType();
                    bombManager.SetCurrentBombType(BombType.SixthBomb);
                    return;
                case "SkyblueBomb":
                    ExitRemoveMode();
                    itemManager.ClearCurrentItemType();
                    bombManager.SetCurrentBombType(BombType.SkyblueBomb);
                    return;
                case "RealBomb":
                    ExitRemoveMode();
                    itemManager.ClearCurrentItemType();
                    if (bombManager.IsRealBombAvailable())
                    {
                        bombManager.SetCurrentBombType(BombType.RealBomb);
                    }
                    return;
            }
        }

        // 아이템 배치 처리
        if (itemManager != null && itemManager.HasItemSelected())
        {
            bombManager.ClearCurrentBombType();
            int x = GlobalToGridX(worldPos.x);
            int y = GlobalToGridY(worldPos.y);
            if (x >= 0 && x < _width && y >= 0 && y < _height && _board[x, y].Count == 0)
            {
                PlaceItemAt(x, y);
            }
            return;
        }
        
        // 폭탄 배치 처리
        if (!bombManager.HasBombSelected())
        {
            int testX = GlobalToGridX(worldPos.x);
            int testY = GlobalToGridY(worldPos.y);
            if (testX >= 0 && testX < _width && testY >= 0 && testY < _height)
            {
                ShowTempMessage("No bomb has been selected!", 1f, "Player's turn");
            }
            return;
        }
        
        int bx = GlobalToGridX(worldPos.x);
        int by = GlobalToGridY(worldPos.y);
        if (bx >= 0 && bx < _width && by >= 0 && by < _height && !HasBombAt(bx, by))
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

    private void MouseRightClickProcess()
    {
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            GameObject obj = hit.collider.gameObject;
            if (obj.GetComponent<AuxiliaryBomb>() != null || 
                obj.GetComponent<RealBomb>() != null || 
                obj.GetComponent<Item>() != null)
            {
                // Use object position instead of mouse position to ensure we target the correct grid cell
                // even if the click is slightly off-center or on a large collider.
                int x = GlobalToGridX(obj.transform.position.x);
                int y = GlobalToGridY(obj.transform.position.y);

                if (x >= 0 && x < _width && y >= 0 && y < _height)
                {
                    RemoveObjectAt(x, y);
                }
            }
        }
    }
}
