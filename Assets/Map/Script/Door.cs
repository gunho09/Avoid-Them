using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorType
    {
        ToRoom,     // 복도 -> 방
        ToHallway,  // 방 -> 복도
        ToNextFloor // 보스 대면 후 다음 층 (필요 시)
    }

    public DoorType type;

    [Header("Visual Settings")]
    public SpriteRenderer spriteRenderer;
    public Sprite openSprite;   // 열려있을 때 (투명 혹은 열린 이미지)
    public Sprite closedSprite; // 닫혀있을 때 (닫힌 이미지)

    public bool isOpen = true;

    [Header("Return Settings")]
    public Vector3 returnOffset = new Vector3(0, -1.5f, 0); // 복귀 시 문 앞 어디로 올지 (기본: 아래쪽)

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // [중요] 벽 위에 겹쳐도 잘 보이도록 순서를 앞으로 당깁니다.
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 5; 
        }

        UpdateVisuals();
    }

    // 외부에서 문 상태를 변경할 때 이 함수를 사용하세요
    public void SetStatus(bool _isOpen)
    {
        isOpen = _isOpen;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        if (isOpen)
        {
            if (openSprite != null) 
            {
                spriteRenderer.sprite = openSprite;
            }
            else 
            {
                // [수정] 열린 그림이 없으면 그냥 닫힌 그림을 유지합니다.
                // (투명해지면 뒤에 벽이 보여서 이상하니까요)
                if (closedSprite != null) spriteRenderer.sprite = closedSprite;
            }
        }
        else
        {
            if (closedSprite != null) spriteRenderer.sprite = closedSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isOpen)
        {
            if (type == DoorType.ToRoom)
            {
                // 문 위치 + 오프셋을 계산해서 전달 (바로 다시 들어가지 않게 함)
                Vector3 safeReturnPos = this.transform.position + returnOffset;
                MapManager.Instance.EnterRoom(safeReturnPos);
            }
            else if (type == DoorType.ToHallway)
            {
                // 방에서 나갈 때도 문(ToHallway)이 있다면 사용하겠지만, 
                // 보통은 MapManager가 알아서 복귀 위치로 보내주므로 여기선 호출만 함
                MapManager.Instance.ReturnToHallway();
            }
            else if (type == DoorType.ToNextFloor)
            {
                MapManager.Instance.NextFloor();
            }
        }
    }
}
