using UnityEngine;

public class Chest : MonoBehaviour
{
    private bool isOpened = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOpened) return;

        // 플레이어 태그 확인 (Player 태그가 설정되어 있어야 함)
        if (collision.CompareTag("Player"))
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpened = true;
        Debug.Log("Chest Opened!");

        // 아이템 선택 UI 호출
        if (ItemSelectUI.Instance != null)
        {
            ItemSelectUI.Instance.ShowSelectUI();
        }
        else
        {
            // 아직 UI가 씬에 없을 경우를 대비한 로그
            Debug.LogWarning("ItemSelectUI Instance is null! Make sure ItemSelectUI is in the scene.");
        }

        // 상자를 획득했으므로 비활성화 (추후 열림 애니메이션으로 대체 가능)
        gameObject.SetActive(false); 
    }
}
