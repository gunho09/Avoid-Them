using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door State")]
    public bool isOpen = true;

    BoxCollider2D col;
    SpriteRenderer sr;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        UpdateDoor();
    }

    public void Open()
    {
        isOpen = true;
        UpdateDoor();
    }

    public void Close()
    {
        isOpen = false;
        UpdateDoor();
    }

    void UpdateDoor()
    {
        
        col.enabled = !isOpen;

        // 시각적으로도 구분 (임시)
        if (sr != null)
            sr.color = isOpen ? Color.blue : Color.gray;
    }
}
