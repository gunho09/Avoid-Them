using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    public ItemData itemData;
    public int stackCount;

    public ItemSlot(ItemData data)
    {
        itemData = data;
        stackCount = 1;
    }
}

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public int MaxSlots = 15;          // 최대로 가질 수 있는 아이템 수 (5 Active + 10 Storage)
    public int MaxActiveSlots = 5;     // 실제 효과가 발동되는 장착 슬롯 수

    public List<ItemSlot> slots = new List<ItemSlot>();
    
    // 현재까지 획득한 아이템 총 개수 (중첩 포함) - 획득 제한용
    // (기획 변경으로 10개 제한이 '슬롯 10개'를 의미하는지, '먹은 횟수 10번'인지 확인 필요하나 
    //  유저 요청 "먹는 건 10개" -> 획득 횟수로 유지)
    public int MaxAcquisitionCount = 10;

    public int TotalAcquiredCount 
    {
        get 
        {
            int count = 0;
            foreach (var slot in slots) 
            {
                if (slot != null) count += slot.stackCount;
            }
            return count;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // [Refactor] MaxSlots만큼 null로 미리 채움
        while (slots.Count < MaxSlots)
        {
            slots.Add(null);
        }
    }

    public bool CanAcquire(ItemData newItem)
    {
        // 1. 총 획득 횟수 제한 (10회)
        if (TotalAcquiredCount >= MaxAcquisitionCount) return false;

        // 2. 이미 있는 아이템이면? (Stack) -> 가능
        if (HasItem(newItem)) return true;

        // 3. 새로운 아이템이면? -> 빈 칸이 하나라도 있으면 가능
        return slots.Exists(s => s == null);
    }

    // 아이템 변경 알림 이벤트
    public event System.Action OnInventoryChanged;

    public void AddItem(ItemData newItem)
    {
        if (!CanAcquire(newItem)) return;

        // 1. 기존에 있는 아이템인지 확인 (Null이 아닌 것 중에서)
        ItemSlot existingSlot = slots.Find(s => s != null && s.itemData == newItem);

        if (existingSlot != null)
        {
            existingSlot.stackCount++;
        }
        else
        {
            // 2. 없다면 빈 칸(null)을 찾아서 채움
            int emptyIndex = slots.FindIndex(s => s == null);
            if (emptyIndex != -1)
            {
                slots[emptyIndex] = new ItemSlot(newItem);
            }
        }
        
        // 변경 알림
        OnInventoryChanged?.Invoke();
    }

    public bool HasItem(ItemData item)
    {
        return slots.Exists(s => s != null && s.itemData == item);
    }

    // --- Active Slot Logic (앞에서부터 5개만 적용) ---

    // 특정 효과의 총 보너스 수치 (Value * Stack) - 활성화된 슬롯에서만 계산
    public float GetTotalStatBonus(ItemEffectType effectType)
    {
        float total = 0f;
        
        // 최대 5개까지만 순회 (고정 크기이므로 index 접근 안전)
        for (int i = 0; i < MaxActiveSlots; i++)
        {
            if (i < slots.Count && slots[i] != null) // Index Bound Check & Null Check
            {
                if (slots[i].itemData.effectType == effectType)
                {
                    total += slots[i].itemData.valuePerStack * slots[i].stackCount;
                }
            }
        }
        return total;
    }

    // 슬롯 순서 변경 (스왑)
    public void SwapItems(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= slots.Count || indexB < 0 || indexB >= slots.Count) return;

        ItemSlot temp = slots[indexA];
        slots[indexA] = slots[indexB];
        slots[indexB] = temp;
        
        // 순서 변경도 스탯 적용(ex: Active 슬롯 진입)에 영향 줄 수 있음
        OnInventoryChanged?.Invoke();
    }

    public int GetStackCount(ItemEffectType effectType)
    {
        // 전체 검색 (Null 제외)
        ItemSlot slot = slots.Find(s => s != null && s.itemData.effectType == effectType);
        return slot != null ? slot.stackCount : 0;
    }

    // 아이템 소모 (비상식량, 밴드 등)
    public void DecreaseItemCount(ItemEffectType effectType)
    {
        // FindIndex 사용해서 삭제 시 해당 슬롯을 null로 만들어야 함
        int index = slots.FindIndex(s => s != null && s.itemData.effectType == effectType);
        
        if (index != -1)
        {
            slots[index].stackCount--;
            Debug.Log($"[Inventory] Used {slots[index].itemData.itemName}. Remaining: {slots[index].stackCount}");
            
            if (slots[index].stackCount <= 0)
            {
                slots[index] = null; // 리스트에서 제거가 아니라 null로 비움
            }
            
            // 변경 알림
            OnInventoryChanged?.Invoke();
        }
    }

    // Active 상태인지 확인 (특정 아이템 데이터 기준)
    public bool IsActive(ItemData item)
    {
        int index = slots.FindIndex(s => s != null && s.itemData == item);
        return index != -1 && index < MaxActiveSlots;
    }
}
