using System.Collections.Generic;
using UnityEngine;

public class ItemSelectUI : MonoBehaviour
{
    public static ItemSelectUI Instance;

    public GameObject panel; // UI 전체 패널 (활성/비활성 용)
    public ItemCard[] cards; // 3개의 카드 스크립트 연결

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if(panel != null) panel.SetActive(false);
    }

    public void ShowSelectUI()
    {
        if (panel == null) return;
        
        panel.SetActive(true);
        Time.timeScale = 0f; // 게임 일시 정지

        // 1. 아이템 3개 뽑기
        List<ItemData> options = ItemDatabase.Instance.GetRandomItems(3);
        
        // 2. 각 카드에 데이터 바인딩
        for (int i = 0; i < cards.Length; i++)
        {
            if (i < options.Count && options[i] != null)
            {
                ItemData item = options[i];
                // 인벤토리 획득 가능 여부 확인
                bool canAcquire = Inventory.Instance.CanAcquire(item);
                
                cards[i].Setup(item, canAcquire, this);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnSelectItem(ItemData item)
    {
        // 인벤토리에 추가
        Inventory.Instance.AddItem(item);

        // UI 닫기
        CloseUI();
    }

    void CloseUI()
    {
        if(panel != null) panel.SetActive(false);
        Time.timeScale = 1f; // 게임 재개
    }
}
