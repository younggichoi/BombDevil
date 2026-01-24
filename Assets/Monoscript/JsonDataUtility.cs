//#define USE_EDITOR

using System.IO;
using Entity;
using UnityEngine;

public static class JsonDataUtility
{
    public static void SaveStageData(StageData data)
    {
        if (data == null)
        {
            Debug.LogError("StageData is null. Cannot save.");
            return;
        }

        string directoryPath = Path.Combine(Application.streamingAssetsPath, "Json", "Stage");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string json = JsonUtility.ToJson(data, true);
        string filePath = Path.Combine(directoryPath, $"stage{data.stageId}.json");
        File.WriteAllText(filePath, json);
        Debug.Log($"Stage data saved to: {filePath}");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public static StageData LoadStageData(int stageId)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Json", "Stage", $"stage{stageId}.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<StageData>(json);
        }

        Debug.LogWarning($"Stage file not found: {filePath}. Creating a new default data object.");
        return new StageData { stageId = stageId };
    }

    public static int GetStageCount()
    {
        string directoryPath = Path.Combine(Application.streamingAssetsPath, "Json", "Stage");
        if (Directory.Exists(directoryPath))
        {
            return Directory.GetFiles(directoryPath, "*.json").Length;
        }
        return 0;
    }

    public static void SaveGameData(SaveData data, int fileNo)
    {
        if (data == null)
        {
            Debug.LogError("SaveData is null. Cannot save.");
            return;
        }

        string directoryPath = Path.Combine(Application.streamingAssetsPath, "Json", "Save");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string json = JsonUtility.ToJson(data, true);
        string filePath = Path.Combine(directoryPath, $"file{fileNo}.json");
        File.WriteAllText(filePath, json);
        Debug.Log($"Save data saved to: {filePath}");
    }

    public static SaveData LoadGameData(int fileNo)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Json", "Save", $"file{fileNo}.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return data;
        }

        Debug.LogWarning($"Save file not found: {filePath}. Creating a new default save data object.");
        return new SaveData();
    }

    public static void SaveStageEditorData(StageEditorData data)
    {
        if (data == null)
        {
            Debug.LogError("StageEditorData is null. Cannot save.");
            return;
        }

        string directoryPath = Path.Combine(Application.streamingAssetsPath, "Json", "StageEditor");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string json = JsonUtility.ToJson(data, true);
        string filePath = Path.Combine(directoryPath, $"stage{data.stageId}.json");
        File.WriteAllText(filePath, json);
        Debug.Log($"Stage editor data saved to: {filePath}");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

    }

    public static StageEditorData LoadStageEditorData(int stageId)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Json", "StageEditor", $"stage{stageId}.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<StageEditorData>(json);
        }

        Debug.LogWarning($"Stage editor file not found: {filePath}. Creating a new default data object.");
        return new StageEditorData { stageId = stageId };
    }

    public static void ResetSaveData(int fileNo)
    {
        string initFilePath = Path.Combine(Application.streamingAssetsPath, "Json", "Run", "init.json");
        string saveFilePath = Path.Combine(Application.streamingAssetsPath, "Json", "Save", $"file{fileNo}.json");

        if (File.Exists(initFilePath))
        {
            string json = File.ReadAllText(initFilePath);
            
            string directoryPath = Path.GetDirectoryName(saveFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Save data file {fileNo} has been reset from init.json.");
        }
        else
        {
            Debug.LogError($"Initial data file not found: {initFilePath}");
        }
    }

#if USE_EDITOR
    public static SaveData LoadInitData(int stageId)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Json", "Run", $"init.json");
        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<SaveData>(json);
    }
#endif
}
