using UnityEngine;

public class EnterTrigger : MonoBehaviour
{
    private RoomController room;

    void Awake()
    {
        room = GetComponentInParent<RoomController>();

        if (room == null)
            Debug.LogError("RoomController ¸ø Ã£À½");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        room.OnRoomEntered();
    }
}
