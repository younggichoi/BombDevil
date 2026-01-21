using UnityEngine;
using Entity;

public partial class GameManager : MonoBehaviour
{
    private void MouseClickProcess()
    {
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;  // 2D 게임이므로 z를 0으로 설정
        
        // Raycast로 클릭된 오브젝트 감지
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        
        if (hit.collider != null)
        {
            string objectName = hit.collider.gameObject.name;
            
            // 폭탄 선택 처리 (스프라이트에 Collider2D 필요)
            switch (objectName)
            {
                case "1stBombIcon":
                    ItemManager.UnselectItem();
                    BombManager.SetCurrentIndex(0);
                    return;
                case "2ndBombIcon":
                    ItemManager.UnselectItem();
                    BombManager.SetCurrentIndex(1);
                    return;
                case "3rdBombIcon":
                    ItemManager.UnselectItem();
                    BombManager.SetCurrentIndex(2);
                    return;
                case "RealBombIcon":
                    ItemManager.UnselectItem();
                    BombManager.SetCurrentIndex(3);
                    return;
                case "ItemIcon":
                    ItemManager.SelectItem();
                    BombManager.SetCurrentIndex(-1);
                    return;
            }
        }

        // 아이템 배치 처리
        if (ItemManager != null && ItemManager.HasItemSelected())
        {
            BombManager.ClearCurrentBombType();
            int x = GlobalToGridX(worldPos.x);
            int y = GlobalToGridY(worldPos.y);
            if (x >= 0 && x < _width && y >= 0 && y < _height && _board[x, y].Count == 0)
            {
                PlaceItemAt(x, y);
            }
            return;
        }
        
        // 폭탄 배치 처리
        if (!BombManager.HasBombSelected())
        {
            int x = GlobalToGridX(worldPos.x);
            int y = GlobalToGridY(worldPos.y);
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                ShowTempMessage("No bomb has been selected!", 1f, "Player's turn");
            }
            return;
        }
        
        int bx = GlobalToGridX(worldPos.x);
        int by = GlobalToGridY(worldPos.y);
        if (bx >= 0 && bx < _width && by >= 0 && by < _height)
        {
            BombType? currentType = BombManager.GetCurrentBombType();
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
