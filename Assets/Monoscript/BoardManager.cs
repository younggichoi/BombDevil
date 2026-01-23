using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    // Grid of tile objects
    private GameObject[,] _tiles;
    
    // Parent object for board UI
    private GameObject _boardObject;
    
    // Canvas reference for UI-based coordinate system
    private RectTransform _canvasRectTransform;
    private Canvas _canvas;
    private Camera _canvasCamera;
    
    // fixed board size on screen (in World Space units)
    private float _fixedBoardSizeWorld = 8f;
    
    // internal setting value (get from GameManager)
    private int _width;
    private int _height;
    
    // calculated cell size (in World Space units)
    private float _cellSize;

    // center position of board (World Space coordinates)
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

    /// <summary>
    /// Initialize board with Canvas UI system
    /// All coordinates are in World Space, will be converted to Canvas coordinates internally
    /// </summary>
    public void Initialize(RectTransform parentCanvas, Sprite fieldSprite, float centerX, float centerY)
    {
        ClearBoard();

        var gameManager = GameService.Get<GameManager>();
        if (gameManager == null) return;

        _canvasRectTransform = parentCanvas;
        _canvas = parentCanvas.GetComponentInParent<Canvas>();
        _canvasCamera = _canvas.worldCamera;
        
        _width = gameManager.GetWidth();
        _height = gameManager.GetHeight();
        _centerX = centerX;
        _centerY = centerY;
        
        // Calculate cell size in World Space units
        _cellSize = _fixedBoardSizeWorld / Mathf.Max(_width, _height);

        // Create Board Background as UI Image
        _boardObject = new GameObject("BoardBackground");
        _boardObject.transform.SetParent(_canvasRectTransform, false);
        
        RectTransform boardRect = _boardObject.AddComponent<RectTransform>();
        
        // Convert World Space center and size to Canvas coordinates
        Vector2 canvasCenter = WorldToCanvas(new Vector3(centerX, centerY, 0));
        float worldBoardWidth = _width * _cellSize;
        float worldBoardHeight = _height * _cellSize;
        Vector2 canvasBoardSize = WorldSizeToCanvasSize(worldBoardWidth, worldBoardHeight);
        
        boardRect.anchoredPosition = canvasCenter;
        boardRect.sizeDelta = canvasBoardSize;
        
        Image boardImage = _boardObject.AddComponent<Image>();
        boardImage.sprite = fieldSprite;
        boardImage.type = Image.Type.Sliced;
        boardImage.raycastTarget = false;
        
        // Set as first sibling to render behind other UI elements
        _boardObject.transform.SetAsFirstSibling();

        _boardObject.transform.position += new Vector3(0.135f, -0.14f, 0);

        _cellSize *= 0.89f;
        
        Debug.Log($"BoardManager: Initialized Canvas UI board {_width}x{_height}, worldCellSize={_cellSize}, center=({centerX}, {centerY}), canvasCenter={canvasCenter}, canvasSize={canvasBoardSize}");
    }
    
    /// <summary>
    /// Convert World Space position to Canvas anchoredPosition
    /// </summary>
    public Vector2 WorldToCanvas(Vector3 worldPosition)
    {
        if (_canvasCamera == null || _canvasRectTransform == null)
        {
            Debug.LogWarning("BoardManager: Canvas or camera not set for Canvas UI mode");
            return Vector2.zero;
        }
        
        // Convert world position to screen position
        Vector2 screenPoint = _canvasCamera.WorldToScreenPoint(worldPosition);
        
        // Convert screen position to canvas local position
        Vector2 canvasPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            screenPoint,
            _canvasCamera,
            out canvasPoint
        );
        
        return canvasPoint;
    }
    
    /// <summary>
    /// Convert World Space size to Canvas size
    /// </summary>
    public Vector2 WorldSizeToCanvasSize(float worldWidth, float worldHeight)
    {
        if (_canvasCamera == null || _canvasRectTransform == null)
        {
            return new Vector2(worldWidth * 100, worldHeight * 100); // Fallback
        }
        
        // Get two world points that define the size
        Vector3 worldOrigin = new Vector3(_centerX - worldWidth / 2, _centerY - worldHeight / 2, 0);
        Vector3 worldEnd = new Vector3(_centerX + worldWidth / 2, _centerY + worldHeight / 2, 0);
        
        // Convert to canvas positions
        Vector2 canvasOrigin = WorldToCanvas(worldOrigin);
        Vector2 canvasEnd = WorldToCanvas(worldEnd);
        
        return new Vector2(Mathf.Abs(canvasEnd.x - canvasOrigin.x), Mathf.Abs(canvasEnd.y - canvasOrigin.y));
    }
    
    /// <summary>
    /// Get cell size in Canvas pixels
    /// </summary>
    public float GetCellSizeCanvas()
    {
        Vector2 canvasCellSize = WorldSizeToCanvasSize(_cellSize, _cellSize);
        return Mathf.Min(canvasCellSize.x, canvasCellSize.y);
    }
    
    /// <summary>
    /// Grid coordinate -> Canvas anchoredPosition (for UI elements)
    /// </summary>
    public Vector2 GridToCanvasPosition(int x, int y)
    {
        Vector3 worldPos = GridToWorld(x, y);
        return WorldToCanvas(worldPos);
    }
    
    /// <summary>
    /// Canvas anchoredPosition -> Grid coordinate
    /// </summary>
    public Vector2Int CanvasToGrid(Vector2 canvasPosition)
    {
        if (_canvasCamera == null || _canvasRectTransform == null)
        {
            return Vector2Int.zero;
        }
        
        // Convert canvas position back to screen position
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            _canvasCamera,
            _canvasRectTransform.TransformPoint(canvasPosition)
        );
        
        // Convert screen position to world position
        Vector3 worldPos = _canvasCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 0));
        worldPos.z = 0;
        
        return WorldToGrid(worldPos);
    }
    
    /// <summary>
    /// Grid coordinate -> World coordinate (used internally for coordinate conversion)
    /// </summary>
    public Vector3 GridToWorld(int x, int y)
    {
        float xCoordination = (x - (_width - 1) / 2f) * _cellSize;
        float yCoordination = (y - (_height - 1) / 2f) * _cellSize;
        return new Vector3(xCoordination + _centerX, yCoordination + _centerY, 0);
    }

    /// <summary>
    /// World coordinate -> Grid coordinate (used internally for coordinate conversion)
    /// </summary>
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
    
    /// <summary>
    /// Get cell size in World Space units
    /// </summary>
    public float GetCellSize()
    {
        return _cellSize;
    }
    
    /// <summary>
    /// Get parent Canvas RectTransform
    /// </summary>
    public RectTransform GetParentCanvas() => _canvasRectTransform;
    
    /// <summary>
    /// Get Canvas Camera
    /// </summary>
    public Camera GetCanvasCamera() => _canvasCamera;
    
    // get board dimensions
    public int GetWidth() => _width;
    public int GetHeight() => _height;
    
    // get boundary coordinates (scaled, including center offset)
    public float GetMinX() => -_width / 2f * _cellSize + _centerX;
    public float GetMaxX() => _width / 2f * _cellSize + _centerX;
    public float GetMinY() => -_height / 2f * _cellSize + _centerY;
    public float GetMaxY() => _height / 2f * _cellSize + _centerY;
    
    // get center position
    public float GetCenterX() => _centerX;
    public float GetCenterY() => _centerY;
}
