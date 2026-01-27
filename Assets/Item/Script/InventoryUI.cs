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
                activeSlots[i].Setup(allItems[i]);
            }
            else
            {
                activeSlots[i].ClearSlot();
            }
        }

        // Storage Slots (5~14) 표시
        for (int i = 0; i < storageSlots.Count; i++)
        {
            int dataIndex = i + 5; // Inventory 리스트 상의 인덱스 (5부터 시작)
            
            if (dataIndex < allItems.Count)
            {
                storageSlots[i].Setup(allItems[dataIndex]);
            }
            else
            {
                storageSlots[i].ClearSlot();
            }
        }
    }
}
