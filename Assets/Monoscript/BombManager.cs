//#define USE_EDITOR

using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Entity;
using UnityEngine.UI;

public class BombManager : MonoBehaviour
{
    // Set via Initialize
    private GameObject auxiliaryBomb;
    private GameObject realBombPrefab;
    private Transform auxiliaryBombSet;
    private Transform realBombSet;
    
    // Text UI for each bomb type
    private TMP_Text _1stBombLeftoverText;
    private TMP_Text _2ndBombLeftoverText;
    private TMP_Text _3rdBombLeftoverText;

    private TMP_Text _1stBombNameText;
    private TMP_Text _2ndBombNameText;
    private TMP_Text _3rdBombNameText;

    // Icon UI for each bomb type
    private GameObject _1stBombIcon;
    private GameObject _2ndBombIcon;
    private GameObject _3rdBombIcon;
    private GameObject _realBombIcon;
    
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
        UpdateAllBombTexts();
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
        TMP_Text _1stBombLeftoverText, TMP_Text _2ndBombLeftoverText, TMP_Text _3rdBombLeftoverText, 
        TMP_Text _1stBombNameText, TMP_Text _2ndBombNameText, TMP_Text _3rdBombNameText, 
        GameObject _1stBombIcon, GameObject _2ndBombIcon, GameObject _3rdBombIcon, GameObject _realBombIcon,
        SaveData saveData, bool realBombEasyMode, bool realBombHardMode)
    {
        this.auxiliaryBomb = auxiliaryBomb;
        this.realBombPrefab = realBombPrefab;
        this.auxiliaryBombSet = auxiliaryBombSet;
        this.realBombSet = realBombSet;
        this._1stBombLeftoverText = _1stBombLeftoverText;
        this._2ndBombLeftoverText = _2ndBombLeftoverText;
        this._3rdBombLeftoverText = _3rdBombLeftoverText;
        this._1stBombNameText = _1stBombNameText;
        this._2ndBombNameText = _2ndBombNameText;
        this._3rdBombNameText = _3rdBombNameText;
        this._1stBombIcon = _1stBombIcon;
        this._2ndBombIcon = _2ndBombIcon;
        this._3rdBombIcon = _3rdBombIcon;
        this._realBombIcon = _realBombIcon;
        _boardManager = GameService.Get<BoardManager>();
        
        _leftoverBombs = new BombCount[3];

        // Initialize leftover bombs from SaveData
        Debug.Log($"Initializing BombManager with SaveData: 1stBombType={saveData.firstBombType}, left1stBomb={saveData.left1stBomb}, " +
                  $"2ndBombType={saveData.secondBombType}, left2ndBomb={saveData.left2ndBomb}, " +
                  $"3rdBombType={saveData.thirdBombType}, left3rdBomb={saveData.left3rdBomb}");
        _leftoverBombs[0] = new BombCount { bombType = saveData.firstBombType, count = saveData.left1stBomb };
        _leftoverBombs[1] = new BombCount { bombType = saveData.secondBombType, count = saveData.left2ndBomb };
        _leftoverBombs[2] = new BombCount { bombType = saveData.thirdBombType, count = saveData.left3rdBomb };

        // Initialize RealBomb count (1 per stage)
        _realBombCount = 1;
        
        // Load all bomb data from JSON
        LoadAllBombData();

        // apply difficulty
        if (realBombEasyMode)
        {
            _bombDataDict[BombType.RealBomb].range = 2;
        }
        else if (realBombHardMode)
        {
            _bombDataDict[BombType.RealBomb].range = 0;
        }
        
        // Update UI
        UpdateAllBombTexts();

        // Initialize all icon UIs
        UpdateIconUI();

        Image icon1 = this._1stBombIcon.GetComponent<Image>();
        Image icon2 = this._2ndBombIcon.GetComponent<Image>();
        Image icon3 = this._3rdBombIcon.GetComponent<Image>();
        Image icon4 = this._realBombIcon.GetComponent<Image>();
        Vector2 spriteSize = icon1.sprite.bounds.size;
        float scaleX = spriteSize.x;
        float scaleY = spriteSize.y;
        float scale = Mathf.Max(scaleX, scaleY);  // Keep aspect ratio, fit within cell
        // Hardcoded based on the relative size of the sprite
        icon1.transform.localScale = Vector3.one * scale * 0.23f;
        icon2.transform.localScale = Vector3.one * scale * 0.23f;
        icon3.transform.localScale = Vector3.one * scale * 0.23f;
        icon4.transform.localScale = Vector3.one * scale * 0.27f;
    }
    
    public void SetInitialBombCounts(int count1, int count2, int count3, BombType type1, BombType type2, BombType type3)
    {
        _leftoverBombs = new BombCount[3];
        _leftoverBombs[0] = new BombCount { bombType = type1, count = count1 };
        _leftoverBombs[1] = new BombCount { bombType = type2, count = count2 };
        _leftoverBombs[2] = new BombCount { bombType = type3, count = count3 };
        UpdateAllBombTexts();
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
        LoadRealBombData();
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

    private void LoadRealBombData()
    {
        string bombTypeName = "RealBomb";
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
        
        if (bombData.animationSpriteNames != null)
        {
            bombData.animationSprites = new List<Sprite>();
            foreach (string spriteName in bombData.animationSpriteNames)
            {
                Sprite sprite = Resources.Load<Sprite>($"Sprites/Bomb/{spriteName}");
                if (sprite != null)
                {
                    bombData.animationSprites.Add(sprite);
                }
            }
        }

        BombType bombType = bombData.GetBombType();
        _bombDataDict[bombType] = bombData;
    }



    // Update specific bomb text UI by slot index
    private void UpdateBombText(int index)
    {
        if (index < 0 || index >= 3) return;
        
        // Null safety checks
        if (_bombDataDict == null || _leftoverBombs == null) return;

        BombData bombData = _bombDataDict[_leftoverBombs[index].bombType];
        if (bombData == null) return;
        
        switch (index)
        {
            case 0:
                if (_1stBombLeftoverText == null || _1stBombNameText == null) return;
                _1stBombLeftoverText.text = $"{_leftoverBombs[index].count:D3}";
                if (_leftoverBombs[index].count == 0)
                {
                    _1stBombNameText.color = Color.red;
                }
                else if (_currentIndex == index)
                {
                    _1stBombNameText.color = Color.green;
                }
                else
                {
                    _1stBombNameText.color = Color.white;
                }
                _1stBombNameText.text = bombData.bombName;
                break;
            case 1:
                if (_2ndBombLeftoverText == null || _2ndBombNameText == null) return;
                _2ndBombLeftoverText.text = $"{_leftoverBombs[index].count:D3}";
                if (_leftoverBombs[index].count == 0)
                {
                    _2ndBombNameText.color = Color.red;
                }
                else if (_currentIndex == index)
                {
                    _2ndBombNameText.color = Color.green;
                }
                else
                {
                    _2ndBombNameText.color = Color.white;
                }
                _2ndBombNameText.text = bombData.bombName;
                break;
            case 2:
                if (_3rdBombLeftoverText == null || _3rdBombNameText == null) return;
                _3rdBombLeftoverText.text = $"{_leftoverBombs[index].count:D3}";
                if (_leftoverBombs[index].count == 0)
                {
                    _3rdBombNameText.color = Color.red;
                }
                else if (_currentIndex == index)
                {
                    _3rdBombNameText.color = Color.green;
                }
                else
                {
                    _3rdBombNameText.color = Color.white;
                }
                _3rdBombNameText.text = bombData.bombName;
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
        UpdateAllBombTexts();
    }

    public bool IsBombLeft(int index)
    {
        if (index == 3)
            return true;
        return _leftoverBombs[index].count > 0;
    }

    public void ClearCurrentBombType()
    {
        _currentIndex = -1;
        UpdateAllBombTexts();
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
        SoundManager.Instance.StopRealBombSound();
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
    // public void ResetExplodeButtonText()
    // {
    //     if (_explodeButtonText != null)
    //         _explodeButtonText.text = "PASS";
    // }
    
    // Update checked UI based on current selection
    // private void UpdateCheckedUI()
    // {
    //     _1stBombLeftoverText.color = Color.white;
    //     _2ndBombLeftoverText.color = Color.white;
    //     _3rdBombLeftoverText.color = Color.white;
    //     switch (_currentIndex)
    //     {
    //         case 0:
    //             _1stBombLeftoverText.color = Color.green;
    //             break;
    //         case 1:
    //             _2ndBombLeftoverText.color = Color.green;
    //             break;
    //         case 2:
    //             _3rdBombLeftoverText.color = Color.green;
    //             break;
    //         case 3:
    //             // TODO: specify that real bomb is selected
    //             break;
    //     }
    // }

    private void UpdateIconUI()
    {
        Debug.Log("Updating bomb icons");
        for (int i = 0; i < 3; i++)
        {
            UpdateIcon(i);
        }
        _realBombIcon.GetComponent<Image>().sprite = GetBombData(BombType.RealBomb).iconSprite;
    }

    private void UpdateIcon(int index)
    {
        Sprite icon;
        switch (index)
        {
            case 0:
                icon = GetBombData(_leftoverBombs[index].bombType).iconSprite;
                _1stBombIcon.GetComponent<Image>().sprite = icon;
                break;
            case 1:
                icon = GetBombData(_leftoverBombs[index].bombType).iconSprite;
                _2ndBombIcon.GetComponent<Image>().sprite = icon;
                break;
            case 2:
                icon = GetBombData(_leftoverBombs[index].bombType).iconSprite;
                _3rdBombIcon.GetComponent<Image>().sprite = icon;
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
        
        // Canvas UI mode only
        Vector2 canvasPos = _boardManager.GridToCanvasPosition(x, y);
        GameObject bomb = Instantiate(auxiliaryBomb, auxiliaryBombSet);
        
        // Setup RectTransform
        RectTransform rectTransform = bomb.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = bomb.AddComponent<RectTransform>();
        }
        rectTransform.anchoredPosition = canvasPos;
        
        // Set size based on cell size (converted to Canvas pixels)
        float cellSizeCanvas = _boardManager.GetCellSizeCanvas();
        // Scale the bomb to 70% of the cell size for better visual fit within the grid
        rectTransform.sizeDelta = new Vector2(cellSizeCanvas, cellSizeCanvas) * 0.7f;
        
        // Setup Image component for UI rendering
        Image image = bomb.GetComponent<Image>();
        if (image == null)
        {
            image = bomb.AddComponent<Image>();
        }
        if (bombData.fieldSprite != null)
        {
            image.sprite = bombData.fieldSprite;
        }
        image.raycastTarget = false;
        
        // Remove SpriteRenderer if exists
        SpriteRenderer sr = bomb.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Destroy(sr);
        }
        
        // Initialize bomb with data
        AuxiliaryBomb bombComponent = bomb.GetComponent<AuxiliaryBomb>();
        bombComponent.Initialize(bombData);

        // Decrease leftover count for this bomb type
        _leftoverBombs[index].count--;
        if (_leftoverBombs[index].count == 0)
        {
            _currentIndex = -1;
        }
        UpdateBombText(index);
        
        // Change explode button text to EXPLOSION
        // if (_explodeButtonText != null)
        //     _explodeButtonText.text = "EXPLOSION";
        
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
        
        // Canvas UI mode only
        Vector2 canvasPos = _boardManager.GridToCanvasPosition(x, y);
        GameObject bomb = Instantiate(realBombPrefab, realBombSet);
        
        // Setup RectTransform
        RectTransform rectTransform = bomb.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = bomb.AddComponent<RectTransform>();
        }
        rectTransform.anchoredPosition = canvasPos;
        
        // Set size based on cell size (converted to Canvas pixels)
        float cellSizeCanvas = _boardManager.GetCellSizeCanvas();
        rectTransform.sizeDelta = new Vector2(cellSizeCanvas, cellSizeCanvas);
        
        // Setup Image component for UI rendering
        Image image = bomb.GetComponent<Image>();
        if (image == null)
        {
            image = bomb.AddComponent<Image>();
        }
        if (bombData.fieldSprite != null)
        {
            image.sprite = bombData.fieldSprite;
        }
        image.raycastTarget = false;
        
        // Initialize bomb with data
        RealBomb bombComponent = bomb.GetComponent<RealBomb>();
        if (bombComponent != null)
        {
            bombComponent.Initialize(bombData);
        }
        
        // Decrease RealBomb count
        _realBombCount--;

        _currentIndex = -1;
        
        return bomb;
    }
}
