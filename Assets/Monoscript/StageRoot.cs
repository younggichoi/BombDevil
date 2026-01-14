using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Entity;
using System.Collections.Generic;

public class StageRoot : MonoBehaviour
{
    // Manager Objects (found automatically)
    private GameManager gameManager;
    private EnemyManager enemyManager;
    private BombManager bombManager;
    private BoardManager boardManager;
    private ItemManager itemManager;
    
    // Prefabs and sprites (received from StageManager)
    private GameObject enemyPrefab;
    private GameObject auxiliaryBombPrefab;
    private GameObject realBombPrefab;
    private Sprite enemySprite;
    
    // Scene objects (found by name)
    private Transform enemySet;
    private Transform auxiliaryBombSet;
    private Transform realBombSet;
    private TMP_Text _1stBombText;
    private TMP_Text _2ndBombText;
    private TMP_Text _3rdBombText;
    private TMP_Text _4thBombText;
    private TMP_Text _5thBombText;
    private TMP_Text _6thBombText;
    private TMP_Text skyblueBombText;
    private TMP_Text realBombText;
    
    // Check UI objects (found by name)
    private GameObject _1stBombChecked;
    private GameObject _2ndBombChecked;
    private GameObject _3rdBombChecked;
    private GameObject _4thBombChecked;
    private GameObject _5thBombChecked;
    private GameObject _6thBombChecked;
    private GameObject skyblueBombChecked;
    private GameObject realBombChecked;

    // Teleporter and Megaphone buttons and texts
    private Button teleporterButton;
    private Button megaphoneButton;
    private Button removeButton;
    private Button resetButton;
    private Button exitButton;
    private TMP_Text teleporterButtonText;
    private TMP_Text megaphoneButtonText;
    
    // Explode button and text
    private Button explodeButton;
    private TMP_Text explodeButtonText;

    // Cached references for optimization
    private bool _isInitialized = false;
    private Transform itemSet;
    private Dictionary<ItemType, GameObject> itemPrefabs;
    private Dictionary<ItemType, TMP_Text> itemButtonTexts;
    private TMP_Text infoText;
    private TMP_Text stageText;
    private TMP_Text timeText;
    private TMP_Text turnText;
    
    // Public accessor for GameManager (used by StageManager)
    public GameManager GameManager => gameManager;
    

