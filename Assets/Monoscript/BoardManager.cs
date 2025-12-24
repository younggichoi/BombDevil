using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Single board sprite (created at runtime)
    private SpriteRenderer boardSprite;
    
    // Board prefab to instantiate
    private Sprite boardSpritePrefab;
    
    // fixed board size on screen (units)
    private float fixedBoardSize = 8f;
    
    // The grid size the board asset is designed for (7x7)
    private int assetGridSize = 7;
    
    // internal setting value (get from GameManager)
    private int _width;
    private int _height;
    
    // calculated cell size
    private float _cellSize;

    public void Initialize(GameManager gameManager, Sprite boardSpritePrefab)
    {
        this.boardSpritePrefab = boardSpritePrefab;
        _width = gameManager.GetWidth();
        _height = gameManager.GetHeight();
        
        // Set asset grid size to match logical grid (assumes board sprite matches logical grid)
        assetGridSize = Mathf.Max(_width, _height);
        
        // Calculate cell size based on largest dimension
        _cellSize = fixedBoardSize / Mathf.Max(_width, _height);
        
        // Create the board object in the scene
        CreateBoard();
        
        // Scale the board sprite to match the grid
        SetupBoardSprite();
    }
    
    // Create the board GameObject with SpriteRenderer
    private void CreateBoard()
    {
        if (boardSpritePrefab == null)
        {
            Debug.LogWarning("BoardManager: boardSpritePrefab is not assigned! Cannot create board.");
            return;
        }
        
        // Create a new GameObject for the board (standalone, not a child)
        GameObject boardObject = new GameObject("Board");
        boardObject.transform.position = Vector3.zero;
        
        // Add SpriteRenderer component and assign the sprite
        boardSprite = boardObject.AddComponent<SpriteRenderer>();
        boardSprite.sprite = boardSpritePrefab;
        
        // Set sorting order so board is behind other objects
        boardSprite.sortingOrder = -10;
        
        Debug.Log("BoardManager: Board object created successfully!");
    }
    
    // Setup single board sprite - scale 8x8 asset to match logical grid
    private void SetupBoardSprite()
    {
        if (boardSprite == null || boardSprite.sprite == null)
        {
            Debug.LogWarning("BoardManager: boardSprite or sprite is not assigned!");
            return;
        }
        
        // Calculate logical board size in world units
        float logicalBoardWidth = _width * _cellSize;
        float logicalBoardHeight = _height * _cellSize;
        
        // Get the sprite's original size in world units
        Vector2 spriteSize = boardSprite.sprite.bounds.size;
        
        // Calculate how much of the 8x8 asset we need based on logical grid
        // For example: if logical grid is 5x5, we need 5/8 of the asset
        float widthRatio = (float)_width / assetGridSize;
        float heightRatio = (float)_height / assetGridSize;
        
        // Calculate scale to make the visible portion match the logical grid
        // The asset represents 8x8 cells, we scale it so our logical grid cells align
        float scaleX = logicalBoardWidth / (spriteSize.x * widthRatio);
        float scaleY = logicalBoardHeight / (spriteSize.y * heightRatio);
        
        boardSprite.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        
        // Center the board at origin
        boardSprite.transform.position = Vector3.zero;
        
        Debug.Log($"BoardManager: Logical grid={_width}x{_height}, Asset grid={assetGridSize}x{assetGridSize}");
        Debug.Log($"BoardManager: cellSize={_cellSize}, boardSize={logicalBoardWidth}x{logicalBoardHeight}");
        Debug.Log($"BoardManager: Scale={boardSprite.transform.localScale}");
    }
    
    // grid coordinate -> world coordinate (public API)
    public Vector3 GridToWorld(int x, int y)
    {
        float xCoordination = (x - (_width - 1) / 2f) * _cellSize;
        float yCoordination = (y - (_height - 1) / 2f) * _cellSize;
        return new Vector3(xCoordination, yCoordination, 0);
    }
    
    // get cell size (for scaling objects)
    public float GetCellSize()
    {
        return _cellSize;
    }
    
    // get board dimensions
    public int GetWidth() => _width;
    public int GetHeight() => _height;
    
    // get boundary coordinates (scaled)
    public float GetMinX() => -_width / 2f * _cellSize;
    public float GetMaxX() => _width / 2f * _cellSize;
    public float GetMinY() => -_height / 2f * _cellSize;
    public float GetMaxY() => _height / 2f * _cellSize;
}
