using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorType
    {
        ToRoom,     // 복도 -> 방
        ToHallway,  // 방 -> 복도
        ToBossRoom  // 복도 -> 보스방
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
        // [자동 수정] 문 위치가 Z축과 어긋나 있으면 강제로 0으로 맞춤 (충돌 문제 해결)
        if (Mathf.Abs(transform.position.z) > 0.1f)
        {
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, pos.y, 0f);
            Debug.LogWarning($"{gameObject.name}의 Z축이 0이 아니어서 강제로 수정했습니다.");
        }

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

        // 보스방 문이면 빨간색으로 표시
        if (type == DoorType.ToBossRoom)
        {
            spriteRenderer.color = Color.red;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }

        if (isOpen)
        {
            if (openSprite != null) 
            {
                spriteRenderer.sprite = openSprite;
            }
            else 
            {
               
                if (closedSprite != null) spriteRenderer.sprite = closedSprite;
            }
        }
        else
        {
            if (closedSprite != null) spriteRenderer.sprite = closedSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnterIfPlayer(collision.gameObject);
    }

    // [추가] 혹시 Is Trigger 체크 안 했을 경우를 대비해 충돌(Collision)로도 작동하게 함
    private void OnCollisionEnter2D(Collision2D collision)
    {
        EnterIfPlayer(collision.gameObject);
    }

    private void EnterIfPlayer(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            // Debug.Log("Player detected! Entering...");
            if (type == DoorType.ToRoom || type == DoorType.ToBossRoom)
            {
                Vector3 safeReturnPos = this.transform.position + returnOffset;
                MapManager.Instance.EnterRoom(safeReturnPos, type == DoorType.ToBossRoom);
            }
            else if (type == DoorType.ToHallway)
            {
                MapManager.Instance.ReturnToHallway();
            }
        }
    }
}
