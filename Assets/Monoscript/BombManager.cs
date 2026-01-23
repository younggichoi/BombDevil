using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Entity;

public class BombManager : MonoBehaviour
{
    // Set via Initialize
    private GameObject auxiliaryBomb;
    private GameObject realBombPrefab;
    private Transform auxiliaryBombSet;
    private Transform realBombSet;
    
    // Text UI for each bomb type
    private TMP_Text _1stBombText;
    private TMP_Text _2ndBombText;
    private TMP_Text _3rdBombText;

    // Icon UI for each bomb type
    private GameObject _1stBombIcon;
    private GameObject _2ndBombIcon;
    private GameObject _3rdBombIcon;
    private GameObject _realBombIcon;
    
    // Check UI for each bomb type (shows which bomb is selected)
    private GameObject _1stBombChecked;
    private GameObject _2ndBombChecked;
    private GameObject _3rdBombChecked;
    private GameObject _realBombChecked;

    // Explode button text
    private TMP_Text _explodeButtonText;

    // BoardManager reference for coordinate conversion
    private BoardManager _boardManager;
    
    // Leftover bomb counts per type
    struct BombCount
    {
        public BombType bombType;
        public int count;
    }
    private BombCount[] _leftoverBombs;
    
    // RealBomb count (1 per stage)
    private int _realBombCount = 1;
    
    // Bomb data loaded from JSON
    private Dictionary<BombType, BombData> _bombDataDict;

    // Current selected index (-1 = not selected, 3 = real bomb)
    private int _currentIndex = -1;

    public void ClearBombs()
    {
        _currentIndex = -1;
        UpdateCheckedUI();
        if (auxiliaryBombSet != null)
        {
            foreach (Transform bomb in auxiliaryBombSet)
            {
                Destroy(bomb.gameObject);
            }
        }
        if (realBombSet != null)
        {
            foreach (Transform bomb in realBombSet)
            {
                Debug.Log("Destroying real bomb");
                Destroy(bomb.gameObject);
            }
        }
    }

    public void Initialize(GameObject auxiliaryBomb, GameObject realBombPrefab, 
        Transform auxiliaryBombSet, Transform realBombSet,
        TMP_Text _1stBombText, TMP_Text _2ndBombText, TMP_Text _3rdBombText, 
        GameObject _1stBombIcon, GameObject _2ndBombIcon, GameObject _3rdBombIcon, GameObject _realBombIcon,
        GameObject _1stBombChecked, GameObject _2ndBombChecked, GameObject _3rdBombChecked,
        GameObject realBombChecked, TMP_Text explodeButtonText,
        SaveData saveData)
    {
        this.auxiliaryBomb = auxiliaryBomb;
        this.realBombPrefab = realBombPrefab;
        this.auxiliaryBombSet = auxiliaryBombSet;
        this.realBombSet = realBombSet;
        this._1stBombText = _1stBombText;
        this._2ndBombText = _2ndBombText;
        this._3rdBombText = _3rdBombText;
        this._1stBombIcon = _1stBombIcon;
        this._2ndBombIcon = _2ndBombIcon;
        this._3rdBombIcon = _3rdBombIcon;
        this._realBombIcon = _realBombIcon;
        this._1stBombChecked = _1stBombChecked;
        this._2ndBombChecked = _2ndBombChecked;
        this._3rdBombChecked = _3rdBombChecked;
        this._realBombChecked = realBombChecked;
        _explodeButtonText = explodeButtonText;
        _boardManager = GameService.Get<BoardManager>();

        // Set initial explode button text
        if (_explodeButtonText != null)
            _explodeButtonText.text = "PASS";
        
        // Initialize leftover bombs from SaveData
        _leftoverBombs = new BombCount[3];
        _leftoverBombs[0] = new BombCount { bombType = saveData.firstBombType, count = saveData.left1stBomb };
        _leftoverBombs[1] = new BombCount { bombType = saveData.secondBombType, count = saveData.left2ndBomb };
        _leftoverBombs[2] = new BombCount { bombType = saveData.thirdBombType, count = saveData.left3rdBomb };

        this._1stBombIcon.AddComponent<SpriteRenderer>();
        this._2ndBombIcon.AddComponent<SpriteRenderer>();
        this._3rdBombIcon.AddComponent<SpriteRenderer>();
        this._realBombIcon.AddComponent<SpriteRenderer>();

        // Add colliders for click detection
        this._1stBombIcon.AddComponent<BoxCollider2D>();
        this._2ndBombIcon.AddComponent<BoxCollider2D>();
        this._3rdBombIcon.AddComponent<BoxCollider2D>();
        this._realBombIcon.AddComponent<BoxCollider2D>();
        
        // Initialize RealBomb count (1 per stage)
        _realBombCount = 1;
        
        // Load all bomb data from JSON
        LoadAllBombData();
        
        // Update UI
        UpdateAllBombTexts();
        
        // Initialize all check UIs to inactive
        UpdateCheckedUI();

        // Initialize all icon UIs
        UpdateIconUI();

        SpriteRenderer icon1 = this._1stBombIcon.GetComponent<SpriteRenderer>();
        SpriteRenderer icon2 = this._2ndBombIcon.GetComponent<SpriteRenderer>();
        SpriteRenderer icon3 = this._3rdBombIcon.GetComponent<SpriteRenderer>();
        SpriteRenderer icon4 = this._realBombIcon.GetComponent<SpriteRenderer>();
        float cellSize = _boardManager.GetCellSize();
        Vector2 spriteSize = icon1.sprite.bounds.size;
        float scaleX = cellSize / spriteSize.x;
        float scaleY = cellSize / spriteSize.y;
        float scale = Mathf.Min(scaleX, scaleY);  // Keep aspect ratio, fit within cell
        icon1.transform.localScale = Vector3.one * scale;
        icon2.transform.localScale = Vector3.one * scale;
        icon3.transform.localScale = Vector3.one * scale;
        icon4.transform.localScale = Vector3.one * scale;
    }
    
