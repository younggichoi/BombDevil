using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Grid of tile objects
    private GameObject[,] _tiles;
    
    // Parent object for tiles
    private GameObject _boardObject;
    
    // fixed board size on screen (units)
    private float fixedBoardSize = 8f;
    
    // internal setting value (get from GameManager)
    private int _width;
    private int _height;
    
    // calculated cell size
    private float _cellSize;
    
    // Tile colors for checkerboard pattern
    private Color _lightTileColor = new Color(0.9f, 0.9f, 0.85f);
    private Color _darkTileColor = new Color(0.7f, 0.75f, 0.65f);

    // center position of board
    private float _centerX;
    private float _centerY;

    public void ClearBoard()
    {
        if (_boardObject != null)
        {
            Destroy(_boardObject);
        }
        _tiles = null;
    }

    // x and y are correction values based on the sprite's relative position
    public void Initialize(Sprite fieldSprite, float x, float y)
    {
        ClearBoard();

        var gameManager = GameService.Get<GameManager>();
        if (gameManager == null) return;

        _width = gameManager.GetWidth();
        _height = gameManager.GetHeight();

        _centerX = x;
        _centerY = y;
        
        // Calculate cell size based on largest dimension
        _cellSize = fixedBoardSize / Mathf.Max(_width, _height);

        _boardObject = new GameObject("BoardBackground");
        // Hardcoded correction values based on the fieldSprite's characteristics
        _boardObject.transform.position = new Vector3(x + 0.175f, y - 0.165f, 0);
        SpriteRenderer boardRenderer = _boardObject.AddComponent<SpriteRenderer>();
        boardRenderer.sprite = fieldSprite;
        boardRenderer.sortingOrder = -10;
        
        // Create the tile grid 
        CreateTileGrid();
    }
    
    // Create individual tiles for the grid
    private void CreateTileGrid()
    {
        Debug.Log("BoardManager: Creating tile grid");
        
        _tiles = new GameObject[_width, _height];
        
        // Create a simple square sprite for tiles
        // Sprite tileSprite = CreateSquareSprite();
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                GameObject tile = new GameObject($"Tile_{x}_{y}");
                
                
                // Position the tile
                tile.transform.position = GridToWorld(x, y);
                tile.transform.SetParent(_boardObject.transform);
                tile.transform.localScale = Vector3.one * _cellSize * 0.95f; // Slight gap between tiles
                
                // Add sprite renderer
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                // sr.sprite = tileSprite;
                sr.sortingOrder = -10;
                
                // Apply checkerboard pattern
                // bool isLight = (x + y) % 2 == 0;
                // sr.color = isLight ? _lightTileColor : _darkTileColor;
                
                _tiles[x, y] = tile;
            }
        }
        
        Debug.Log($"BoardManager: Created {_width}x{_height} tile grid");
    }
    
    // Create a simple square sprite for tiles
    private Sprite CreateSquareSprite()
    {
        Debug.Log("BoardManager: Creating square sprite for tiles");
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.white;
        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
    }
    
    // grid coordinate -> world coordinate (public API)
    public Vector3 GridToWorld(int x, int y)
    {
        float xCoordination = (x - (_width - 1) / 2f) * _cellSize;
        float yCoordination = (y - (_height - 1) / 2f) * _cellSize;
        return new Vector3(xCoordination + _centerX, yCoordination + _centerY, 0);
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        float xCoordination = worldPosition.x - _centerX;
        float yCoordination = worldPosition.y - _centerY;
        int x = Mathf.RoundToInt((xCoordination / _cellSize) + (_width - 1) / 2f);
        int y = Mathf.RoundToInt((yCoordination / _cellSize) + (_height - 1) / 2f);
        return new Vector2Int(x, y);
    }

    public bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }
    
    // get cell size (for scaling objects)
    public float GetCellSize()
    {
        return _cellSize;
    }
    
    // get board dimensions
    public int GetWidth() => _width;
    public int GetHeight() => _height;
    
    // get boundary coordinates (scaled, including center offset)
    public float GetMinX() => -_width / 2f * _cellSize + _centerX;
    public float GetMaxX() => _width / 2f * _cellSize + _centerX;
    public float GetMinY() => -_height / 2f * _cellSize + _centerY;
    public float GetMaxY() => _height / 2f * _cellSize + _centerY;
}
