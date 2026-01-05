using System.IO;
using Entity;
using UnityEngine;

public static class JsonDataUtility
{
    /// <summary>
    /// Saves StageDifferentData as a JSON file in the StreamingAssets/Json/Stage directory.
    /// </summary>
    /// <param name="data">The StageDifferentData to save.</param>
    public static void SaveStageData(StageDifferentData data)
    {
        if (data == null)
        {
            Debug.LogError("StageDifferentData is null. Cannot save.");
            return;
        }

        // Define the directory path within StreamingAssets
        string directoryPath = Path.Combine(Application.streamingAssetsPath, "Json", "Stage");

        // Create the directory if it doesn't exist
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Convert the object to a JSON string
        string json = JsonUtility.ToJson(data, true);

        // Define the file path, using stageId for a unique name
        string filePath = Path.Combine(directoryPath, $"stage_{data.stageId}.json");

        // Write the JSON string to the file
        File.WriteAllText(filePath, json);

        Debug.Log($"Stage data saved to: {filePath}");

        // Refresh the AssetDatabase to show the new file in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    /// <summary>
    /// Loads StageDifferentData from a JSON file in the StreamingAssets/Json/Stage directory.
    /// </summary>
    /// <param name="stageId">The ID of the stage to load.</param>
    /// <returns>The loaded StageDifferentData, or null if not found.</returns>
    public static StageDifferentData LoadStageData(int stageId)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Json", "Stage", $"stage_{stageId}.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<StageDifferentData>(json);
        }

        Debug.LogWarning($"Stage file not found: {filePath}. Creating a new default data object.");
        return new StageDifferentData { stageId = stageId };
    }

    /// <summary>
    /// Gets the count of stage JSON files in the StreamingAssets/Json/Stage directory.
    /// </summary>
    /// <returns>The number of stage files.</returns>
    public static int GetStageCount()
    {
        string directoryPath = Path.Combine(Application.streamingAssetsPath, "Json", "Stage");
        if (Directory.Exists(directoryPath))
        {
            return Directory.GetFiles(directoryPath, "*.json").Length;
        }
        return 0;
    }
}
