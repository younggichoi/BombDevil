using UnityEngine;
using System.Collections.Generic;
using Entity;

public class WallManager : MonoBehaviour
{
    private static GameObject _wallPrefab;
    private static GameObject _wallSet;
    private BoardManager _boardManager;
    private static Sprite _wallSprite;

    public void Initialize(GameObject wallPrefab, Sprite wallSprite)
    {
        _wallPrefab = wallPrefab;
        _wallSet = GameObject.Find("WallSet");
        _wallSprite = wallSprite;
    }

    public static GameObject CreateWall(int x, int y)
    {
        /*if (_wallPrefab == null)
        {
            _wallPrefab = Resources.Load<GameObject>("Prefab/Wall/Wall");
        }
        
        if (_wallSet == null)
        {
        }*/

        var boardManager = GameService.Get<BoardManager>();
        GameObject wallGO = Instantiate(_wallPrefab, boardManager.GridToWorld(x, y), Quaternion.identity);
        wallGO.transform.SetParent(_wallSet.transform);
        var wall = wallGO.AddComponent<Wall>();
        wall.Initialize(_wallSprite);
        return wallGO;
    }

    public static void DeleteAllWalls()
    {
        if (_wallSet == null)
        {
            _wallSet = GameObject.Find("WallSet");
        }

        foreach (Transform child in _wallSet.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public List<Vector2Int> GenerateRandomWallPositions(int wallNumber)
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x < _boardManager.GetWidth(); x++)
        {
            for (int y = 0; y < _boardManager.GetHeight(); y++)
            {
                if (true) // Placeholder for actual condition to check if the tile is empty
                {
                    availablePositions.Add(new Vector2Int(x, y));
                }
            }
        }

        List<Vector2Int> selectedPositions = new List<Vector2Int>();
        System.Random rand = new System.Random();
        for (int i = 0; i < wallNumber && availablePositions.Count > 0; i++)
        {
            int index = rand.Next(availablePositions.Count);
            selectedPositions.Add(availablePositions[index]);
            availablePositions.RemoveAt(index);
        }

        return selectedPositions;
    }
    
    private BoardManager BoardManager
    {
        get
        {
            if (_boardManager == null)
            {
                _boardManager = GameService.Get<BoardManager>();
            }
            return _boardManager;
        }
    }
}
