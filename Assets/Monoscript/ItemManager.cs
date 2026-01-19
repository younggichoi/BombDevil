using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Entity;

public class ItemManager : MonoBehaviour
{
    // Set via Initialize
    private Dictionary<ItemType, GameObject> _itemPrefabs;
    private Transform _itemSet;
    private TMP_Text _itemText;

    private BoardManager _boardManager;

    // Leftover item counts per type
    private Dictionary<ItemType, int> _leftoverItems;

    // Item button text UI
    private Dictionary<ItemType, TMP_Text> _itemButtonTexts;

    // Item data loaded from JSON
    private Dictionary<ItemType, ItemData> _itemDataDict;

    // Current selected item type (null = not selected)
    private ItemType? _currentItemType = null;

    private void Awake()
    {
        GameService.Register(this);
        if (_itemPrefabs == null)
        {
            _itemPrefabs = new Dictionary<ItemType, GameObject>();
        }
    }

    public void Initialize(Dictionary<ItemType, GameObject> itemPrefabs, Transform itemSet, Dictionary<ItemType, TMP_Text> itemButtonTexts)
    {
        _itemPrefabs = itemPrefabs;
        _boardManager = GameService.Get<BoardManager>();
        _itemSet = itemSet;
        _itemButtonTexts = itemButtonTexts;

        var gameManager = GameService.Get<GameManager>();
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
        string path = Path.Combine(Application.streamingAssetsPath, $"Json/Item/{itemTypeName}.json");
        if (!File.Exists(path))
        {
            Debug.LogError($"Failed to load {itemTypeName}.json from {path}");
            return;
        }
        string json = File.ReadAllText(path);
        ItemData data = JsonUtility.FromJson<ItemData>(json);
        data.fieldSprite = Resources.Load<Sprite>("Sprites/Item/" + data.fieldSpriteName);
        data.iconSprite = Resources.Load<Sprite>("Sprites/Item/" + data.iconSpriteName);
        if (System.Enum.TryParse(itemTypeName, out ItemType type))
        {
            _itemDataDict[type] = data;
        }
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
        var gameManager = GameService.Get<GameManager>();
        if (gameManager != null)
        {
            gameManager.ShowItemSelectedMessage(itemType.ToString());
        }
    }

    public void ClearCurrentItemType()
    {
        _currentItemType = null;
    }

    public bool CheckItemAvailable(ItemType itemType)
    {
        bool available = _leftoverItems.TryGetValue(itemType, out int count) && count > 0;
        var gameManager = GameService.Get<GameManager>();
        if (!available && gameManager != null)
        {
            
            gameManager.ShowNoItemLeftMessage(itemType.ToString());
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
        if (sr != null && itemData != null && itemData.fieldSprite != null)
        {
            sr.sprite = itemData.fieldSprite;
            // Scale sprite to fit exactly one cell
            float cellSize = _boardManager.GetCellSize();
            Vector2 spriteSize = sr.sprite.bounds.size;
            float scaleX = cellSize / spriteSize.x;
            float scaleY = cellSize / spriteSize.y;
            float scale = Mathf.Min(scaleX, scaleY);  // Keep aspect ratio, fit within cell
            item.transform.localScale = Vector3.one * scale;
        }

        // Add Item component to identify it later
        Item itemComponent = item.AddComponent<Item>();
        itemComponent.Type = itemType;

        _leftoverItems[itemType]--;
        UpdateAllItemButtonTexts();
        Debug.Log($"Placed item {itemType} at ({x}, {y}). Leftover: {_leftoverItems[itemType]}.");
        return item;
    }

    public void RestoreItem(ItemType itemType)
    {
        if (_leftoverItems.ContainsKey(itemType))
        {
            _leftoverItems[itemType]++;
            UpdateAllItemButtonTexts();
        }
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

    public void ClearItems()
    {
        if (_itemSet != null)
        {
            foreach (Transform child in _itemSet)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void ResetItems()
    {
        Debug.Log("ItemManager.ResetItems called");
        var gameManager = GameService.Get<GameManager>();
        if (gameManager == null) {
            Debug.LogError("GameManager is null in ItemManager.ResetItems");
            return;
        }
        // Initialize leftover items from GameManager
        _leftoverItems = new Dictionary<ItemType, int>();
        if (_itemPrefabs == null) {
            Debug.LogError("_itemPrefabs is null in ItemManager.ResetItems");
            return;
        }
        foreach (var itemType in _itemPrefabs.Keys)
        {
            _leftoverItems[itemType] = gameManager.GetInitialItemCount(itemType);
        }
        UpdateAllItemButtonTexts();
    }
}
