using TMPro;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    // Set via Initialize
    private GameObject auxiliaryBomb;
    private Transform auxiliaryBombSet;
    private TMP_Text leftoverAuxiliaryBombText;

    // BoardManager reference for coordinate conversion
    private BoardManager _boardManager;
    private int _leftoverAuxiliaryBomb;
    private Color _auxiliaryBombColor;

    public void Initialize(GameObject auxiliaryBomb, GameManager gameManager, Transform auxiliaryBombSet, TMP_Text leftoverAuxiliaryBombText, BoardManager boardManager)
    {
        this.auxiliaryBomb = auxiliaryBomb;
        this.auxiliaryBombSet = auxiliaryBombSet;
        this.leftoverAuxiliaryBombText = leftoverAuxiliaryBombText;
        _boardManager = boardManager;
        _leftoverAuxiliaryBomb = gameManager.GetInitialAuxiliaryBomb();
        _auxiliaryBombColor = gameManager.GetAuxiliaryBombColor();
        leftoverAuxiliaryBombText.text = $"leftover: {_leftoverAuxiliaryBomb}";
    }
    
    // planting auxiliary bomb API (call from GameManager)
    public GameObject PlantAuxiliaryBomb(int x, int y)
    {
        if (_leftoverAuxiliaryBomb <= 0)
            return null;
        
        Vector3 worldPos = _boardManager.GridToWorld(x, y);
        GameObject bomb = Instantiate(auxiliaryBomb, worldPos, Quaternion.identity, auxiliaryBombSet);
        bomb.GetComponent<SpriteRenderer>().color = _auxiliaryBombColor;
        
        // Apply scale based on cell size
        float cellSize = _boardManager.GetCellSize();
        bomb.transform.localScale = Vector3.one * cellSize;

        _leftoverAuxiliaryBomb--;
        leftoverAuxiliaryBombText.text = $"leftover: {_leftoverAuxiliaryBomb}";
        
        return bomb;
    }
    
    // get remaining auxiliary bombs count
    public int GetLeftoverAuxiliaryBomb()
    {
        return _leftoverAuxiliaryBomb;
    }
    
    // get planted (active) auxiliary bombs count
    public int GetPlantedAuxiliaryBombCount()
    {
        return auxiliaryBombSet.childCount;
    }
}

