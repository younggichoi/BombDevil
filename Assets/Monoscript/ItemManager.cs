using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Entity;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    // Set via Initialize
    private Dictionary<ItemType, GameObject> _itemPrefabs;
    private Transform _itemSet;
    private TMP_Text _itemLeftoverText;

    private BoardManager _boardManager;

    // Leftover item counts per type
    private List<ItemCount> _leftoverItems;
    
    // Initial item counts (stored for reset)
    private List<ItemCount> _initialItems;

    // Item data loaded from JSON
    private Dictionary<ItemType, ItemData> _itemDataDict;

    // Current selected item index
    private int _currentItemIndex = 0;

    private bool _hasItemSelected = false;

    private GameObject _itemIcon;

    private void Awake()
    {
        GameService.Register(this);
        if (_itemPrefabs == null)
        {
            _itemPrefabs = new Dictionary<ItemType, GameObject>();
        }
    }

    public void Initialize(Dictionary<ItemType, GameObject> itemPrefabs, Transform itemSet, 
        List<ItemCount> initialItems, GameObject itemIcon, TMP_Text itemLeftoverText)
    {
        _itemPrefabs = itemPrefabs;
        _boardManager = GameService.Get<BoardManager>();
        _itemSet = itemSet;
        _itemIcon = itemIcon;
        _itemLeftoverText = itemLeftoverText;

        // Store initial items for reset
        _initialItems = new List<ItemCount>(initialItems ?? new List<ItemCount>());
        
        // Initialize leftover items dictionary
        _leftoverItems = new List<ItemCount>(_initialItems);

        // Load all item data from JSON
        LoadAllItemData();

        // Update UI
        // UpdateAllItemButtonTexts();
        UpdateItemIcon();
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

    // private void UpdateAllItemButtonTexts()
    // {
    //     if (_itemButtonTexts == null) return;

    //     foreach (var kvp in _itemButtonTexts)
    //     {
    //         var itemType = kvp.Key;
    //         var textComponent = kvp.Value;

    //         if (textComponent != null)
    //         {
    //             int count = GetLeftoverItem(itemType);
    //             textComponent.text = $"{itemType}: {count}";
    //         }
    //     }
    // }

    // private void UpdateAllItemTexts()
    // {
    //     if (_itemText != null)
    //     {
    //         string text = "";
    //         foreach (var item in _leftoverItems)
    //         {
    //             text += $"{item.itemType}: {item.count}\n";
    //         }
    //         _itemText.text = text;
    //     }
    // }

    public ItemData GetItemData(ItemType itemType)
    {
        if (_itemDataDict.TryGetValue(itemType, out ItemData data))
            return data;
        return null;
    }

    public ItemType? GetCurrentItemType()
    {
        return _leftoverItems[_currentItemIndex].itemType;
    }

    public bool HasItemSelected()
    {
        return _leftoverItems.Count > 0 && _hasItemSelected;
    }

    public void UnselectItem()
    {
        _hasItemSelected = false;
        UpdateItemIcon();
    }

    public void SelectItem()
    {
        _hasItemSelected = true;
        UpdateItemIcon();
    }

    // public void SetCurrentItemType(ItemType itemType)
    // {
    //     _currentItemType = itemType;
    //     var gameManager = GameService.Get<GameManager>();
    //     if (gameManager != null)
    //     {
    //         gameManager.ShowItemSelectedMessage(itemType.ToString());
    //     }
    // }
    public void SetNextIndex()
    {
        _currentItemIndex = (_currentItemIndex + 1) % _leftoverItems.Count;
        UpdateItemIcon();
    }

    public void SetPreviousIndex()
    {
        _currentItemIndex = (_currentItemIndex - 1 + _leftoverItems.Count) % _leftoverItems.Count;
        UpdateItemIcon();
    }

    // public void ClearCurrentItemType()
    // {
    //     _currentItemType = null;
    // }

    // public bool CheckItemAvailable(ItemType itemType)
    // {
    //     bool available = _leftoverItems.TryGetValue(itemType, out int count) && count > 0;
    //     var gameManager = GameService.Get<GameManager>();
    //     if (!available && gameManager != null)
    //     {
            
    //         gameManager.ShowNoItemLeftMessage(itemType.ToString());
    //     }
    //     return available;
    // }

    public GameObject PlaceItem(int x, int y)
    {
        if (!_hasItemSelected)
        {
            Debug.LogWarning($"PlaceItem failed: No item type selected.");
            return null;
        }
        ItemType itemType = _leftoverItems[_currentItemIndex].itemType;
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
        Vector2 canvasPos = _boardManager.GridToCanvasPosition(x, y);
        RectTransform rectTransform = item.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = canvasPos;

        float cellSizeCanvas = _boardManager.GetCellSizeCanvas();
        rectTransform.sizeDelta = new Vector2(cellSizeCanvas, cellSizeCanvas);

        Image image = item.AddComponent<Image>();
        image.sprite = itemData.fieldSprite;
        // item.transform.position = _boardManager.GridToWorld(x, y);
        // item.transform.localScale = Vector3.one * _boardManager.GetCellSize();

        // Load and assign sprite if possible
        // var sr = item.GetComponent<SpriteRenderer>();
        // if (sr != null && itemData != null && itemData.fieldSprite != null)
        // {
        //     sr.sprite = itemData.fieldSprite;
        //     // Scale sprite to fit exactly one cell
        //     float cellSize = _boardManager.GetCellSize();
        //     Vector2 spriteSize = sr.sprite.bounds.size;
        //     float scaleX = cellSize / spriteSize.x;
        //     float scaleY = cellSize / spriteSize.y;
        //     float scale = Mathf.Min(scaleX, scaleY);  // Keep aspect ratio, fit within cell
        //     item.transform.localScale = Vector3.one * scale;
        // }

        // Add Item component to identify it later
        Item itemComponent = item.AddComponent<Item>();
        itemComponent.Type = itemType;

        // Modify struct by creating new instance
        ItemCount current = _leftoverItems[_currentItemIndex];
        current.count--;
        if (current.count == 0)
        {
            _leftoverItems.RemoveAt(_currentItemIndex);
            if (_currentItemIndex >= _leftoverItems.Count && _leftoverItems.Count > 0)
            {
                _currentItemIndex = _leftoverItems.Count - 1;
            }
            else if (_leftoverItems.Count == 0)
            {
                _hasItemSelected = false;
                _currentItemIndex = -1;
            }
        }
        else
        {
            _leftoverItems[_currentItemIndex] = current;
        }
        // UpdateAllItemButtonTexts();
        UpdateItemIcon();
        Debug.Log($"Placed item {itemType} at ({x}, {y}). Leftover: {current.count}.");
        return item;
    }

    public void RestoreItem(ItemType itemType)
    {
        for(int i = 0; i < _leftoverItems.Count; i++)
        {
            if (_leftoverItems[i].itemType == itemType)
            {
                ItemCount temp = _leftoverItems[i];
                temp.count++;
                _leftoverItems[i] = temp;
                // UpdateAllItemButtonTexts();
                UpdateItemIcon();
                return;
            }
        }
        _leftoverItems.Add(new ItemCount(itemType, 1));
        // UpdateAllItemButtonTexts();
        UpdateItemIcon();
    }

    public int GetLeftoverItem(ItemType itemType)
    {
        for(int i = 0; i < _leftoverItems.Count; i++)
        {
            if (_leftoverItems[i].itemType == itemType)
            {
                return _leftoverItems[i].count;
            }
        }
        return 0;
    }

    public List<ItemCount> GetRemainingItem()
    {
        return new List<ItemCount>(_leftoverItems);
    }

    public int GetLeftoverItem()
    {
        if (_leftoverItems.Count == 0)
            return 0;
        return _leftoverItems[_currentItemIndex].count;
    }

    public int GetTotalLeftoverItems()
    {
        int total = 0;
        foreach (var item in _leftoverItems)
            total += item.count;
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
        if (_initialItems == null) {
            Debug.LogError("_initialItems is null in ItemManager.ResetItems");
            return;
        }
        // Reset leftover items from stored initial values
        _leftoverItems = new List<ItemCount>(_initialItems);
        _currentItemIndex = 0;
        // UpdateAllItemButtonTexts();
        UpdateItemIcon();
    }

    private void UpdateItemIcon()
    {
        var image = _itemIcon.GetComponent<Image>();
        if (_currentItemIndex == -1)
        {
            image.sprite = null;
            _itemLeftoverText.text = "";
            image.color = Color.clear;
            return;
        }
        ItemData data = GetItemData(_leftoverItems[_currentItemIndex].itemType);
        image.sprite = data.iconSprite;
        image.color = Color.white;
        // Resize to match cell size
        // float cellSize = _boardManager.GetCellSize();
        Vector2 spriteSize = image.sprite.bounds.size;
        // float scaleX = cellSize / spriteSize.x;
        // float scaleY = cellSize / spriteSize.y;
        // float scale = Mathf.Min(scaleX, scaleY);
        float scaleX = spriteSize.x;
        float scaleY = spriteSize.y;
        float scale = Mathf.Max(scaleX, scaleY);
        // Hardcoded to match the size of the item slot sprite
        _itemIcon.transform.localScale = Vector3.one * scale * 0.1f;

        _itemLeftoverText.text = $"{_leftoverItems[_currentItemIndex].count}";
        if (_hasItemSelected)
            _itemLeftoverText.color = Color.green;
        else
            _itemLeftoverText.color = Color.white;
    }

}
