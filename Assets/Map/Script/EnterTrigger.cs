using UnityEngine;

public class EnterTrigger : MonoBehaviour
{
    RoomController room;
    bool triggered = false;

    void Awake()
    {
        room = GetComponentInParent<RoomController>();

        if (room == null)
        {
            Debug.LogError("EnterTrigger: RoomController를 찾을 수 없음");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        if (room == null) return;

        if (room.areaType != AreaType.Room)
            return;

        triggered = true;   

        room.OnRoomEntered();

        // 다시 들어와도 안 울리게
        gameObject.SetActive(false);

       
    }
}
