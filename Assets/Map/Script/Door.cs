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
    public Sprite openSprite;  
    public Sprite closedSprite; 

    public bool isOpen = true;

    [Header("Return Settings")]
    public Vector3 returnOffset = new Vector3(0, -1.5f, 0); 

    
    private static float lastDoorUseTime = 0f;
    private const float doorCooldown = 1.5f; 

    private void Start()
    {
        Debug.Log($"Door Script Init: {gameObject.name}, Type: {type}");

        if (type == DoorType.ToBossRoom && MapManager.Instance != null)
        {
            if (MapManager.Instance.clearedRooms < MapManager.Instance.totalRoomsPerFloor)
            {
                isOpen = false;
                Debug.Log("보스방 진입 불가: 아직 방을 다 클리어하지 않았습니다.");
            }
            else
            {
                isOpen = true;
            }
        }

        
        if (Mathf.Abs(transform.position.z) > 0.1f)
        {
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, pos.y, 0f);
            Debug.LogWarning($"{gameObject.name}의 Z축이 0이 아니어서 강제로 수정했습니다.");
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 5; 
        }

        UpdateVisuals();
    }

  
    private void Update()
    {
        if (!isOpen) return;
        
        // [최적화] 매 프레임 Find 하는 대신 MapManager의 플레이어 참조 사용
        GameObject player = null;
        if (MapManager.Instance != null && MapManager.Instance.player != null)
        {
            player = MapManager.Instance.player;
        }
        else
        {
            // 비상용 (MapManager가 없을 때만 Find)
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("MainChar");
        }
        
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < 1.5f) 
            {
                // [로그 최적화] 너무 자주 뜨지 않게 주석 처리하거나 빈도 줄이는 게 좋음
                // Debug.Log($"[Door] 거리 체크로 플레이어 감지! 거리: {distance}");
                TriggerDoor();
            }
        }
    }

    private void TriggerDoor()
    {
        
        if (Time.time - lastDoorUseTime < doorCooldown)
        {
            Debug.Log($"[Door] 쿨다운 중! 남은 시간: {doorCooldown - (Time.time - lastDoorUseTime):F2}초");
            return; 
        }
        
        Debug.Log($"[Door] TriggerDoor 호출됨! Type: {type}");
        
        if (MapManager.Instance == null)
        {
            Debug.LogError("[Door] MapManager.Instance가 null입니다! 씬에 MapManager가 있는지 확인하세요.");
            return;
        }
        
       
        lastDoorUseTime = Time.time;
        
        if (type == DoorType.ToRoom || type == DoorType.ToBossRoom)
        {
            Vector3 safeReturnPos = this.transform.position + returnOffset;
            Debug.Log($"[Door] EnterRoom 호출 시도...");
            MapManager.Instance.EnterRoom(safeReturnPos, type == DoorType.ToBossRoom);
        }
        else if (type == DoorType.ToHallway)
        {
            Debug.Log("[Door] ReturnToHallway 호출 시도...");
            MapManager.Instance.ReturnToHallway();
        }
    }

    //외부사용
    public void SetStatus(bool _isOpen)
    {
        isOpen = _isOpen;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        //보스 그 빨강표시함
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

  
    private void OnCollisionEnter2D(Collision2D collision)
    {
        EnterIfPlayer(collision.gameObject);
    }

    // Stay 체크
    private void OnTriggerStay2D(Collider2D collision)
    {
        EnterIfPlayer(collision.gameObject);
    }

   
    private void OnCollisionStay2D(Collision2D collision)
    {
        EnterIfPlayer(collision.gameObject);
    }

    private void EnterIfPlayer(GameObject target)
    {
       
        Debug.Log($"Door Hit Check: {target.name}");

        
        Debug.Log($"[Door] Object detected. Open: {isOpen}, Type: {type}");

        if (!isOpen)
        {
            Debug.Log($"문이 잠겨있습니다! (Mobs remaining?) Type: {type}");
            return;
        }

        Debug.Log($"문 작동! 이동 시도 -> Type: {type}");
        
        if (type == DoorType.ToRoom || type == DoorType.ToBossRoom)
        {
            Vector3 safeReturnPos = this.transform.position + returnOffset;
            MapManager.Instance.EnterRoom(safeReturnPos, type == DoorType.ToBossRoom);
        }
        else if (type == DoorType.ToHallway)
        {
            Debug.Log("복도로 돌아갑니다.");
            MapManager.Instance.ReturnToHallway();
        }
    }
}
