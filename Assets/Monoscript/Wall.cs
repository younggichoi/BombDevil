using UnityEngine;

public class Wall : MonoBehaviour
{
    public void Initialize(Sprite sprite)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        
        sr.sprite = sprite;
        
        // Scale sprite to fit exactly one cell
        var boardManager = GameService.Get<BoardManager>();
        if (boardManager != null && sprite != null)
        {
            float cellSize = boardManager.GetCellSize();
            Vector2 spriteSize = sprite.bounds.size;
            float scaleX = cellSize / spriteSize.x;
            float scaleY = cellSize / spriteSize.y;
            float scale = Mathf.Min(scaleX, scaleY); // Keep aspect ratio, fit within cell
            transform.localScale = Vector3.one * scale;
        }
    }
}
