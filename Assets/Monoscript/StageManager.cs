using UnityEngine;
using Entity;

public class StageManager : MonoBehaviour
{
    // setting option (common in all stages)
    public int knockbackDistance;
    public float walkDuration;
    public float knockbackDuration;
    public Color enemyColor;
    public Color auxiliaryBombColor;
    
    // Prefabs and sprites (assign in Inspector - common for all stages)
    public GameObject enemyPrefab;
    public GameObject auxiliaryBombPrefab;
    public Sprite enemySprite;
    
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
        StageDestroy();
        currStage = Instantiate(stageRootPrefab);
        StageCommonData commonData = new StageCommonData(walkDuration, knockbackDuration, knockbackDistance,
            enemyColor, auxiliaryBombColor);
        
        StageRoot stageRoot = currStage.GetComponent<StageRoot>();
        stageRoot.Install(stageId, commonData, enemyPrefab, auxiliaryBombPrefab, enemySprite);
        
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
                _currentGameManager = null;
            }
            Destroy(currStage);
        }
    }
    
    // handle game state changes
    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Win:
                Debug.Log($"Stage {_currentStageId} cleared!");
                NextStage();
                break;
            case GameState.Lose:
                Debug.Log("Game Over! Restarting stage...");
                RestartStage();
                break;
        }
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
