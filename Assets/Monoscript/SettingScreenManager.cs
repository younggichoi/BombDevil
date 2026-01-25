using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Entity;
using System.IO;

public class SettingScreenManager : MonoBehaviour
{
    

    private int _currentBombIndex = 0;
    private int _currentDifficultyIndex = 0;

    private List<int> _selectedBombIndices = new List<int>();
    private int _selectedDifficultyIndex = -1;

    // setting at the inspector
    public List<Sprite> bombIcons;
    public List<Sprite> difficultyIcons;
    public List<Sprite> bombSelectedIcons;

    public GameObject _bombSelectButton;
    public GameObject _bombSelectLeftButton;
    public GameObject _bombSelectRightButton;
    public GameObject _difficultySelectButton;
    public GameObject _difficultySelectLeftButton;
    public GameObject _difficultySelectRightButton;

    public GameObject _bombSelectedLeft;
    public GameObject _bombSelectedRight;
    public GameObject _bombSelectedMiddle;

    void Start()
    {
        UpdateBombButtonIcon();
        UpdateDifficultyButtonIcon();
    }

        void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);
        }
    }

    public void OnSettingButtonClicked()
    {
        // Validate selections
        if (_selectedDifficultyIndex == -1)
        {
            Debug.LogWarning("Please select a difficulty level.");
            return;
        }

        if (_selectedBombIndices.Count != 3)
        {
            Debug.LogWarning("Please select exactly 3 bombs.");
            return;
        }

        _selectedBombIndices.Sort();

        // Create SaveData with selected settings
        JsonDataUtility.ResetSaveData(1);
        SaveData initData = JsonDataUtility.LoadGameData(1); // TODO: remove hardcoding on file number
        //initData에서 폭탄과 아이템 개수, 점수는 그대로 두고 나머지만 변경
        initData.difficulty = _selectedDifficultyIndex;
        initData.firstBombType = (BombType)_selectedBombIndices[0];
        initData.secondBombType = (BombType)_selectedBombIndices[1];
        initData.thirdBombType = (BombType)_selectedBombIndices[2];
        /*SaveData initData = new SaveData
        {
            difficulty = _selectedDifficultyIndex,
            firstBombType = (BombType)_selectedBombIndices[0],
            secondBombType = (BombType)_selectedBombIndices[1],
            thirdBombType = (BombType)_selectedBombIndices[2],
            //Bomb counts hardcoded to 0 for now, can be modified later
            left1stBomb = 0,
            left2ndBomb = 0,
            left3rdBomb = 0,
            scoring = 0
        };

        // Add default items, hardcoded for now
        initData.leftItem.Add(new ItemCount(ItemType.Teleporter, 0));
        initData.leftItem.Add(new ItemCount(ItemType.Megaphone, 0));*/

        // Pass data to GameManager
        GameManager.pendingSaveData = initData;

        // Load IngameScene
        UnityEngine.SceneManagement.SceneManager.LoadScene("IngameScene");
    }

    public void OnBombSelectButtonClicked()
    {
        if (_selectedBombIndices.Contains(_currentBombIndex))
        {
            _selectedBombIndices.Remove(_currentBombIndex);
        }
        else if (_selectedBombIndices.Count < 3)
        {
            _selectedBombIndices.Add(_currentBombIndex);
        }
        UpdateBombButtonIcon();
    }

    public void OnBombSelectLeftButtonClicked()
    {
        _currentBombIndex--;
        if (_currentBombIndex < 0)
        {
            _currentBombIndex = bombIcons.Count - 1;
        }
        UpdateBombButtonIcon();
    }

    public void OnBombSelectRightButtonClicked()
    {
        _currentBombIndex++;
        if (_currentBombIndex >= bombIcons.Count)
        {
            _currentBombIndex = 0;
        }
        UpdateBombButtonIcon();
    }

    public void OnDifficultySelectButtonClicked()
    {
        if (_selectedDifficultyIndex == _currentDifficultyIndex)
        {
            _selectedDifficultyIndex = -1;
        }
        else
        {
            _selectedDifficultyIndex = _currentDifficultyIndex;
        }
        UpdateDifficultyButtonIcon();
    }

    public void OnDifficultySelectLeftButtonClicked()
    {
        _currentDifficultyIndex--;
        if (_currentDifficultyIndex < 0)
        {
            _currentDifficultyIndex = difficultyIcons.Count - 1;
        }
        UpdateDifficultyButtonIcon();
    }

    public void OnDifficultySelectRightButtonClicked()
    {
        _currentDifficultyIndex++;
        if (_currentDifficultyIndex >= difficultyIcons.Count)
        {
            _currentDifficultyIndex = 0;
        }
        UpdateDifficultyButtonIcon();
    }

    private void UpdateBombButtonIcon()
    {
        ChangeBombSprite(_bombSelectButton, bombIcons[_currentBombIndex]);
        if (_selectedBombIndices.Contains(_currentBombIndex))
        {
            ChangeBombIndexSprite(_bombSelectedMiddle, bombSelectedIcons[_selectedBombIndices.IndexOf(_currentBombIndex)], true);
        }
        else
        {
            RemoveBombIndexSprite(_bombSelectedMiddle);
        }
        int leftIndex = _currentBombIndex - 1;
        if (leftIndex < 0)
        {
            leftIndex = bombIcons.Count - 1;
        }
        int rightIndex = _currentBombIndex + 1;
        if (rightIndex >= bombIcons.Count)
        {
            rightIndex = 0;
        }
        ChangeBombSprite(_bombSelectLeftButton, bombIcons[leftIndex]);
        ChangeBombSprite(_bombSelectRightButton, bombIcons[rightIndex]);

        if (_selectedBombIndices.Contains(leftIndex))
        {
            ChangeBombIndexSprite(_bombSelectedLeft, bombSelectedIcons[_selectedBombIndices.IndexOf(leftIndex)], false);
        }
        else
        {
            RemoveBombIndexSprite(_bombSelectedLeft);
        }
        if (_selectedBombIndices.Contains(rightIndex))
        {
            ChangeBombIndexSprite(_bombSelectedRight, bombSelectedIcons[_selectedBombIndices.IndexOf(rightIndex)], false);
        }
        else
        {
            RemoveBombIndexSprite(_bombSelectedRight);
        }
    }

    private void UpdateDifficultyButtonIcon()
    {
        ChangeDifficultySprite(_difficultySelectButton, difficultyIcons[_currentDifficultyIndex], _currentDifficultyIndex);
        if (_currentDifficultyIndex == 0)
        {
            ChangeDifficultySprite(_difficultySelectLeftButton, difficultyIcons[difficultyIcons.Count - 1], difficultyIcons.Count - 1);
        }
        else
        {
            ChangeDifficultySprite(_difficultySelectLeftButton, difficultyIcons[_currentDifficultyIndex - 1], _currentDifficultyIndex - 1);
        }
        if (_currentDifficultyIndex == difficultyIcons.Count - 1)
        {
            ChangeDifficultySprite(_difficultySelectRightButton, difficultyIcons[0], 0);
        }
        else
        {
            ChangeDifficultySprite(_difficultySelectRightButton, difficultyIcons[_currentDifficultyIndex + 1], _currentDifficultyIndex + 1);
        }
    }

    private void ChangeBombSprite(GameObject button, Sprite sprite)
    {
        Image image = button.GetComponent<Image>();
        image.sprite = sprite;
        image.SetNativeSize();
        button.transform.localScale = Vector3.one * 0.3f;
    }

    private void ChangeBombIndexSprite(GameObject button, Sprite sprite, bool isMiddle)
    {
        Image image = button.GetComponent<Image>();
        image.sprite = sprite;
        image.SetNativeSize();
        button.transform.localScale = Vector3.one * 0.08f;
        if (isMiddle)
            image.color = Color.white;
        else
            image.color = Color.white * 0.7f; // gray color
    }

    private void RemoveBombIndexSprite(GameObject button)
    {
        Image image = button.GetComponent<Image>();
        image.sprite = null;
        image.color = Color.clear;
    }

    private void ChangeDifficultySprite(GameObject button, Sprite sprite, int index)
    {
        Image image = button.GetComponent<Image>();
        image.sprite = sprite;
        image.SetNativeSize();
        button.transform.localScale = Vector3.one * 0.25f;

        float transparency = image.color.a;
        if (_selectedDifficultyIndex == index)
        {
            image.color = Color.green;
        }
        else
        {
            image.color = Color.white;
        }
        image.color = new Color(image.color.r, image.color.g, image.color.b, transparency);
    }
}