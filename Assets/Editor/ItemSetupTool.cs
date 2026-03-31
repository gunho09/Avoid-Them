using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ItemSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Boss Rewards")]
    public static void SetupBossRewards()
    {
        string dataPath = "Assets/Item/Data";
        if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);

        // 1. 신규 아이템 6종 생성 및 설정 (기본 수치 포함)
        Dictionary<string, (ItemEffectType type, float val)> newItems = new Dictionary<string, (ItemEffectType, float)>()
        {
            { "Doppelganger", (ItemEffectType.Doppelganger, 1.0f) },
            { "Bulldozer", (ItemEffectType.Bulldozer, 1.5f) },
            { "KnowHow", (ItemEffectType.KnowHow, 0.05f) },
            { "Poison", (ItemEffectType.Poison, 0.05f) },
            { "OutofCombatRegen", (ItemEffectType.OutofCombatRegen, 0.01f) },
            { "Awakening", (ItemEffectType.Awakening, 0.5f) }
        };

        List<ItemData> createdItems = new List<ItemData>();

        foreach (var item in newItems)
        {
            string assetPath = $"{dataPath}/{item.Key}.asset";
            ItemData data = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ItemData>();
                data.itemName = item.Key;
                data.effectType = item.Value.type;
                data.valuePerStack = item.Value.val;
                data.rarity = ItemRarity.Legend; // 보스 전용이므로 레전드 등급
                data.isBossOnly = true; // [New] 보스 전용으로 설정
                AssetDatabase.CreateAsset(data, assetPath);
                Debug.Log($"[Setup] Created Item: {item.Key} with Value: {item.Value.val}");
            }
            else
            {
                // 이미 존재한다면 수치 및 플래그 업데이트
                data.valuePerStack = item.Value.val;
                data.isBossOnly = true; 
                EditorUtility.SetDirty(data);
            }
            createdItems.Add(data);
        }

        // 1.5. 기존의 다른 모든 아이템들에 대해 isBossOnly가 켜져 있다면 해제 (정화 작업)
        string[] allItemGuids = AssetDatabase.FindAssets("t:ItemData");
        foreach (string guid in allItemGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData data = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (data != null && !newItems.ContainsKey(data.itemName))
            {
                if (data.isBossOnly)
                {
                    data.isBossOnly = false;
                    EditorUtility.SetDirty(data);
                    Debug.Log($"[Cleanup] Unchecked isBossOnly for: {data.itemName}");
                }
            }
        }

        AssetDatabase.SaveAssets();

        // 2. ItemDatabase에 등록
        ItemDatabase db = null;
        string[] dbGuids = AssetDatabase.FindAssets("t:ItemDatabase");
        if (dbGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(dbGuids[0]);
            db = AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);
        }

        if (db != null)
        {
            // [New] 현재 아이템 목록을 통째로 새로고침
            db.LoadAllItems(); 
            
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets(); // 데이터베이스 즉시 저장
            Debug.Log("[Setup] Registered items to ItemDatabase and Synchronized.");
        }

        // 3. 보스 프리팹들에 ItemDrop 프리팹 연결
        GameObject itemDropPrefab = null;
        string[] dropGuids = AssetDatabase.FindAssets("ItemDrop t:Prefab");
        if (dropGuids.Length > 0)
        {
            string dropPath = AssetDatabase.GUIDToAssetPath(dropGuids[0]);
            itemDropPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dropPath);
        }

        if (itemDropPrefab != null)
        {
            string[] bossNames = { "1Boss", "2Boss", "3Boss" };
            foreach (var name in bossNames)
            {
                string[] bossGuids = AssetDatabase.FindAssets($"{name} t:Prefab");
                if (bossGuids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(bossGuids[0]);
                    GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    
                    // 각 보스 스크립트 찾아서 할당 (필드 이름 주의!)
                    var b1 = bossPrefab.GetComponent<FirstBoss>();
                    if (b1 != null) b1.itemDropPrefab = itemDropPrefab;
                    
                    var b2 = bossPrefab.GetComponent<SecondBoss>();
                    if (b2 != null) b2.itemDropPrefab = itemDropPrefab;
                    
                    var b3 = bossPrefab.GetComponent<Boss3>();
                    if (b3 != null) b3.itemDropPrefab = itemDropPrefab;

                    EditorUtility.SetDirty(bossPrefab);
                    Debug.Log($"[Setup] Linked ItemDrop to {name}");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=green>[Setup Complete] 보스 전용 아이템 설정 및 드랍 연결이 완료되었습니다! 이제 드라이브가 나오지 않을 것입니다.</color>");
    }
}
