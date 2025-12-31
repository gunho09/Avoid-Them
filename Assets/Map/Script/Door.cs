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
    public bool isOpen = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isOpen)
        {
            if (type == DoorType.ToRoom)
            {
                // 현재 내(문) 위치를 함께 전달하여 나중에 여기로 돌아오게 함
                MapManager.Instance.EnterRoom(this.transform.position);
            }
            else if (type == DoorType.ToHallway)
            {
                MapManager.Instance.ReturnToHallway();
            }
            else if (type == DoorType.ToNextFloor)
            {
                MapManager.Instance.NextFloor();
            }
        }
    }
}