    // Load all bomb data from JSON files
    private void LoadAllBombData()
    {
        _bombDataDict = new Dictionary<BombType, BombData>();
        
        // Load each bomb type
        LoadBombData("1stBomb");
        LoadBombData("2ndBomb");
        LoadBombData("3rdBomb");
        LoadBombData("4thBomb");
        LoadBombData("5thBomb");
        LoadBombData("6thBomb");
        LoadBombData("SkyblueBomb");
        LoadBombData("RealBomb");
    }
    
    // Load single bomb data from JSON
    private void LoadBombData(string bombTypeName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, $"Json/Bomb/{bombTypeName}.json");
        if (!File.Exists(path))
        {
            Debug.LogError($"Failed to load {bombTypeName}.json from {path}");
            return;
        }
        
        string json = File.ReadAllText(path);
        BombData bombData = JsonUtility.FromJson<BombData>(json);
        bombData.fieldSprite = Resources.Load<Sprite>($"Sprites/Bomb/{bombData.fieldSpriteName}");
        bombData.iconSprite = Resources.Load<Sprite>($"Sprites/Bomb/{bombData.iconSpriteName}");
        BombType bombType = bombData.GetBombType();
        _bombDataDict[bombType] = bombData;
        
    }

    // Update specific bomb text UI by slot index
    private void UpdateBombText(int index)
    {
        if (index < 0 || index >= 3) return;
        
        switch (index)
        {
            case 0:
                if (_1stBombText != null)
                    _1stBombText.text = $"leftover: {_leftoverBombs[index].count}";
                break;
            case 1:
                if (_2ndBombText != null)
                    _2ndBombText.text = $"leftover: {_leftoverBombs[index].count}";
                break;
            case 2:
                if (_3rdBombText != null)
                    _3rdBombText.text = $"leftover: {_leftoverBombs[index].count}";
                break;
        }
    }
    
    // Update all bomb text UIs
    private void UpdateAllBombTexts()
    {
        for (int i = 0; i < 3; i++)
        {
            UpdateBombText(i);
        }
    }
    
    // Get bomb data by type
    public BombData GetBombData(BombType bombType)
    {
        if (_bombDataDict.TryGetValue(bombType, out BombData data))
        {
            return data;
        }
        return null;
    }
    
    // Get current selected bomb type (nullable)
    public BombType? GetCurrentBombType()
    {
        if (_currentIndex == 3)
            return BombType.RealBomb;
        return _leftoverBombs[_currentIndex].bombType;
    }
    
    public int GetCurrentIndex()
    {
        return _currentIndex;
    }

    // Check if a bomb type is selected
    public bool HasBombSelected()
    {
        return _currentIndex != -1;
    }
    
    public void SetCurrentIndex(int index)
    {
        _currentIndex = index;
        UpdateCheckedUI();
    }

    public void ClearCurrentBombType()
    {
        _currentIndex = -1;
        UpdateCheckedUI();
    }
    
    // Restore a bomb to inventory (used when removing placed bombs)
    public void RestoreBomb(BombType bombType)
    {
        for (int i = 0; i < 3; i++)
        {
            if (_leftoverBombs[i].bombType == bombType)
            {
                _leftoverBombs[i].count++;
                UpdateBombText(i);
                return;
            }
        }
    }

    public void RestoreBomb(int index)
    {
        _leftoverBombs[index].count++;
        UpdateBombText(index);
    }
    
    // Restore RealBomb to inventory
    public void RestoreRealBomb()
    {
        _realBombCount++;
    }
    
    // Get display name for bomb type
    private string GetBombDisplayName(BombType bombType)
    {
        switch (bombType)
        {
            case BombType.FirstBomb: return "1st";
            case BombType.SecondBomb: return "2nd";
            case BombType.ThirdBomb: return "3rd";
            case BombType.FourthBomb: return "4th";
            case BombType.FifthBomb: return "5th";
            case BombType.SixthBomb: return "6th";
            case BombType.SkyblueBomb: return "Skyblue";
            case BombType.RealBomb: return "Real";
            default: return bombType.ToString();
        }
    }
    
    // Check if selected bomb has any left (and show message if not)
    public bool CheckBombAvailable(int index, bool checkRealBomb)
    {
        bool available = false;
        
        if (checkRealBomb)
        {
            available = _realBombCount > 0;
        }
        else
        {
            available = _leftoverBombs[index].count > 0;
        }
        
        var gameManager = GameService.Get<GameManager>();
        if (!available && gameManager != null)
        {
            gameManager.ShowNoBombLeftMessage(GetBombDisplayName(_leftoverBombs[index].bombType));
        }
        
        return available;
    }
    
    // Reset explode button text to PASS (called after turn ends)
    public void ResetExplodeButtonText()
    {
        if (_explodeButtonText != null)
            _explodeButtonText.text = "PASS";
    }
    
    // Update checked UI based on current selection
    private void UpdateCheckedUI()
    {
        // Deactivate all check UIs first
        if (_1stBombChecked != null) _1stBombChecked.SetActive(false);
        if (_2ndBombChecked != null) _2ndBombChecked.SetActive(false);
        if (_3rdBombChecked != null) _3rdBombChecked.SetActive(false);
        if (_realBombChecked != null) _realBombChecked.SetActive(false);

        if (_currentIndex == -1)
            return;
        
        switch (_currentIndex)
        {
            case 0:
                if (_1stBombChecked != null) _1stBombChecked.SetActive(true);
                break;
            case 1:
                if (_2ndBombChecked != null) _2ndBombChecked.SetActive(true);
                break;
            case 2:
                if (_3rdBombChecked != null) _3rdBombChecked.SetActive(true);
                break;
            case 3:
                if (_realBombChecked != null) _realBombChecked.SetActive(true);
                break;
        }
    }

    private void UpdateIconUI()
    {
        for (int i = 0; i < 3; i++)
        {
            UpdateIcon(i);
        }
    }

    private void UpdateIcon(int index)
    {
        switch (index)
        {
            case 0:
                if (_1stBombIcon != null)
                {
                    Sprite icon = GetBombData(_leftoverBombs[index].bombType).iconSprite;
                    _1stBombIcon.GetComponent<SpriteRenderer>().sprite = icon;
                }
                break;
            case 1:
                if (_2ndBombIcon != null)
                {
                    Sprite icon = GetBombData(_leftoverBombs[index].bombType).iconSprite;
                    _2ndBombIcon.GetComponent<SpriteRenderer>().sprite = icon;
                }
                break;
            case 2:
                if (_3rdBombIcon != null)
                {
                    Sprite icon = GetBombData(_leftoverBombs[index].bombType).iconSprite;
                    _3rdBombIcon.GetComponent<SpriteRenderer>().sprite = icon;
                }
                break;
        }
    }
    
    // Planting auxiliary bomb API (call from GameManager)
    // Uses current selected bomb type
    public GameObject PlantAuxiliaryBomb(int x, int y)
    {
        if (_currentIndex == -1)
            return null;
            
        return PlantBomb(x, y, _currentIndex);
    }
    
    // Plant specific bomb type
    public GameObject PlantBomb(int x, int y, int index)
    {
        if (index < 0 || index >= 3) return null;
        
        BombData bombData = GetBombData(_leftoverBombs[index].bombType);
        if (bombData == null)
        {
            Debug.LogError($"Bomb data not found for type: {_leftoverBombs[index].bombType}");
            return null;
        }
        
        Vector3 worldPos = _boardManager.GridToWorld(x, y);
        GameObject bomb = Instantiate(auxiliaryBomb, worldPos, Quaternion.identity, auxiliaryBombSet);
        
        // Initialize bomb with data
        AuxiliaryBomb bombComponent = bomb.GetComponent<AuxiliaryBomb>();
        bombComponent.Initialize(bombData);
        
        // Apply scale based on cell size and sprite bounds
        SpriteRenderer sr = bomb.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float cellSize = _boardManager.GetCellSize();
            Vector2 spriteSize = sr.sprite.bounds.size;
            float scaleX = cellSize / spriteSize.x;
            float scaleY = cellSize / spriteSize.y;
            float scale = Mathf.Min(scaleX, scaleY);  // Keep aspect ratio, fit within cell
            bomb.transform.localScale = Vector3.one * scale;
        }

        // Decrease leftover count for this bomb type
        _leftoverBombs[index].count--;
        UpdateBombText(index);
        
        // Change explode button text to EXPLOSION
        if (_explodeButtonText != null)
            _explodeButtonText.text = "EXPLOSION";
        
        return bomb;
    }
    
    // Get remaining bombs count for specific type
    // public int GetLeftoverBomb(BombType bombType)
    // {
    //     if (_leftoverBombs.TryGetValue(bombType, out int count))
    //     {
    //         return count;
    //     }
    //     return 0;
    // }

    public int GetBombCount(int index)
    {
        return _leftoverBombs[index].count;
    }

    public BombType GetBombType(int index)
    {
        return _leftoverBombs[index].bombType;
    }
    
    // Get total remaining bombs count (all types)
    public int GetTotalLeftoverBombs()
    {
        int total = 0;
        for (int i = 0; i < 3; i++)
        {
            total += _leftoverBombs[i].count;
        }
        return total;
    }
    
    // Get planted (active) auxiliary bombs count
    public int GetPlantedAuxiliaryBombCount()
    {
        return auxiliaryBombSet.childCount;
    }
    
    // Get planted (active) real bombs count
    public int GetPlantedRealBombCount()
    {
        return realBombSet != null ? realBombSet.childCount : 0;
    }
    
    // Check if RealBomb is available
    public bool IsRealBombAvailable()
    {
        return _realBombCount > 0;
    }
    
    // Get RealBomb count
    public int GetRealBombCount()
    {
        return _realBombCount;
    }
    
    // Plant RealBomb at specified position
    public GameObject PlantRealBomb(int x, int y)
    {
        if (_realBombCount <= 0)
            return null;
        
        BombData bombData = GetBombData(BombType.RealBomb);
        if (bombData == null)
        {
            Debug.LogError("RealBomb data not found!");
            return null;
        }
        
        Vector3 worldPos = _boardManager.GridToWorld(x, y);
        GameObject bomb = Instantiate(realBombPrefab, worldPos, Quaternion.identity, realBombSet);
        
        // Initialize bomb with data
        RealBomb bombComponent = bomb.GetComponent<RealBomb>();
        if (bombComponent != null)
        {
            bombComponent.Initialize(bombData);
        }
        
        // Apply scale based on cell size and sprite bounds
        SpriteRenderer sr = bomb.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float cellSize = _boardManager.GetCellSize();
            Vector2 spriteSize = sr.sprite.bounds.size;
            float scaleX = cellSize / spriteSize.x;
            float scaleY = cellSize / spriteSize.y;
            float scale = Mathf.Min(scaleX, scaleY);  // Keep aspect ratio, fit within cell
            bomb.transform.localScale = Vector3.one * scale;
        }
        
        // Decrease RealBomb count
        _realBombCount--;
        
        // Change explode button text to EXPLOSION
        if (_explodeButtonText != null)
            _explodeButtonText.text = "EXPLOSION";
        
        return bomb;
    }
}