    public void Install(int stageId, IngameCommonData commonData, 
        GameObject enemyPrefab, GameObject auxiliaryBombPrefab, GameObject realBombPrefab, Sprite enemySprite)
    {
        // 1. One-time Init (Find objects, cache references, set listeners)
        if (!_isInitialized)
        {
            // Store prefabs and sprites (assuming they are common for all stages as per StageManager)
            this.enemyPrefab = enemyPrefab;
            this.auxiliaryBombPrefab = auxiliaryBombPrefab;
            this.realBombPrefab = realBombPrefab;
            this.enemySprite = enemySprite;
            
            // Find managers in children
            gameManager = GetComponentInChildren<GameManager>();
            enemyManager = GetComponentInChildren<EnemyManager>();
            bombManager = GetComponentInChildren<BombManager>();
            boardManager = GetComponentInChildren<BoardManager>();
            itemManager = GetComponentInChildren<ItemManager>();

            // Register all managers with the GameService
            GameService.Register(gameManager);
            GameService.Register(enemyManager);
            GameService.Register(bombManager);
            GameService.Register(boardManager);
            GameService.Register(itemManager);
            
            // Find item prefabs and UI
            ItemPrefabLibrary itemPrefabLibrary = GetComponentInChildren<ItemPrefabLibrary>();
            itemPrefabs = itemPrefabLibrary != null ? itemPrefabLibrary.GetPrefabDictionary() : new Dictionary<ItemType, GameObject>();
            itemSet = GameObject.Find("ItemSet")?.transform;
            
            teleporterButtonText = GameObject.Find("TeleporterButton")?.GetComponentInChildren<TMP_Text>();
            megaphoneButtonText = GameObject.Find("MegaphoneButton")?.GetComponentInChildren<TMP_Text>();
            
            itemButtonTexts = new Dictionary<ItemType, TMP_Text>
            {
                { ItemType.Teleporter, teleporterButtonText },
                { ItemType.Megaphone, megaphoneButtonText }
            };
            
            // Find scene objects by name
            enemySet = GameObject.Find("EnemySet")?.transform;
            auxiliaryBombSet = GameObject.Find("AuxiliaryBombSet")?.transform;
            realBombSet = GameObject.Find("RealBombSet")?.transform;
            _1stBombText = GameObject.Find("Leftover1stBomb")?.GetComponent<TMP_Text>();
            _2ndBombText = GameObject.Find("Leftover2ndBomb")?.GetComponent<TMP_Text>();
            _3rdBombText = GameObject.Find("Leftover3rdBomb")?.GetComponent<TMP_Text>();
            _4thBombText = GameObject.Find("Leftover4thBomb")?.GetComponent<TMP_Text>();
            _5thBombText = GameObject.Find("Leftover5thBomb")?.GetComponent<TMP_Text>();
            _6thBombText = GameObject.Find("Leftover6thBomb")?.GetComponent<TMP_Text>();
            skyblueBombText = GameObject.Find("LeftoverSkyblueBomb")?.GetComponent<TMP_Text>();
            realBombText = GameObject.Find("LeftoverRealBomb")?.GetComponent<TMP_Text>();
            
            // Find check UI objects by name
            _1stBombChecked = GameObject.Find("1stBombChecked");
            _2ndBombChecked = GameObject.Find("2ndBombChecked");
            _3rdBombChecked = GameObject.Find("3rdBombChecked");
            _4thBombChecked = GameObject.Find("4thBombChecked");
            _5thBombChecked = GameObject.Find("5thBombChecked");
            _6thBombChecked = GameObject.Find("6thBombChecked");
            skyblueBombChecked = GameObject.Find("SkyblueBombChecked");
            realBombChecked = GameObject.Find("RealBombChecked");
    
            teleporterButton = GameObject.Find("TeleporterButton")?.GetComponent<Button>();
            megaphoneButton = GameObject.Find("MegaphoneButton")?.GetComponent<Button>();
            removeButton = GameObject.Find("RemoveButton")?.GetComponent<Button>();
            resetButton = GameObject.Find("ResetButton")?.GetComponent<Button>();
            exitButton = GameObject.Find("ExitButton")?.GetComponent<Button>();
            
            // Find explode button and text
            GameObject explodeButtonObj = GameObject.Find("ExplodeButton");
            if (explodeButtonObj != null)
            {
                explodeButton = explodeButtonObj.GetComponent<Button>();
            }
            explodeButtonText = GameObject.Find("ExplodeButtonText")?.GetComponent<TMP_Text>();
            
            // Find UI for GameManager
            infoText = GameObject.Find("InfoText")?.GetComponent<TMP_Text>();
            stageText = GameObject.Find("StageText")?.GetComponent<TMP_Text>();
            timeText = GameObject.Find("TimeText")?.GetComponent<TMP_Text>();
            turnText = GameObject.Find("TurnText")?.GetComponent<TMP_Text>();

            // Connect Listeners (Once)
            if (explodeButton != null)
            {
                explodeButton.onClick.RemoveAllListeners();
                explodeButton.onClick.AddListener(() => GameService.Get<GameManager>()?.OnExplodeButtonClick());
            }
    
            if (teleporterButton != null)
            {
                teleporterButton.onClick.RemoveAllListeners();
                teleporterButton.onClick.AddListener(() => 
                    GameService.Get<GameManager>()?.OnItemButtonClicked(ItemType.Teleporter));
            }
    
            if (megaphoneButton != null)
            {
                megaphoneButton.onClick.RemoveAllListeners();
                megaphoneButton.onClick.AddListener(() => 
                    GameService.Get<GameManager>()?.OnItemButtonClicked(ItemType.Megaphone));
            }
            
            if (removeButton != null)
            {
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(() =>
                    GameService.Get<GameManager>()?.OnRemoveButtonClick());
            }
    
            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(() =>
                    GameService.Get<GameManager>()?.OnResetButtonClick());
            }
    
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(() => GameService.Get<GameManager>()?.OnExitButtonClick());
            }

            _isInitialized = true;
        }

        // 2. Per-Stage Logic (Reset dynamic state)

        // Initialize all managers with their scene references first.
        gameManager.Initialize(stageId, commonData);
        boardManager.Initialize(null); // Sprite is not used, can be null
        enemyManager.Initialize(this.enemyPrefab, enemySet, this.enemySprite);
        bombManager.Initialize(this.auxiliaryBombPrefab, this.realBombPrefab, 
            auxiliaryBombSet, realBombSet,
            _1stBombText, _2ndBombText, _3rdBombText, _4thBombText, _5thBombText, _6thBombText,
            skyblueBombText, realBombText,
            _1stBombChecked, _2ndBombChecked, _3rdBombChecked, _4thBombChecked, _5thBombChecked, _6thBombChecked,
            skyblueBombChecked, realBombChecked,
            explodeButtonText);
        itemManager.Initialize(itemPrefabs, itemSet, itemButtonTexts);

        // Now that all managers are initialized, clear the stage.
        gameManager.ClearStage();

        // Set UI texts in GameManager
        gameManager.SetInfoText(infoText);
        gameManager.SetStageStatsUI(stageText, timeText, turnText);

        enemyManager = GameService.Get<EnemyManager>();
        enemyManager.SetEnemyPrefab(enemyPrefab);
        enemyManager.SetEnemySprite(enemySprite);
        
        // Create enemies for this stage
        gameManager.CreateEnemy();
    }
    
    // Load sprite from Resources folder
    private Sprite LoadSprite(string path, string assetName)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"StageRoot: {assetName} sprite path is empty!");
            return null;
        }
        
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogError($"StageRoot: Failed to load {assetName} sprite from Resources/{path}");
        }
        return sprite;
    }
}
