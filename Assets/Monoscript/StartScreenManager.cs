using UnityEngine;

public class StartScreenManager : MonoBehaviour
{
    private UnityEngine.UI.Button StartButton;
    private UnityEngine.UI.Button EditorButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        StartButton = GameObject.Find("NewGameButton").GetComponent<UnityEngine.UI.Button>();
        StartButton.onClick.AddListener(OnStartButtonClicked);
        EditorButton = GameObject.Find("EditorButton").GetComponent<UnityEngine.UI.Button>();
        EditorButton.onClick.AddListener(OnEditorButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);
        }
    }

    void OnStartButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SettingScene");
    }

    void OnEditorButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StageEditorScene");
    }
}
