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
        
        // [New] 게임 시작 시 혹은 씬 로드 시 아이템 목록이 비어있다면 자동으로 채움
#if UNITY_EDITOR
        if (allItems == null || allItems.Count == 0)
        {
            LoadAllItems();
        }
#endif
    }

    public ItemData GetRandomItem(List<ItemData> excludeList = null)
    {
        // 1. 등급 결정 (60 / 30 / 10)
        float rand = Random.Range(0f, 100f);
        ItemRarity targetRarity = ItemRarity.Normal;
        
        if (rand < 10f) targetRarity = ItemRarity.Legend;
        else if (rand < 40f) targetRarity = ItemRarity.Rare;
        else targetRarity = ItemRarity.Normal;

        // 2. 해당 등급 아이템 풀 생성 (인벤토리에 있는 아이템 제외 + 보스 전용 아이템 제외)
        List<ItemData> pool = allItems.FindAll(x => x.rarity == targetRarity && !x.isBossOnly);
        
        // [New] 인벤토리에 이미 있는 아이템 제외
        if (Inventory.Instance != null)
        {
            pool = pool.FindAll(x => !Inventory.Instance.HasItem(x));
        }

        // [New] excludeList에 있는 아이템도 제외 (같은 보상 내 중복 방지)
        if (excludeList != null)
        {
            pool = pool.FindAll(x => !excludeList.Contains(x));
        }

        // (만약 해당 등급 아이템이 하나도 없으면 전체에서 랜덤 - 보스 전용 제외)
        if (pool.Count == 0) 
        {
            pool = allItems.FindAll(x => !x.isBossOnly);
            if (Inventory.Instance != null)
                pool = pool.FindAll(x => !Inventory.Instance.HasItem(x));
            if (excludeList != null)
                pool = pool.FindAll(x => !excludeList.Contains(x));
        }

        // 3. 랜덤 선택
        if (pool.Count > 0)
        {
            return pool[Random.Range(0, pool.Count)];
        }
        
        return null; // 모든 아이템을 이미 보유 중
    }

    // 3개의 랜덤 아이템 뽑기 (중복 없음 + 인벤토리 보유 아이템 제외)
    public List<ItemData> GetRandomItems(int count)
    {
        List<ItemData> result = new List<ItemData>();
        
        // 무한 루프 방지를 위한 최대 시도 횟수
        int maxAttempts = count * 10;
        int attempts = 0;

        while (result.Count < count && attempts < maxAttempts)
        {
            attempts++;
            ItemData item = GetRandomItem(result);
            
            if (item != null && !result.Contains(item))
            {
                result.Add(item);
            }
        }
        
        return result;
    }

    public ItemData GetRandomBossItem()
    {
        // 1. 일차적으로 isBossOnly가 체크된 아이템들을 찾습니다.
        List<ItemData> pool = allItems.FindAll(x => x != null && x.isBossOnly);
        
        // [New] 만약 목록이 아예 비어있다면 에디터에서 강제 로드 시도
        if (pool.Count == 0)
        {
#if UNITY_EDITOR
            LoadAllItems();
#endif
            pool = allItems.FindAll(x => x != null && x.isBossOnly);
        }

        // [New] 그래도 없으면 6종 타입 강제 검색
        if (pool.Count == 0)
        {
            List<ItemEffectType> bossTypes = new List<ItemEffectType>()
            {
                ItemEffectType.Doppelganger, ItemEffectType.Bulldozer, ItemEffectType.KnowHow,
                ItemEffectType.Poison, ItemEffectType.OutofCombatRegen, ItemEffectType.Awakening
            };
            pool = allItems.FindAll(x => x != null && bossTypes.Contains(x.effectType));
        }

        // 인벤토리에 이미 있는 아이템 제외
        if (Inventory.Instance != null && pool.Count > 0)
        {
            pool = pool.FindAll(x => !Inventory.Instance.HasItem(x));
        }

        if (pool.Count > 0)
        {
            return pool[Random.Range(0, pool.Count)];
        }

        // [Critical Fix] 보스 아이템이 하나도 없으면 일반 아이템을 주지 않고 에러를 기록합니다.
        Debug.LogError("<color=red>[ItemDatabase] 보스 보상 아이템을 찾을 수 없습니다! Assets/Item/Data 폴더를 확인하세요.</color>");
        return null; // 일반 아이템(드라이브, 학살자 등)이 나오는 것보다 아예 안 나오는 게 버그 수정에 좋습니다.
    }

    public ItemData GetItemDataByEffect(ItemEffectType effectType)
    {
        return allItems.Find(x => x.effectType == effectType);
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
