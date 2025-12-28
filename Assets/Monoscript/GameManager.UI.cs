using TMPro;
using UnityEngine;
using System.Collections;

public partial class GameManager : MonoBehaviour
{
    private void SetInfoMessage(string message)
    {
        if (_infoText != null)
            _infoText.text = message;
    }

    private void ShowTempMessage(string message, float duration, string restoreMessage)
    {
        if (_tempMessageCoroutine != null)
            StopCoroutine(_tempMessageCoroutine);
        _tempMessageCoroutine = StartCoroutine(TempMessageCoroutine(message, duration, restoreMessage));
    }

    private IEnumerator TempMessageCoroutine(string message, float duration, string restoreMessage)
    {
        SetInfoMessage(message);
        yield return new WaitForSeconds(duration);
        SetInfoMessage(restoreMessage);
        _tempMessageCoroutine = null;
    }

    public void ShowBombSelectedMessage(string bombName)
    {
        ShowTempMessage($"{bombName} bomb is selected!", 1f, "Player's turn");
    }

    public void ShowNoBombLeftMessage(string bombName)
    {
        ShowTempMessage($"There's no {bombName} bomb left!", 1f, "Player's turn");
    }

    private void UpdateStageStatsUI()
    {
        if (_stageText != null)
            _stageText.text = $"Stage {_stageId}";
        if (_turnText != null)
            _turnText.text = $"Turns: {_remainingTurns}";
        UpdateTimeText();
    }

    private void UpdateTimeText()
    {
        if (_timeText != null)
        {
            int hours = (int)(_elapsedTime / 3600);
            int minutes = (int)((_elapsedTime % 3600) / 60);
            int seconds = (int)(_elapsedTime % 60);
            _timeText.text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }
}
