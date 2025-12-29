using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Entity;

public class ItemPrefabLibrary : MonoBehaviour
{
    public GameObject teleporterPrefab;
    public GameObject megaphonePrefab;
    // Add more item prefabs as needed

    public Dictionary<ItemType, GameObject> GetPrefabDictionary()
    {
        var dict = new Dictionary<ItemType, GameObject>();
        dict[ItemType.Teleporter] = teleporterPrefab;
        dict[ItemType.Megaphone] = megaphonePrefab;
        // Add more as needed
        return dict;
    }
}
