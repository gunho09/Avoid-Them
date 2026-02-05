using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private ItemData _data;
    private RoomControl _roomControl;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    // 방 컨트롤러가 이 함수를 호출해서 "너는 '흡혈' 아이템이야!"라고 정해줍니다.
    public void Setup(ItemData data, RoomControl roomControl)
    {
        _data = data;
        _roomControl = roomControl;

        // 여기서 프리팹의 그림을 해당 아이템의 그림으로 교체합니다!
        if (_sr != null && data.icon != null)
        {
            _sr.sprite = data.icon;
        }
        
        // 아이템 크기 조정 (필요 시)
        transform.localScale = Vector3.one * 1.5f; 
    }

    private bool canPickup = false;

    private void Start()
    {
        StartCoroutine(EnablePickupRoutine());
    }

    System.Collections.IEnumerator EnablePickupRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        canPickup = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canPickup) return;

        // "Player" 태그가 맞는지 꼭 확인해야 함
        if (collision.CompareTag("Player"))
        {
            PickUp();
        }
    }

    // 마우스 올렸을 때 설명 표시
    private void OnMouseEnter()
    {
        if (_data != null && ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.ShowTooltip(_data.itemName, _data.description, _data.rarity);
        }
    }

    // 마우스 뗐을 때 설명 숨김
    private void OnMouseExit()
    {
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltip();
        }
    }

    void PickUp()
    {
        Debug.Log($"[ItemPickup] {_data.itemName} 획득 시도!");

        // 1. 인벤토리에 추가
        if (Inventory.Instance != null)
        {
            Inventory.Instance.AddItem(_data);
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-7"); // 아이템 획득
            Debug.Log($"[ItemPickup] 인벤토리에 추가 요청 보냄");
        }
        else
        {
             Debug.LogError("[ItemPickup] Inventory.Instance가 null입니다!");
        }

        // 2. 방 컨트롤러에게 "나 먹혔어!"라고 알림 (나머지 2개 삭제 + 문 열기 위해)
        if (_roomControl != null)
        {
            _roomControl.OnItemPicked();
        }

        // 3. 나 자신 삭제
        Destroy(gameObject);
    }
}
