using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorType
    {
        ToRoom,     
        ToHallway,  
        ToBossRoom,  
        BossRoomToHallway 
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
        if (type == DoorType.ToBossRoom || type == DoorType.BossRoomToHallway) 
        {
            isOpen = true; 
        }       

        if (Mathf.Abs(transform.position.z) > 0.1f)
        {
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, pos.y, 0f);
        }

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.sortingOrder = 5; 

        UpdateVisuals();
    }


    private void TriggerDoor()
    {
        if (Time.time - lastDoorUseTime < doorCooldown) return;
        
        if (MapManager.Instance == null) return;

        if (!isOpen) return;

        if (type == DoorType.ToBossRoom)
        {
            if (MapManager.Instance.clearedRooms < MapManager.Instance.totalRoomsPerFloor)
            {
                if (WarningUI.Instance != null) WarningUI.Instance.ShowWarning(warningMessage);
                lastDoorUseTime = Time.time; 
                return;
            }
        }

        if (type == DoorType.BossRoomToHallway)
        {
            if (!MapManager.Instance.isBossDead)
            {
                if (WarningUI.Instance != null) WarningUI.Instance.ShowWarning("보스 처치 안됨");
                lastDoorUseTime = Time.time;
                return;
            }
        }


        lastDoorUseTime = Time.time;
        
        if (type == DoorType.ToRoom || type == DoorType.ToBossRoom)
        {
            Vector3 safeReturnPos = this.transform.position + returnOffset;
            MapManager.Instance.EnterRoom(safeReturnPos, type == DoorType.ToBossRoom);
        }
        else if (type == DoorType.ToHallway || type == DoorType.BossRoomToHallway)
        {
            MapManager.Instance.ReturnToHallway();
        }
    }

    public void SetStatus(bool _isOpen)
    {
        isOpen = _isOpen;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;
        
        if (type == DoorType.ToBossRoom || type == DoorType.BossRoomToHallway)
            spriteRenderer.color = Color.red;
        else
            spriteRenderer.color = Color.white;

        spriteRenderer.sprite = isOpen ? (openSprite ? openSprite : closedSprite) : closedSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.name == "MainChar")
        {
            TriggerDoor();
        }
    }
}