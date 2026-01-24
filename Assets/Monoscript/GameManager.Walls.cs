using UnityEngine;

public partial class GameManager : MonoBehaviour
{
    public void CreateWall()
    {
        DeleteWall();
        int currentWall = 0;
        while (currentWall < _wallNumber)
        {
            int x = UnityEngine.Random.Range(0, _width);
            int y = UnityEngine.Random.Range(0, _height);
            if (_board[x, y].Count == 0)
            {
                GameObject wall = WallManager.CreateWall(x, y);
                _board[x, y].Add(wall);
                currentWall++;
                Debug.Log($"Wall created at ({x}, {y})");
            }
        }
    }

    public void DeleteWall()
    {
        WallManager.DeleteAllWalls();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _board[x, y].RemoveAll(obj => obj == null || obj.GetComponent<Wall>() != null);
            }
        }
    }
}
