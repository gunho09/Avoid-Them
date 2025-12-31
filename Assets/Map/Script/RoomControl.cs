using UnityEngine;

public class RoomControl : MonoBehaviour
{
    [Header("Room Settings")]
    public Transform[] enemySpawnPoints; // 적들이 생성될 위치
    public GameObject[] enemyPrefabs;    // 생성할 적 프리팹 목록 (원거리, 근거리, 탱커 등)
    
    public GameObject rewardChest;  // 클리어 보상 상자
    public GameObject returnDoor;   // 돌아가는 문
    
    private int livingEnemyCount = 0; // 살아있는 적 수
    private bool isCleared = false;

    void Start()
    {
        // 1. 문 비활성화 (보이게 두되, 기능만 끔)
        if (returnDoor != null)
        {
            // GameObject는 켜두고 Door 스크립트의 isOpen만 false로 설정
            returnDoor.SetActive(true); 
            Door doorScript = returnDoor.GetComponent<Door>();
            if (doorScript != null)
            {
                doorScript.isOpen = false;
            }
        }

        // 2. 적 생성 (랜덤)
        SpawnEnemies();
    }

    // ... (SpawnEnemies and OnEnemyKilled remain same)

    void RoomClear()
    {
        isCleared = true;
        Debug.Log("방 클리어! 보상 생성 & 문 열림");

        if (rewardChest != null)
            rewardChest.SetActive(true);

        if (returnDoor != null)
        {
            // 문 기능 활성화
            Door doorScript = returnDoor.GetComponent<Door>();
            if (doorScript != null)
            {
                doorScript.isOpen = true;
            }
        }

        // 매니저에 알림
        if (MapManager.Instance != null)
            MapManager.Instance.OnRoomCleared();
    }

    public void OnRoomEntered()
    {
        Debug.Log("Player entered the room");
        // 필요한 경우 여기에 진입 시 로직 추가 (예: 문 닫기, 적 생성 시작 등)
    }
}
