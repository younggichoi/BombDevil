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
    private TMP_Text _4thBombText;
    private TMP_Text _5thBombText;
    private TMP_Text _6thBombText;
    private TMP_Text _skyblueBombText;
    private TMP_Text _realBombText;
    
    // Check UI for each bomb type (shows which bomb is selected)
    private GameObject _1stBombChecked;
    private GameObject _2ndBombChecked;
    private GameObject _3rdBombChecked;
    private GameObject _4thBombChecked;
    private GameObject _5thBombChecked;
    private GameObject _6thBombChecked;
    private GameObject _skyblueBombChecked;
    private GameObject _realBombChecked;

    // Explode button text
    private TMP_Text _explodeButtonText;

    // BoardManager reference for coordinate conversion
    private BoardManager _boardManager;
    
    // Leftover bomb counts per type
    private Dictionary<BombType, int> _leftoverBombs;
    
    // RealBomb count (1 per stage)
    private int _realBombCount = 1;
    
    // Bomb data loaded from JSON
    private Dictionary<BombType, BombData> _bombDataDict;
    
    // Current selected bomb type (null = not selected)
    private BombType? _currentBombType = null;
    
    // GameManager reference for info messages
    private GameManager _gameManager;

    public void ClearBombs()
    {
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

    public void Initialize(GameObject auxiliaryBomb, GameObject realBombPrefab, GameManager gameManager, 
        Transform auxiliaryBombSet, Transform realBombSet,
        TMP_Text _1stBombText, TMP_Text _2ndBombText, TMP_Text _3rdBombText, 
        TMP_Text _4thBombText, TMP_Text _5thBombText, TMP_Text _6thBombText,
        TMP_Text skyblueBombText, TMP_Text realBombText,
        GameObject _1stBombChecked, GameObject _2ndBombChecked, GameObject _3rdBombChecked,
        GameObject _4thBombChecked, GameObject _5thBombChecked, GameObject _6thBombChecked,
        GameObject skyblueBombChecked, GameObject realBombChecked,
        TMP_Text explodeButtonText, BoardManager boardManager)
    {
        this.auxiliaryBomb = auxiliaryBomb;
        this.realBombPrefab = realBombPrefab;
        this.auxiliaryBombSet = auxiliaryBombSet;
        this.realBombSet = realBombSet;
        this._1stBombText = _1stBombText;
        this._2ndBombText = _2ndBombText;
        this._3rdBombText = _3rdBombText;
        this._4thBombText = _4thBombText;
        this._5thBombText = _5thBombText;
        this._6thBombText = _6thBombText;
        _skyblueBombText = skyblueBombText;
        _realBombText = realBombText;
        this._1stBombChecked = _1stBombChecked;
        this._2ndBombChecked = _2ndBombChecked;
        this._3rdBombChecked = _3rdBombChecked;
        this._4thBombChecked = _4thBombChecked;
        this._5thBombChecked = _5thBombChecked;
        this._6thBombChecked = _6thBombChecked;
        _skyblueBombChecked = skyblueBombChecked;
        _realBombChecked = realBombChecked;
        _explodeButtonText = explodeButtonText;
        _boardManager = boardManager;
        _gameManager = gameManager;

        // Set initial explode button text
        if (_explodeButtonText != null)
            _explodeButtonText.text = "PASS";
        
        // Initialize leftover bombs from GameManager
        _leftoverBombs = new Dictionary<BombType, int>
        {
            { BombType.FirstBomb, gameManager.GetInitialBombCount(BombType.FirstBomb) },
            { BombType.SecondBomb, gameManager.GetInitialBombCount(BombType.SecondBomb) },
            { BombType.ThirdBomb, gameManager.GetInitialBombCount(BombType.ThirdBomb) },
            { BombType.FourthBomb, gameManager.GetInitialBombCount(BombType.FourthBomb) },
            { BombType.FifthBomb, gameManager.GetInitialBombCount(BombType.FifthBomb) },
            { BombType.SixthBomb, gameManager.GetInitialBombCount(BombType.SixthBomb) },
            { BombType.SkyblueBomb, gameManager.GetInitialBombCount(BombType.SkyblueBomb) }
        };
        
        // Initialize RealBomb count (1 per stage)
        _realBombCount = 1;
        
        // Load all bomb data from JSON
        LoadAllBombData();
        
        // Update UI
        UpdateAllBombTexts();
        
        // Initialize all check UIs to inactive
        UpdateCheckedUI();
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
        BombType bombType = bombData.GetBombType();
        _bombDataDict[bombType] = bombData;
        
    }
    
    // Update all bomb text UIs
    private void UpdateAllBombTexts()
    {
        UpdateBombText(BombType.FirstBomb);
        UpdateBombText(BombType.SecondBomb);
        UpdateBombText(BombType.ThirdBomb);
        UpdateBombText(BombType.FourthBomb);
        UpdateBombText(BombType.FifthBomb);
        UpdateBombText(BombType.SixthBomb);
        UpdateBombText(BombType.SkyblueBomb);
        UpdateRealBombText();
    }
    
    // Update specific bomb text UI
    private void UpdateBombText(BombType bombType)
    {
        if (!_leftoverBombs.TryGetValue(bombType, out int count))
            return;
        
        switch (bombType)
        {
            case BombType.FirstBomb:
                if (_1stBombText != null)
                    _1stBombText.text = $"1st: {count}";
                break;
            case BombType.SecondBomb:
                if (_2ndBombText != null)
                    _2ndBombText.text = $"2nd: {count}";
                break;
            case BombType.ThirdBomb:
                if (_3rdBombText != null)
                    _3rdBombText.text = $"3rd: {count}";
                break;
            case BombType.FourthBomb:
                if (_4thBombText != null)
                    _4thBombText.text = $"4th: {count}";
                break;
            case BombType.FifthBomb:
                if (_5thBombText != null)
                    _5thBombText.text = $"5th: {count}";
                break;
            case BombType.SixthBomb:
                if (_6thBombText != null)
                    _6thBombText.text = $"6th: {count}";
                break;
            case BombType.SkyblueBomb:
                if (_skyblueBombText != null)
                    _skyblueBombText.text = $"Sky: {count}";
                break;
        }
    }
    
    // Update RealBomb text UI
    private void UpdateRealBombText()
    {
        if (_realBombText != null)
            _realBombText.text = $"Real: {_realBombCount}";
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
        return _currentBombType;
    }
    
    // Check if a bomb type is selected
    public bool HasBombSelected()
    {
        return _currentBombType.HasValue;
    }
    
    // Set current bomb type (for UI selection)
    public void SetCurrentBombType(BombType bombType)
    {
        _currentBombType = bombType;
        UpdateCheckedUI();
        
        // Show selection message via GameManager
        if (_gameManager != null)
        {
            string bombName = GetBombDisplayName(bombType);
            _gameManager.ShowBombSelectedMessage(bombName);
        }
    }

    public void ClearCurrentBombType()
    {
        _currentBombType = null;
        UpdateCheckedUI();
    }
    
    // Restore a bomb to inventory (used when removing placed bombs)
    public void RestoreBomb(BombType bombType)
    {
        if (_leftoverBombs.ContainsKey(bombType))
        {
            _leftoverBombs[bombType]++;
            UpdateBombText(bombType);
        }
    }
    
    // Restore RealBomb to inventory
    public void RestoreRealBomb()
    {
        _realBombCount++;
        UpdateRealBombText();
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
    public bool CheckBombAvailable(BombType bombType)
    {
        bool available = false;
        
        if (bombType == BombType.RealBomb)
        {
            available = _realBombCount > 0;
        }
        else if (_leftoverBombs.TryGetValue(bombType, out int count))
        {
            available = count > 0;
        }
        
        if (!available && _gameManager != null)
        {
            _gameManager.ShowNoBombLeftMessage(GetBombDisplayName(bombType));
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
        if (_4thBombChecked != null) _4thBombChecked.SetActive(false);
        if (_5thBombChecked != null) _5thBombChecked.SetActive(false);
        if (_6thBombChecked != null) _6thBombChecked.SetActive(false);
        if (_skyblueBombChecked != null) _skyblueBombChecked.SetActive(false);
        if (_realBombChecked != null) _realBombChecked.SetActive(false);
        
        // Activate the selected one
        if (_currentBombType.HasValue)
        {
            switch (_currentBombType.Value)
            {
                case BombType.FirstBomb:
                    if (_1stBombChecked != null) _1stBombChecked.SetActive(true);
                    break;
                case BombType.SecondBomb:
                    if (_2ndBombChecked != null) _2ndBombChecked.SetActive(true);
                    break;
                case BombType.ThirdBomb:
                    if (_3rdBombChecked != null) _3rdBombChecked.SetActive(true);
                    break;
                case BombType.FourthBomb:
                    if (_4thBombChecked != null) _4thBombChecked.SetActive(true);
                    break;
                case BombType.FifthBomb:
                    if (_5thBombChecked != null) _5thBombChecked.SetActive(true);
                    break;
                case BombType.SixthBomb:
                    if (_6thBombChecked != null) _6thBombChecked.SetActive(true);
                    break;
                case BombType.SkyblueBomb:
                    if (_skyblueBombChecked != null) _skyblueBombChecked.SetActive(true);
                    break;
                case BombType.RealBomb:
                    if (_realBombChecked != null) _realBombChecked.SetActive(true);
                    break;
            }
        }
    }
    
    // Planting auxiliary bomb API (call from GameManager)
    // Uses current selected bomb type
    public GameObject PlantAuxiliaryBomb(int x, int y)
    {
        if (!_currentBombType.HasValue)
            return null;
            
        return PlantBomb(x, y, _currentBombType.Value);
    }
    
    // Plant specific bomb type
    public GameObject PlantBomb(int x, int y, BombType bombType)
    {
        // Check if we have leftover bombs of this type
        if (!_leftoverBombs.TryGetValue(bombType, out int leftover) || leftover <= 0)
            return null;
        
        BombData bombData = GetBombData(bombType);
        if (bombData == null)
        {
            Debug.LogError($"Bomb data not found for type: {bombType}");
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
        _leftoverBombs[bombType]--;
        UpdateBombText(bombType);
        
        // Change explode button text to EXPLOSION
        if (_explodeButtonText != null)
            _explodeButtonText.text = "EXPLOSION";
        
        return bomb;
    }
    
    // Get remaining bombs count for specific type
    public int GetLeftoverBomb(BombType bombType)
    {
        if (_leftoverBombs.TryGetValue(bombType, out int count))
        {
            return count;
        }
        return 0;
    }
    
    // Get total remaining bombs count (all types)
    public int GetTotalLeftoverBombs()
    {
        int total = 0;
        foreach (var kvp in _leftoverBombs)
        {
            total += kvp.Value;
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
        UpdateRealBombText();
        
        // Change explode button text to EXPLOSION
        if (_explodeButtonText != null)
            _explodeButtonText.text = "EXPLOSION";
        
        return bomb;
    }
}
