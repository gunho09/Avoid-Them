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
            foreach (var slot in slots) count += slot.stackCount;
            return count;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool CanAcquire(ItemData newItem)
    {
        // 1. 총 획득 횟수 제한 (10회)
        if (TotalAcquiredCount >= MaxAcquisitionCount) return false;

        // 2. 이미 있는 아이템이면? (Stack) -> 가능
        if (HasItem(newItem)) return true;

        // 3. 새로운 아이템이면? -> 10칸 꽉 찼으면 불가
        return slots.Count < MaxSlots;
    }

    // 아이템 변경 알림 이벤트
    public event System.Action OnInventoryChanged;

    public void AddItem(ItemData newItem)
    {
        if (!CanAcquire(newItem)) return;

        ItemSlot existingSlot = slots.Find(s => s.itemData == newItem);
        if (existingSlot != null)
        {
            existingSlot.stackCount++;
        }
        else
        {
            slots.Add(new ItemSlot(newItem));
        }
        
        // 변경 알림
        OnInventoryChanged?.Invoke();
    }

    public bool HasItem(ItemData item)
    {
        return slots.Exists(s => s.itemData == item);
    }

    // --- Active Slot Logic (앞에서부터 5개만 적용) ---

    // 특정 효과의 총 보너스 수치 (Value * Stack) - 활성화된 슬롯에서만 계산
    public float GetTotalStatBonus(ItemEffectType effectType)
    {
        float total = 0f;
        
        // 최대 5개까지만 순회
        int count = Mathf.Min(slots.Count, MaxActiveSlots);
        
        for (int i = 0; i < count; i++)
        {
            if (slots[i].itemData.effectType == effectType)
            {
                total += slots[i].itemData.valuePerStack * slots[i].stackCount;
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
        // 활성화된 슬롯에서만 체크할지, 보유량 전체를 보여줄지 결정해야 함.
        // UI 표시용이라면 전체를 찾는게 맞고, 로직용이라면 Active만.
        // 현재는 UI 표시용(ItemCard)으로 주로 쓰이므로 전체 검색 유지하되,
        // 필요 시 Active Check 로직 추가.
        ItemSlot slot = slots.Find(s => s.itemData.effectType == effectType);
        return slot != null ? slot.stackCount : 0;
    }

    // 아이템 소모 (비상식량, 밴드 등)
    public void DecreaseItemCount(ItemEffectType effectType)
    {
        ItemSlot slot = slots.Find(s => s.itemData.effectType == effectType);
        if (slot != null)
        {
            slot.stackCount--;
            Debug.Log($"[Inventory] Used {slot.itemData.itemName}. Remaining: {slot.stackCount}");
            
            if (slot.stackCount <= 0)
            {
                slots.Remove(slot);
            }
            
            // 변경 알림
            OnInventoryChanged?.Invoke();
        }
    }

    // Active 상태인지 확인 (특정 아이템 데이터 기준)
    public bool IsActive(ItemData item)
    {
        int index = slots.FindIndex(s => s.itemData == item);
        return index != -1 && index < MaxActiveSlots;
    }
}
