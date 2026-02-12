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

    
    private float lastDoorUseTime = 0f;
    private const float doorCooldown = 1.5f; 

    [Header("Boss Room Settings")]
    public string warningMessage = "5개의 방을 다 돌아보고 오세요"; 

    private void Start()
    {
        Debug.Log($"Door Script Init: {gameObject.name}, Type: {type}");

        if (type == DoorType.ToBossRoom)
        {
            // 기존 Start 시점 체크 로직 제거
            // 이제 EnterIfPlayer에서 동적으로 체크합니다.
            isOpen = true; // 기본적으로 열어두고, 진입 시 조건을 검사합니다.
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
        if (Time.time - lastDoorUseTime < doorCooldown) return;

        // 1. 보스 방 진입 조건 체크 (동적)
        // 1. 보스 방 진입 조건 체크 (동적)
        if (type == DoorType.ToBossRoom)
        {
            if (MapManager.Instance != null && MapManager.Instance.clearedRooms < MapManager.Instance.totalRoomsPerFloor)
            {
                // 조건 미달
                int current = MapManager.Instance.clearedRooms;
                int targetCount = MapManager.Instance.totalRoomsPerFloor;
                
                string msg = $"보스방 진입 불가: 현재 {current}/{targetCount} 클리어 (MapManager.clearedRooms={current}, total={targetCount})";
                Debug.Log(msg);
                
                // 경고 UI 표시
                if (WarningUI.Instance != null)
                {
                    WarningUI.Instance.ShowWarning($"{targetCount - current}개의 방을 더 클리어하세요!");
                }
                
                lastDoorUseTime = Time.time; 
                return;
            }
        }

        if (!isOpen)
        {
            // Debug.Log($"문이 잠겨있습니다! (Mobs remaining?) Type: {type}");
            return;
        }

        // Debug.Log($"문 작동! 이동 시도 -> Type: {type}");
        lastDoorUseTime = Time.time;
        
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
