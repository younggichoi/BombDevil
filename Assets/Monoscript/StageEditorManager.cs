using System.IO;
using Entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageEditorManager : MonoBehaviour
{
    public Button SaveButton;
    public Button ExitButton;
    public TMP_InputField WidthInput;
    public TMP_InputField HeightInput;
    public TMP_InputField EnemyNumberInput;
    public TMP_InputField Initial1stBombInput;
    public TMP_InputField Initial2ndBombInput;
    public TMP_InputField Initial3rdBombInput;
    public TMP_InputField InitialTeleporterInput;    
    public TMP_InputField InitialMegaphoneInput;
    public TMP_InputField RemainingTurnsInput;
    public TMP_InputField WallNumberInput;
    public TMP_Dropdown StageDropdown;
    public TextMeshProUGUI StageLabel;
    private int _stageId = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StageLabel.text = "Editing Stage " + _stageId;
        
        int stageCount = JsonDataUtility.GetStageCount();
        StageDropdown.options.Clear();
        for (int i = 1; i <= stageCount; i++)
        {
            StageDropdown.options.Add(new TMP_Dropdown.OptionData("Stage " + i));
        }
        
        StageDropdown.onValueChanged.AddListener(delegate { OnValueChanged(); });
        SaveButton.onClick.AddListener(OnSaveButtonClicked);
        ExitButton.onClick.AddListener(() => { UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene"); });
        
        LoadStageDataAndUpdateUI(_stageId);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnValueChanged()
    {
        _stageId = StageDropdown.value + 1;
        StageLabel.text = "Editing Stage " + _stageId;
        LoadStageDataAndUpdateUI(_stageId);
    }

    void LoadStageDataAndUpdateUI(int stageId)
    {
        var stageData = JsonDataUtility.LoadStageEditorData(stageId);
        if (stageData != null)
        {
            WidthInput.text = stageData.width.ToString();
            HeightInput.text = stageData.height.ToString();
            EnemyNumberInput.text = stageData.enemyNumber.ToString();
            Initial1stBombInput.text = stageData.initial1stBomb.ToString();
            Initial2ndBombInput.text = stageData.initial2ndBomb.ToString();
            Initial3rdBombInput.text = stageData.initial3rdBomb.ToString();
            InitialTeleporterInput.text = stageData.initialTeleporter.ToString();
            InitialMegaphoneInput.text = stageData.initialMegaphone.ToString();
            WallNumberInput.text = stageData.wallNumber.ToString();
            RemainingTurnsInput.text = stageData.remainingTurns.ToString();
        }
    }

    void OnSaveButtonClicked()
    {
        StageEditorData stageData = new StageEditorData
        {
            stageId = _stageId,
            width = int.Parse(WidthInput.text),
            height = int.Parse(HeightInput.text),
            enemyNumber = int.Parse(EnemyNumberInput.text),
            initial1stBomb = int.Parse(Initial1stBombInput.text),
            initial2ndBomb = int.Parse(Initial2ndBombInput.text),
            initial3rdBomb = int.Parse(Initial3rdBombInput.text),
            initialTeleporter = int.Parse(InitialTeleporterInput.text),
            initialMegaphone = int.Parse(InitialMegaphoneInput.text),
            wallNumber = int.Parse(WallNumberInput.text),
            remainingTurns = int.Parse(RemainingTurnsInput.text)
        };
        JsonDataUtility.SaveStageEditorData(stageData);
    }
}
