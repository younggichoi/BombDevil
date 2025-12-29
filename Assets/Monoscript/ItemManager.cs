using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Entity;

public class ItemManager : MonoBehaviour
{
    // Set via Initialize
    private Dictionary<ItemType, GameObject> _itemPrefabs;
    private Transform _itemSet;
    private TMP_Text _itemText;
    private GameManager _gameManager;

    private BoardManager _boardManager;

    // Leftover item counts per type
    private Dictionary<ItemType, int> _leftoverItems;

    // Item button text UI
    private Dictionary<ItemType, TMP_Text> _itemButtonTexts;

    // Item data loaded from JSON
    private Dictionary<ItemType, ItemData> _itemDataDict;

    // Current selected item type (null = not selected)
    private ItemType? _currentItemType = null;

    public void Initialize(Dictionary<ItemType, GameObject> itemPrefabs, GameManager gameManager, BoardManager boardManager, Transform itemSet, Dictionary<ItemType, TMP_Text> itemButtonTexts)
    {
        _itemPrefabs = itemPrefabs;
        _gameManager = gameManager;
        _boardManager = boardManager;
        _itemSet = itemSet;
        _itemButtonTexts = itemButtonTexts;

        // Initialize leftover items from GameManager
        _leftoverItems = new Dictionary<ItemType, int>();
        foreach (var itemType in _itemPrefabs.Keys)
        {
            _leftoverItems[itemType] = gameManager.GetInitialItemCount(itemType);
        }

        // Load all item data from JSON
        LoadAllItemData();

        // Update UI
        UpdateAllItemButtonTexts();
    }

    private void LoadAllItemData()
    {
        _itemDataDict = new Dictionary<ItemType, ItemData>();
        foreach (var itemType in _itemPrefabs.Keys)
        {
            LoadItemData(itemType.ToString());
        }
    }

    private void LoadItemData(string itemTypeName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Json/Item/{itemTypeName}");
        Debug.Log($"File content: {jsonFile.text}");
        if (jsonFile == null)
            return;
        ItemData data = JsonUtility.FromJson<ItemData>(jsonFile.text);
        if (System.Enum.TryParse(itemTypeName, out ItemType type))
        {
            _itemDataDict[type] = data;
        }
        Debug.Log($"Loaded item data for {itemTypeName}");
    }

    private void UpdateAllItemButtonTexts()
    {
        if (_itemButtonTexts == null) return;

        foreach (var kvp in _itemButtonTexts)
        {
            var itemType = kvp.Key;
            var textComponent = kvp.Value;

            if (textComponent != null && _leftoverItems.TryGetValue(itemType, out int count))
            {
                textComponent.text = $"{itemType}: {count}";
            }
        }
    }

    private void UpdateAllItemTexts()
    {
        if (_itemText != null)
        {
            string text = "";
            foreach (var kvp in _leftoverItems)
            {
                text += $"{kvp.Key}: {kvp.Value}\n";
            }
            _itemText.text = text;
        }
    }

    public ItemData GetItemData(ItemType itemType)
    {
        if (_itemDataDict.TryGetValue(itemType, out ItemData data))
            return data;
        return null;
    }

    public ItemType? GetCurrentItemType()
    {
        return _currentItemType;
    }

    public bool HasItemSelected()
    {
        return _currentItemType.HasValue;
    }

    public void SetCurrentItemType(ItemType itemType)
    {
        _currentItemType = itemType;
        if (_gameManager != null)
        {
            _gameManager.ShowItemSelectedMessage(itemType.ToString());
        }
    }

    public void ClearCurrentItemType()
    {
        _currentItemType = null;
    }

    public bool CheckItemAvailable(ItemType itemType)
    {
        bool available = _leftoverItems.TryGetValue(itemType, out int count) && count > 0;
        if (!available && _gameManager != null)
        {
            _gameManager.ShowNoItemLeftMessage(itemType.ToString());
        }
        return available;
    }

    public GameObject PlaceItem(int x, int y)
    {
        if (!_currentItemType.HasValue)
        {
            Debug.LogWarning($"PlaceItem failed: No item type selected.");
            return null;
        }
        ItemType itemType = _currentItemType.Value;
        if (!_leftoverItems.TryGetValue(itemType, out int leftover) || leftover <= 0)
        {
            Debug.LogWarning($"PlaceItem failed: No leftover items for {itemType}.");
            return null;
        }
        if (!_itemPrefabs.TryGetValue(itemType, out GameObject prefab))
        {
            Debug.LogWarning($"PlaceItem failed: No prefab found for {itemType}.");
            return null;
        }
        ItemData itemData = GetItemData(itemType);
        if (itemData == null)
        {
            Debug.LogWarning($"PlaceItem failed: No ItemData found for {itemType}.");
            return null;
        }
        GameObject item = Instantiate(prefab, _itemSet);
        item.transform.position = _boardManager.GridToWorld(x, y);
        item.transform.localScale = Vector3.one * _boardManager.GetCellSize();

        // Load and assign sprite if possible
        var sr = item.GetComponent<SpriteRenderer>();
        if (sr != null && itemData != null && !string.IsNullOrEmpty(itemData.spriteName))
        {
            Sprite sprite = Resources.Load<Sprite>("ItemSprites/" + itemData.spriteName);
            if (sprite != null)
                sr.sprite = sprite;
        }

        _leftoverItems[itemType]--;
        UpdateAllItemButtonTexts();
        Debug.Log($"Placed item {itemType} at ({x}, {y}). Leftover: {_leftoverItems[itemType]}");
        return item;
    }

    public int GetLeftoverItem(ItemType itemType)
    {
        if (_leftoverItems.TryGetValue(itemType, out int count))
            return count;
        return 0;
    }

    public int GetTotalLeftoverItems()
    {
        int total = 0;
        foreach (var count in _leftoverItems.Values)
            total += count;
        return total;
    }
}
