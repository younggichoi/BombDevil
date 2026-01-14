using UnityEngine;
using Entity;

public partial class GameManager : MonoBehaviour
{
    // Call this from UI button event
    public void OnItemButtonClicked(ItemType itemType)
    {
        ItemManager.SetCurrentItemType(itemType);
    }

    // Call this from UI or game logic to place item at grid position
    public void PlaceItemAt(int x, int y)
    {
        if (_board[x, y].Count > 0)
            return;
        ItemType? currentType = ItemManager.GetCurrentItemType();
        if (!currentType.HasValue || !ItemManager.CheckItemAvailable(currentType.Value))
            return;
        GameObject item = ItemManager.PlaceItem(x, y);
        if (item != null)
        {
            _board[x, y].Add(item);
            _placedItems.Add(new Vector2Int(x, y));
            _allItems.Add((new Vector2Int(x, y), currentType.Value));
            if(item != null && currentType == ItemType.Teleporter)
            {
                _teleporters.Add(new Vector2Int(x, y));
            }
            if(item != null && currentType == ItemType.Megaphone)
            {
                item.AddComponent<Megaphone>();
            }
        }
    }
}