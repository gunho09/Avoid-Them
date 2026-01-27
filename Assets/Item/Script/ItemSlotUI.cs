using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image iconImage;
    public TextMeshProUGUI stackText;

    private ItemData _itemData;

    public void Setup(ItemSlot slot)
    {
        _itemData = slot.itemData;

        if (_itemData != null)
        {
            // 아이콘 설정
            if (iconImage != null)
            {
                iconImage.sprite = _itemData.icon;
                iconImage.enabled = true;
            }

            // 개수 설정 (1개면 숨김, 2개 이상부터 표시)
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

    // 마우스 올렸을 때 툴팁 표시
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_itemData != null && ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.ShowTooltip(_itemData.itemName, _itemData.description, _itemData.rarity);
        }
    }

    // 마우스 뗐을 때 툴팁 숨김
    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltip();
        }
    }
}
