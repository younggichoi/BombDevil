using System.Collections.Generic;
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
    private TMP_Text _blueBombText;
    private TMP_Text _greenBombText;
    private TMP_Text _pinkBombText;
    private TMP_Text _realBombText;
    
    // Check UI for each bomb type (shows which bomb is selected)
    private GameObject _blueBombChecked;
    private GameObject _greenBombChecked;
    private GameObject _pinkBombChecked;
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

    public void Initialize(GameObject auxiliaryBomb, GameObject realBombPrefab, GameManager gameManager, 
        Transform auxiliaryBombSet, Transform realBombSet,
        TMP_Text blueBombText, TMP_Text greenBombText, TMP_Text pinkBombText, TMP_Text realBombText,
        GameObject blueBombChecked, GameObject greenBombChecked, GameObject pinkBombChecked, GameObject realBombChecked,
        TMP_Text explodeButtonText, BoardManager boardManager)
    {
        this.auxiliaryBomb = auxiliaryBomb;
        this.realBombPrefab = realBombPrefab;
        this.auxiliaryBombSet = auxiliaryBombSet;
        this.realBombSet = realBombSet;
        _blueBombText = blueBombText;
        _greenBombText = greenBombText;
        _pinkBombText = pinkBombText;
        _realBombText = realBombText;
        _blueBombChecked = blueBombChecked;
        _greenBombChecked = greenBombChecked;
        _pinkBombChecked = pinkBombChecked;
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
            { BombType.BlueBomb, gameManager.GetInitialBombCount(BombType.BlueBomb) },
            { BombType.GreenBomb, gameManager.GetInitialBombCount(BombType.GreenBomb) },
            { BombType.PinkBomb, gameManager.GetInitialBombCount(BombType.PinkBomb) }
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
        LoadBombData("BlueBomb");
        LoadBombData("GreenBomb");
        LoadBombData("PinkBomb");
        LoadBombData("RealBomb");
    }
    
    // Load single bomb data from JSON
    private void LoadBombData(string bombTypeName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Json/Bomb/{bombTypeName}");
        if (jsonFile == null)
        {
            Debug.LogError($"Failed to load {bombTypeName}.json from Resources/Json/Bomb/");
            return;
        }
        
        BombData bombData = JsonUtility.FromJson<BombData>(jsonFile.text);
        BombType bombType = bombData.GetBombType();
        _bombDataDict[bombType] = bombData;
        
        Debug.Log($"Loaded bomb data: {bombTypeName} (range: {bombData.range}, knockback: {bombData.knockbackDistance})");
    }
    
    // Update all bomb text UIs
    private void UpdateAllBombTexts()
    {
        UpdateBombText(BombType.BlueBomb);
        UpdateBombText(BombType.GreenBomb);
        UpdateBombText(BombType.PinkBomb);
        UpdateRealBombText();
    }
    
    // Update specific bomb text UI
    private void UpdateBombText(BombType bombType)
    {
        int count = _leftoverBombs[bombType];
        
        switch (bombType)
        {
            case BombType.BlueBomb:
                if (_blueBombText != null)
                    _blueBombText.text = $"Blue: {count}";
                break;
            case BombType.GreenBomb:
                if (_greenBombText != null)
                    _greenBombText.text = $"Green: {count}";
                break;
            case BombType.PinkBomb:
                if (_pinkBombText != null)
                    _pinkBombText.text = $"Pink: {count}";
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
    
    // Get display name for bomb type
    private string GetBombDisplayName(BombType bombType)
    {
        switch (bombType)
        {
            case BombType.BlueBomb: return "Blue";
            case BombType.GreenBomb: return "Green";
            case BombType.PinkBomb: return "Pink";
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
        if (_blueBombChecked != null) _blueBombChecked.SetActive(false);
        if (_greenBombChecked != null) _greenBombChecked.SetActive(false);
        if (_pinkBombChecked != null) _pinkBombChecked.SetActive(false);
        if (_realBombChecked != null) _realBombChecked.SetActive(false);
        
        // Activate the selected one
        if (_currentBombType.HasValue)
        {
            switch (_currentBombType.Value)
            {
                case BombType.BlueBomb:
                    if (_blueBombChecked != null) _blueBombChecked.SetActive(true);
                    break;
                case BombType.GreenBomb:
                    if (_greenBombChecked != null) _greenBombChecked.SetActive(true);
                    break;
                case BombType.PinkBomb:
                    if (_pinkBombChecked != null) _pinkBombChecked.SetActive(true);
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
        
        // Apply scale based on cell size
        float cellSize = _boardManager.GetCellSize();
        bomb.transform.localScale = Vector3.one * cellSize;

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
        
        // Apply scale based on cell size
        float cellSize = _boardManager.GetCellSize();
        bomb.transform.localScale = Vector3.one * cellSize;
        
        // Decrease RealBomb count
        _realBombCount--;
        UpdateRealBombText();
        
        // Change explode button text to EXPLOSION
        if (_explodeButtonText != null)
            _explodeButtonText.text = "EXPLOSION";
        
        return bomb;
    }
}
