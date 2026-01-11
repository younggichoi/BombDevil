using UnityEngine;
using System.Collections.Generic;
using Entity;

public class WallManager : MonoBehaviour
{
    public List<Vector2Int> GenerateRandomWallPositions(int wallNumber, BoardManager boardManager)
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x < boardManager.GetWidth(); x++)
        {
            for (int y = 0; y < boardManager.GetHeight(); y++)
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
}
