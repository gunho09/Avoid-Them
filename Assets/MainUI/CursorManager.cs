using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture;

    void Start()
    {
        ApplyCursor();
    }

    void Update()
    {
        // 다른 코드가 숨겨버려도 계속 복구
        if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
        {
            ApplyCursor();
        }
    }

    void ApplyCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None; // 중요!!
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }
}