using UnityEngine;

public class Megaphone : MonoBehaviour
{
    public static Vector2Int? activeMegaphonePosition;

    private void OnEnable()
    {
        var boardManager = GameService.Get<BoardManager>();
        if (boardManager != null)
        {
            activeMegaphonePosition = boardManager.WorldToGrid(transform.position);
        }
    }

    private void OnDisable()
    {
        var boardManager = GameService.Get<BoardManager>();
        if (boardManager != null)
        {
            Vector2Int currentGridPos = boardManager.WorldToGrid(transform.position);
            if (activeMegaphonePosition == currentGridPos)
            {
                activeMegaphonePosition = null;
            }
        }
    }
}
