using UnityEngine;
using Entity;

public partial class GameManager : MonoBehaviour
{
    // Canvas RectTransform for UI coordinate conversion
    private RectTransform _canvasRectTransform;
    private Camera _mainCamera;
    
    /// <summary>
    /// Set the Canvas RectTransform for UI coordinate conversion
    /// </summary>
    public void SetCanvasRectTransform(RectTransform canvasRect)
    {
        _canvasRectTransform = canvasRect;
        _mainCamera = Camera.main;
    }
    
    /// <summary>
    /// Convert mouse screen position to grid coordinates (supports both Canvas UI and World Space)
    /// </summary>
    private Vector2Int GetGridFromMousePosition()
    {
        Vector3 screenPos = Input.mousePosition;
        

        // Canvas UI mode: convert screen position to Canvas local position
        Vector2 canvasPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            screenPos,
            _mainCamera,
            out canvasPoint
        );
        return BoardManager.CanvasToGrid(canvasPoint);


    }

    private void MouseClickProcess()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);

        // Get grid position from mouse click (works for both Canvas UI and World Space)
        Vector2Int gridPos = GetGridFromMousePosition();
        int x = gridPos.x;
        int y = gridPos.y;
        
        // For legacy raycast (still needed for some features)
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        // 아이템 배치 처리
        if (ItemManager != null && ItemManager.HasItemSelected())
        {
            BombManager.ClearCurrentBombType();
            if (x >= 0 && x < _width && y >= 0 && y < _height && _board[x, y].Count == 0)
            {
                PlaceItemAt(x, y);
            }
            return;
        }
        
        // 폭탄 배치 처리
        if (!BombManager.HasBombSelected())
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                ShowTempMessage("No bomb has been selected!", 1f, "Player's turn");
            }
            return;
        }
        
        if (x >= 0 && x < _width && y >= 0 && y < _height)
        {
            BombType? currentType = BombManager.GetCurrentBombType();
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

    public void BombIconClick(int index)
    {
        if (!BombManager.IsBombLeft(index))
            return;
        ItemManager.UnselectItem();
        BombManager.SetCurrentIndex(index);
    }

    public void ItemIconClick()
    {
        ItemManager.SelectItem();
        BombManager.SetCurrentIndex(-1);
    }

    private void MouseRightClickProcess()
    {
        Vector2Int gridPos = GetGridFromMousePosition();
        int x = gridPos.x;
        int y = gridPos.y;
        
        // For Canvas UI mode, we need different removal logic
        if (x >= 0 && x < _width && y >= 0 && y < _height)
        {
            RemoveObjectAt(x, y);
        }
        
        // Legacy World Space mode with raycast
        // Vector3 screenPos = Input.mousePosition;
        // Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        // worldPos.z = 0;

        // RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        // if (hit.collider != null)
        // {
        //     GameObject obj = hit.collider.gameObject;
        //     if (obj.GetComponent<AuxiliaryBomb>() != null || 
        //         obj.GetComponent<RealBomb>() != null || 
        //         obj.GetComponent<Item>() != null)
        //     {
        //         int objX = GlobalToGridX(obj.transform.position.x);
        //         int objY = GlobalToGridY(obj.transform.position.y);

        //         if (objX >= 0 && objX < _width && objY >= 0 && objY < _height)
        //         {
        //             RemoveObjectAt(objX, objY);
        //         }
        //     }
        // }
    }
}
