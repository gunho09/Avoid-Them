using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemCard : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI rarityText;
    public Button selectButton;
    public GameObject lockOverlay; // 선택 불가 시 표시할 어두운 배경
    public TextMeshProUGUI lockReasonText;

    private ItemData _data;
    private ItemSelectUI _manager;

    public void Setup(ItemData data, bool canAcquire, ItemSelectUI manager)
    {
        _data = data;
        _manager = manager;
        gameObject.SetActive(true);

        // UI 갱신
        if (iconImage != null) iconImage.sprite = data.icon;
        if (nameText != null) nameText.text = data.itemName;
        if (descText != null) 
        {
            // 설명 + (현재 레벨/보유 수 표시 등을 원하면 추가)
            int currentStack = Inventory.Instance.GetStackCount(data.effectType);
            string bonusText = currentStack > 0 ? $"\n(현재 보유: {currentStack}개)" : "\n(신규 획득)";
            descText.text = data.description + bonusText;
        }
        
        // 등급 텍스트/색상
        if (rarityText != null) 
        {
            rarityText.text = data.rarity.ToString();
            switch(data.rarity)
            {
                case ItemRarity.Normal: rarityText.color = Color.white; break;
                case ItemRarity.Rare: rarityText.color = new Color(0.2f, 0.5f, 1f); break; // 파랑
                case ItemRarity.Legend: rarityText.color = new Color(1f, 0.8f, 0.1f); break; // 골드
            }
        }

        // 버튼 상태 설정
        selectButton.onClick.RemoveAllListeners();
        
        if (canAcquire)
        {
            selectButton.interactable = true;
            if (lockOverlay != null) lockOverlay.SetActive(false);
            selectButton.onClick.AddListener(() => _manager.OnSelectItem(_data));
        }
        else
        {
            selectButton.interactable = false;
            // 왜 못 먹는지 이유 표시
            if (lockOverlay != null)
            {
                lockOverlay.SetActive(true);
                if (lockReasonText != null)
                {
                    if (Inventory.Instance.TotalAcquiredCount >= Inventory.Instance.MaxAcquisitionCount)
                        lockReasonText.text = "최대 개수 도달\n(10/10)";
                    else
                        lockReasonText.text = "슬롯 부족";
                }
            }
        }
    }
}
