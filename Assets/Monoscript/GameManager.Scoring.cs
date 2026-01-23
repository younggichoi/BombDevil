using UnityEngine;

public partial class GameManager : MonoBehaviour
{
    private void StageClearScoring()
    {
        float acquiredScoring = 1000.0f + 200.0f * _stageId;
        acquiredScoring *= (1.0f + (_elapsedTurns < 2 ? 1.0f : 0) + (_elapsedTime < 120 ? 0.5f : 0));
        _scoring += (int)acquiredScoring;
        Debug.Log($"Stage {_stageId} cleared! Acquired scoring: {acquiredScoring}, elapsedTurnsBonus: {(_elapsedTurns < 2)}, elapsedTimeBonus: {(_elapsedTime < 120)}");
    }
}