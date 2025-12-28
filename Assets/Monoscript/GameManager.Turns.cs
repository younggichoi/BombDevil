using UnityEngine;
using System.Collections;
using Entity;

public partial class GameManager : MonoBehaviour
{
    public void OnExplodeButtonClick()
    {
        if (_currentState != GameState.Playing || _isTurnInProgress)
            return;
        StartCoroutine(ExecuteTurnSequence());
    }

    private IEnumerator ExecuteTurnSequence()
    {
        _isTurnInProgress = true;
        _realBombUsedThisTurn = false;
        if (_tempMessageCoroutine != null)
        {
            StopCoroutine(_tempMessageCoroutine);
            _tempMessageCoroutine = null;
        }
        SetInfoMessage("Exploding...");
        yield return StartCoroutine(ExplodeAllBombsCoroutine());
        if (_realBombUsedThisTurn && GetEnemyCount() > 0)
        {
            yield return new WaitForSeconds(1f);
            SetInfoMessage("Game Over");
            SetGameState(GameState.Lose);
            yield return new WaitForSeconds(2f);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            yield break;
        }
        SetInfoMessage("Enemy's turn");
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(MoveAllEnemiesCoroutine());
        _remainingTurns--;
        if (_turnText != null)
            _turnText.text = $"Turns: {_remainingTurns}";
        if (_remainingTurns <= 0 && GetEnemyCount() > 0)
        {
            yield return new WaitForSeconds(1f);
            SetInfoMessage("Game Over");
            SetGameState(GameState.Lose);
            yield return new WaitForSeconds(2f);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            yield break;
        }
        bombManager.ResetExplodeButtonText();
        SetInfoMessage("Player's turn");
        CheckGameState();
        _isTurnInProgress = false;
    }

    private IEnumerator ExplodeAllBombsCoroutine()
    {
        while (_allBombs.Count > 0)
        {
            var bombInfo = _allBombs[0];
            _allBombs.RemoveAt(0);
            Vector2Int bombCoordinate = bombInfo.coord;
            bool isRealBomb = bombInfo.isRealBomb;
            int x = bombCoordinate.x;
            int y = bombCoordinate.y;
            if (isRealBomb)
            {
                _realBombs.Remove(bombCoordinate);
                _realBombUsedThisTurn = true;
                GameObject bombObj = _board[x, y].Find(obj => obj != null && obj.GetComponent<RealBomb>() != null);
                if (bombObj == null)
                    continue;
                RealBomb bomb = bombObj.GetComponent<RealBomb>();
                int range = bomb.GetRange();
                _board[x, y].Remove(bombObj);
                bomb.Explode();
                KillEnemiesInRange(x, y, range);
                yield return new WaitForSeconds(_knockbackDuration);
            }
            else
            {
                _auxiliaryBombs.Remove(bombCoordinate);
                GameObject bombObj = _board[x, y].Find(obj => obj != null && obj.GetComponent<AuxiliaryBomb>() != null);
                if (bombObj == null)
                    continue;
                AuxiliaryBomb bomb = bombObj.GetComponent<AuxiliaryBomb>();
                int range = bomb.GetRange();
                int knockbackDistance = bomb.GetKnockbackDistance();
                BombType bombType = bomb.GetBombType();
                _board[x, y].Remove(bombObj);
                bomb.Explode();
                switch (bombType)
                {
                    case BombType.BlueBomb:
                    case BombType.GreenBomb:
                        Debug.Log($"Knockback bomb at ({x}, {y}) with range {range} and knockback distance {knockbackDistance}");
                        Knockback(x, y, range, knockbackDistance);
                        break;
                    case BombType.PinkBomb:
                        break;
                    case BombType.SkyblueBomb:
                        break;
                    default:
                        break;
                }
                yield return new WaitForSeconds(_knockbackDuration);
            }
        }
    }

    private IEnumerator MoveAllEnemiesCoroutine()
    {
        System.Collections.Generic.List<(int x, int y, GameObject obj, Enemy enemy)> enemies = new System.Collections.Generic.List<(int, int, GameObject, Enemy)>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                foreach (var obj in _board[x, y])
                {
                    if (obj != null)
                    {
                        Enemy enemy = obj.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            enemies.Add((x, y, obj, enemy));
                        }
                    }
                }
            }
        }
        foreach (var (x, y, obj, enemy) in enemies)
        {
            Vector2 dirAndDist = enemy.GetMoveDirection();
            enemy.MoveInDirection();
            ReflectMoveInBoard(x, y, obj, dirAndDist);
        }
        if (enemies.Count > 0)
        {
            yield return new WaitForSeconds(_walkDuration);
        }
    }
}
