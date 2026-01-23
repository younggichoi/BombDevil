using UnityEngine;
using Entity;
using UnityEngine.UI;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    // setting option (common in all stages)
    public float walkDuration;
    public float knockbackDuration;
    public Color enemyColor;
    
    // Prefabs and sprites (assign in Inspector - common for all stages)
    public GameObject enemyPrefab;
    public GameObject auxiliaryBombPrefab;
    public GameObject realBombPrefab;
    public GameObject wallPrefab;
    public GameObject treasureChestPrefab;
    public GameObject itemIcon;
    public Sprite enemySprite;
    public Sprite stunnedEnemySprite;
    public Sprite fieldSprite;
    public Sprite wallSprite;
    public Sprite treasureChestSprite;

    // center position of board
    public float centerX;
    public float centerY;
    
    // StageRoot prefab
    public GameObject stageRootPrefab;
    // current stage
    private GameObject currStage;
    
    // stage tracking
    [SerializeField] private int _currentStageId = 1;
    [SerializeField] private int _maxStageId = 3;
    
    // current GameManager reference
    private GameManager _currentGameManager;

    void Start()
    {
        StartGame();

        // Find the reset button and add a listener
        Button resetButton = GameObject.Find("ResetButton")?.GetComponent<Button>();
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(RestartStage);
        }
        else
        {
            Debug.LogWarning("ResetButton not found in the scene!");
        }
    }
    
    // start game from stage 1
    public void StartGame()
    {
        _currentStageId = 1;
        StageInitialize(_currentStageId);
    }

    // init new stage
    public void StageInitialize(int stageId)
    {
        if (stageId == 1)
        {
            JsonDataUtility.ResetSaveData(1);
        }

        // Don't destroy the stage root, just prepare it
        if (currStage == null)
        {
            currStage = Instantiate(stageRootPrefab);
        }
        else
        {
             if (_currentGameManager != null)
            {
                _currentGameManager.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        IngameCommonData commonData = new IngameCommonData(walkDuration, knockbackDuration,
            enemyColor);
        
        StageRoot stageRoot = currStage.GetComponent<StageRoot>();

        stageRoot.Install(stageId, commonData, enemyPrefab, auxiliaryBombPrefab, realBombPrefab,
            wallPrefab, treasureChestPrefab, itemIcon, enemySprite, stunnedEnemySprite, fieldSprite,
            wallSprite, treasureChestSprite, centerX, centerY);
        
        // Subscribe to game state changed event
        _currentGameManager = stageRoot.GameManager;
        _currentGameManager.OnGameStateChanged += OnGameStateChanged;
        
        Debug.Log($"Stage {stageId} initialized");
    }

    // destroy current stage
    public void StageDestroy()
    {
        if (currStage != null)
        {
            // Unsubscribe from event before destroying
            if (_currentGameManager != null)
            {
                _currentGameManager.OnGameStateChanged -= OnGameStateChanged;
            }
            // Note: We no longer Destroy(currStage) here to reuse it.
            // Resources are cleared via stageRoot.Install -> GameManager.Initialize -> ClearStage
        }
    }
    
    // handle game state changes
    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Win:
                Debug.Log($"Stage {_currentStageId} cleared!");
                SaveGameProgress();
                NextStage();
                break;
            case GameState.Lose:
                Debug.Log("Game Over! Restarting stage...");
                RestartStage();
                break;
        }
    }

    private void SaveGameProgress()
    {
        SaveData saveData = new SaveData
        {
            left1stBomb = _currentGameManager.GetBombCount(0),
            left2ndBomb = _currentGameManager.GetBombCount(1),
            left3rdBomb = _currentGameManager.GetBombCount(2),
            firstBombType = _currentGameManager.GetBombType(0),
            secondBombType = _currentGameManager.GetBombType(1),
            thirdBombType = _currentGameManager.GetBombType(2),
            leftItem = GameService.Get<ItemManager>().GetRemainingItem(),
            scoring = _currentGameManager.GetScore()
        };
        JsonDataUtility.SaveGameData(saveData, 1); // Hardcoded to file1.json
    }
    
    // move to next stage
    public void NextStage()
    {
        if (_currentStageId < _maxStageId)
        {
            _currentStageId++;
            StageInitialize(_currentStageId);
        }
        else
        {
            Debug.Log("Congratulations! All stages cleared!");
            // TODO: Show game complete UI or return to main menu
        }
    }
    
    // restart current stage
    public void RestartStage()
    {
        StageInitialize(_currentStageId);
    }
    
    // get current stage ID
    public int GetCurrentStageId()
    {
        return _currentStageId;
    }
}
