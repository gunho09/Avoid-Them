using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    
    // 인스펙터에서 모든 아이템 데이터를 할당해줘야 함 (또는 Resources 폴더 로드)
    public List<ItemData> allItems = new List<ItemData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public ItemData GetRandomItem()
    {
        // 1. 등급 결정 (60 / 30 / 10)
        float rand = Random.Range(0f, 100f);
        ItemRarity targetRarity = ItemRarity.Normal;
        
        if (rand < 10f) targetRarity = ItemRarity.Legend;
        else if (rand < 40f) targetRarity = ItemRarity.Rare;
        else targetRarity = ItemRarity.Normal;

        // 2. 해당 등급 아이템 풀 생성
        List<ItemData> pool = allItems.FindAll(x => x.rarity == targetRarity);
        
        // (만약 해당 등급 아이템이 하나도 없으면 전체에서 랜덤)
        if (pool.Count == 0) pool = allItems;

        // 3. 랜덤 선택
        if (pool.Count > 0)
        {
            return pool[Random.Range(0, pool.Count)];
        }
        
        return null; // 데이터가 하나도 없으면
    }

    // 3개의 랜덤 아이템 뽑기 (독립 시행)
    public List<ItemData> GetRandomItems(int count)
    {
        List<ItemData> result = new List<ItemData>();
        for(int i=0; i<count; i++)
        {
            ItemData item = GetRandomItem();
            if (item != null) result.Add(item);
        }
        return result;
    }

#if UNITY_EDITOR
    [ContextMenu("Load All Items From Folder")]
    public void LoadAllItems()
    {
        allItems = new List<ItemData>();
        string[] connectionGuids = UnityEditor.AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Item/Data" });
        
        foreach (string guid in connectionGuids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ItemData data = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (data != null) allItems.Add(data);
        }
        Debug.Log($"[ItemDatabase] {allItems.Count} Items Loaded.");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
