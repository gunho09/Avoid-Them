using UnityEngine;

public class RoomControl : MonoBehaviour
{
    [Header("Room Settings")]
    public Transform playerSpawnPoint;   // 플레이어가 방에 입장할 때 생성될 위치 (없으면 문 앞)
    public Transform cameraPoint;        // [NEW] 이 방에 들어왔을 때 카메라가 비출 중심 위치 (없으면 방 생성 위치 기준)
    
    // public Transform[] enemySpawnPoints; // 더 이상 사용 안 함 (수동 배치)
    // public GameObject[] enemyPrefabs;    // 더 이상 사용 안 함
    
    public GameObject rewardChest;  
    public GameObject returnDoor;   
    
    private int livingEnemyCount = 0; 
    private bool isCleared = false;

    void Start()
    {
        if (returnDoor != null)
        {
            returnDoor.SetActive(true); 
            Door doorScript = returnDoor.GetComponent<Door>();
            if (doorScript != null)
            {
                doorScript.SetStatus(false);
            }
        }

        // 배치된 적 세기
        // (zombie 스크립트가 붙은 모든 자식 오브젝트를 찾습니다)
        var enemies = GetComponentsInChildren<zombie>();
        livingEnemyCount = enemies.Length;

        Debug.Log($"방 진입: 배치된 적 {livingEnemyCount}마리 감지됨");

        if (livingEnemyCount == 0)
        {
            RoomClear();
        }
    }

    // void SpawnEnemies() 제거됨

    
    public void OnEnemyKilled()
    {
        if (isCleared) return;

        livingEnemyCount--;

        if (livingEnemyCount <= 0)
        {
            RoomClear();
        }
    }

    void RoomClear()
    {
        isCleared = true;
        Debug.Log("방 클리어! 보상 생성 & 문 열림");

        if (rewardChest != null)
            rewardChest.SetActive(true);

        if (returnDoor != null)
        {
            
            Door doorScript = returnDoor.GetComponent<Door>();
            if (doorScript != null)
            {
                doorScript.SetStatus(true);
            }
        }

        
        if (MapManager.Instance != null)
            MapManager.Instance.OnRoomCleared();
    }

    public void OnRoomEntered()
    {
        Debug.Log("Player entered the room");
        // 필요한 경우 여기에 진입 시 로직 추가
    }
}
