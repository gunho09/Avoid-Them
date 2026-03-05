using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private ItemData _data;
    private RoomControl _roomControl;
    private SpriteRenderer _sr;

    private Collider2D _collider;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    [Tooltip("아이템 생성 시 크기 조절 (기본: 0.35)")]
    public float pickupScale = 0.35f;

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
        
        // 아이템 크기 조정 (설정된 값 사용)
        transform.localScale = Vector3.one * pickupScale; 
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

    private bool isHovering = false;

    private void OnMouseEnter()
    {
        if (ItemTooltip.Instance != null && _data != null)
        {
            isHovering = true;
            ItemTooltip.Instance.ShowTooltip(_data.itemName, _data.description, _data.rarity);
        }
    }

    private void OnMouseExit()
    {
        if (ItemTooltip.Instance != null)
        {
            isHovering = false;
            ItemTooltip.Instance.HideTooltip();
        }
    }

    void PickUp()
    {
        Debug.Log($"[ItemPickup] {_data.itemName} 획득 시도!");

        // 1. 인벤토리에 추가 시도
        if (Inventory.Instance != null)
        {
            if (Inventory.Instance.AddItem(_data))
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-7"); // 아이템 획득
                Debug.Log($"[ItemPickup] 인벤토리에 추가 완료");

                // 2. 방 컨트롤러에게 "나 먹혔어!"라고 알림
                if (_roomControl != null)
                {
                    _roomControl.OnItemPicked();
                }

                // 3. 나 자신 삭제
                Destroy(gameObject);
            }
            else
            {
                // 인벤토리 가득 참 (CanAcquire failure)
                Debug.Log("[ItemPickup] 인벤토리가 가득 차서 획득 불가!");
                if (WarningUI.Instance != null)
                {
                    WarningUI.Instance.ShowWarning("인벤토리가 가득 찼습니다! (최대 10개)");
                }
            }
        }
        else
        {
             Debug.LogError("[ItemPickup] Inventory.Instance가 null입니다!");
        }
    }

    private void OnDisable()
    {
        if (isHovering && ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltip();
        }
    }
}
