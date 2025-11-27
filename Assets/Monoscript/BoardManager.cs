using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject tile;
    public Transform board;
    public GameManager gameManager;
    
    // internal setting value (get from GameManager)
    private int _width;
    private int _height;
    private Color _tileColor1;
    private Color _tileColor2;

    void Awake()
    {
        _width = gameManager.width;
        _height = gameManager.height;
        _tileColor1 = gameManager.tileColor1;
        _tileColor2 = gameManager.tileColor2;
    }
    
    // lay tiles
    void Start()
    {
        int tileXIndex;
        int tileYIndex;
        for (tileXIndex = 0; tileXIndex < _width; tileXIndex++)
        {
            for (tileYIndex = 0; tileYIndex < _height; tileYIndex++)
            {
                if (tileXIndex % 2 == tileYIndex % 2)
                    CreateTile(tileXIndex, tileYIndex, _tileColor1);
                else
                    CreateTile(tileXIndex, tileYIndex, _tileColor2);
            }
        }
    }

    // create tile object in (x,y)
    private void CreateTile(int x, int y, Color color)
    {
        GameObject tileObj = Instantiate(tile, CalculatePosition(x, y),  Quaternion.identity, board);
        tileObj.GetComponent<SpriteRenderer>().color = color;
    }
    
    // grid coordinate -> global coordinate
    private Vector3 CalculatePosition(int x, int y)
    {
        float xCoordination = x - (_width - 1) / 2f;
        float yCoordination = y - (_height - 1) / 2f;
        return new Vector3(xCoordination, yCoordination, 0);
    }
}
