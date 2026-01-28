using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI Components")]
    public Image iconImage;
    public TextMeshProUGUI stackText;

    private ItemData _itemData;
    public int slotIndex; // 자신의 인벤토리 인덱스

    private Transform originalParent;
    private GameObject ghostIcon; // 드래그 시 따라다닐 아이콘

    public void Setup(ItemSlot slot, int index)
    {
        _itemData = slot.itemData;
        slotIndex = index;

        if (_itemData != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = _itemData.icon;
                iconImage.enabled = true;
            }

            if (stackText != null)
            {
                if (slot.stackCount > 1)
                {
                    stackText.text = slot.stackCount.ToString();
                    stackText.enabled = true;
                }
                else
                {
                    stackText.enabled = false;
                }
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        _itemData = null;
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
        if (stackText != null)
        {
            stackText.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_itemData != null && ItemTooltip.Instance != null && ghostIcon == null) // 드래그 중엔 툴팁 안 뜸
        {
            ItemTooltip.Instance.ShowTooltip(_itemData.itemName, _itemData.description, _itemData.rarity);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltip();
        }
    }

    // --- Drag & Drop Implementation ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_itemData == null) return; // 빈 슬롯은 드래그 불가

        // 툴팁 끄기
        if (ItemTooltip.Instance != null) ItemTooltip.Instance.HideTooltip();

        // 1. Ghost Icon 생성 (Canvas 최상단에 그려지도록)
        ghostIcon = new GameObject("GhostIcon");
        ghostIcon.transform.SetParent(this.transform.root); // Canvas(Root) 밑으로 이동
        ghostIcon.transform.SetAsLastSibling(); // 맨 위에 그리기
        
        Image ghostImg = ghostIcon.AddComponent<Image>();
        ghostImg.sprite = iconImage.sprite;
        ghostImg.raycastTarget = false; // 마우스 이벤트 통과 (중요)
        ghostImg.color = new Color(1, 1, 1, 0.6f); // 반투명

        RectTransform rect = ghostIcon.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 80); // 적당한 크기 (슬롯 크기보다 약간 작게)
        ghostIcon.transform.position = eventData.position;

        // 2. 원본 아이콘 흐리게 처리
        if (iconImage != null) iconImage.color = new Color(1, 1, 1, 0.3f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            ghostIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 1. Ghost 삭제
        if (ghostIcon != null)
        {
            Destroy(ghostIcon);
        }

        // 2. 원본 복구
        if (iconImage != null) iconImage.color = Color.white;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 드래그 된 물체가 ItemSlotUI인지 확인
        ItemSlotUI draggedSlot = eventData.pointerDrag?.GetComponent<ItemSlotUI>();
        
        if (draggedSlot != null && draggedSlot != this)
        {
            Debug.Log($"Swap {draggedSlot.slotIndex} -> {this.slotIndex}");
            
            // 데이터 스왑 요청
            if (Inventory.Instance != null)
            {
                Inventory.Instance.SwapItems(draggedSlot.slotIndex, this.slotIndex);
            }
        }
    }
}
