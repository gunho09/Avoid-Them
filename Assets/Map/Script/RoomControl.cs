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
        // 1. 문 닫기
        if (returnDoor != null)
            returnDoor.SetActive(false);

        // 2. 적 생성 (랜덤)
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        // 스폰 포인트나 프리팹 목록이 비어있으면 바로 클리어 처리
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || enemySpawnPoints == null)
        {
            if (livingEnemyCount == 0) RoomClear();
            return;
        }

        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            if (spawnPoint != null)
            {
                // 랜덤으로 적 종류 선택 (0 ~ 배열크기-1)
                int randomIndex = Random.Range(0, enemyPrefabs.Length);
                GameObject selectedPrefab = enemyPrefabs[randomIndex];

                if (selectedPrefab != null)
                {
                    // 적 생성
                    GameObject enemy = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
                    
                    // 생성한 적을 방의 자식으로 설정
                    enemy.transform.SetParent(this.transform); 
                    
                    // 몹 수 증가
                    livingEnemyCount++;
                }
            }
        }

        // 만약 생성된 적이 하나도 없다면 바로 클리어
        if (livingEnemyCount == 0)
        {
            RoomClear();
        }
    }

    // 몹이 죽을 때 호출되는 함수
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
            returnDoor.SetActive(true);

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
