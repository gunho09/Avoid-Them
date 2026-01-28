using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject uiPanel; // 인벤토리 전체 패널
    public Transform activeSlotsParent; // 상단 5개 슬롯 부모
    public Transform storageSlotsParent; // 하단 10개 슬롯 부모
    public ItemSlotUI slotPrefab; 

    private List<ItemSlotUI> activeSlots = new List<ItemSlotUI>();
    private List<ItemSlotUI> storageSlots = new List<ItemSlotUI>();

    private void Start()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged += UpdateUI;
        }

        // 초기 슬롯 생성
        CreateSlots();
        
        // 초기 UI 상태 (닫힘)
        if (uiPanel != null) uiPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged -= UpdateUI;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (uiPanel != null)
            {
                bool isActive = !uiPanel.activeSelf;
                uiPanel.SetActive(isActive);
                
                if (isActive) UpdateUI();
            }
        }
    }

    void CreateSlots()
    {
        // 1. 기존 슬롯 정리 (혹시 모를 중복 방지)
        foreach (Transform child in activeSlotsParent) Destroy(child.gameObject);
        foreach (Transform child in storageSlotsParent) Destroy(child.gameObject);
        activeSlots.Clear();
        storageSlots.Clear();

        // 2. 상단 5개 (Active) 생성
        for (int i = 0; i < 5; i++)
        {
            if (slotPrefab != null && activeSlotsParent != null)
            {
                ItemSlotUI slot = Instantiate(slotPrefab, activeSlotsParent);
                activeSlots.Add(slot);
                slot.ClearSlot(); // 빈 상태로 초기화
            }
        }

        // 3. 하단 10개 (Storage) 생성
        for (int i = 0; i < 10; i++)
        {
            if (slotPrefab != null && storageSlotsParent != null)
            {
                ItemSlotUI slot = Instantiate(slotPrefab, storageSlotsParent);
                storageSlots.Add(slot);
                slot.ClearSlot();
            }
        }
    }

    public void UpdateUI()
    {
        if (Inventory.Instance == null) return;
        
        Debug.Log($"[InventoryUI] UI 갱신 시작. 현재 아이템 수: {Inventory.Instance.slots.Count}");
        
        List<ItemSlot> allItems = Inventory.Instance.slots;

        // Active Slots (0~4) 표시
        for (int i = 0; i < activeSlots.Count; i++)
        {
            if (i < allItems.Count)
            {
                if (allItems[i] != null) // Null(빈칸)이 아니면 표시
                {
                    activeSlots[i].Setup(allItems[i], i);
                }
                else // Null(빈칸)이면 클리어 + 인덱스 설정
                {
                    activeSlots[i].ClearSlot();
                    activeSlots[i].slotIndex = i;
                }
            }
            else // 인벤토리보다 슬롯 UI가 많을 때 (기본적으로 위 else와 동일)
            {
                activeSlots[i].ClearSlot();
                activeSlots[i].slotIndex = i;
            }
        }

        // Storage Slots (5~14) 표시
        for (int i = 0; i < storageSlots.Count; i++)
        {
            int dataIndex = i + 5; 
            
            if (dataIndex < allItems.Count)
            {
                if (allItems[dataIndex] != null)
                {
                    storageSlots[i].Setup(allItems[dataIndex], dataIndex);
                }
                else
                {
                    storageSlots[i].ClearSlot();
                    storageSlots[i].slotIndex = dataIndex;
                }
            }
            else
            {
                storageSlots[i].ClearSlot();
                storageSlots[i].slotIndex = dataIndex;
            }
        }
    }
}
