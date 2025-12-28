using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Game Progression")]
    public int currentFloor = 1;         // 현재 층 (1 ~ 4)
    public int maxFloors = 4;            // 최대 층
    public int clearedRooms = 0;         // 현재 층에서 깬 방 개수
    public int totalRoomsPerFloor = 6;   // 층당 방 개수 (보스방 제외)

    [Header("Single Scene Settings")]
    public GameObject player;              // 플레이어 오브젝트 (Inspector에서 할당)
    public Transform roomSpawnPoint;       // 방이 생성될 위치 (복도와 멀리 떨어진 좌표)
    public GameObject[] roomPrefabs;       // 방 프리팹 목록 (랜덤 선택)
    public GameObject bossRoomPrefab;      // 보스방 프리팹

    private GameObject currentRoomInstance; // 현재 생성된 방 인스턴스
    private Vector3 lastDoorPosition;       // 들어왔던 문 위치 기억

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 복도 -> 방 (또는 보스방) 이동
    // doorPos: 플레이어가 들어간 문의 위치 (나올 때 돌아오기 위함)
    public void EnterRoom(Vector3 doorPos)
    {
        // 들어간 문 위치 저장
        lastDoorPosition = doorPos;

        if (clearedRooms < totalRoomsPerFloor)
        {
            Debug.Log($"일반 방 입장 ({clearedRooms + 1}번째 방)");
            SpawnRoom(false);
        }
        else
        {
            Debug.Log("보스 방 입장!");
            SpawnRoom(true);
        }
    }

    // 방 생성 & 플레이어 이동
    private void SpawnRoom(bool isBoss)
    {
        // 1. 방 프리팹 생성
        GameObject prefabToSpawn = isBoss ? bossRoomPrefab : roomPrefabs[Random.Range(0, roomPrefabs.Length)];
        
        if (prefabToSpawn == null)
        {
            Debug.LogError("방 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 기존 방이 있다면 제거
        if (currentRoomInstance != null)
            Destroy(currentRoomInstance);

        currentRoomInstance = Instantiate(prefabToSpawn, roomSpawnPoint.position, Quaternion.identity);

        // 2. 플레이어 순간이동 (방 생성 위치로)
        player.transform.position = roomSpawnPoint.position; 
    }

    // 방 클리어 시 호출
    public void OnRoomCleared()
    {
        clearedRooms++;
        Debug.Log($"방 클리어! 현재 층 완료한 방: {clearedRooms}/{totalRoomsPerFloor}");
    }

    // 방/보스방 -> 복도 이동
    public void ReturnToHallway()
    {
        // 1. 방 제거
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
        }

        // 2. 플레이어를 아까 들어왔던 문 위치로 복귀
        player.transform.position = lastDoorPosition; 
    }

    // 보스 처치 후 다음 층 이동
    public void NextFloor()
    {
        if (currentFloor < maxFloors)
        {
            currentFloor++;
            clearedRooms = 0;
            Debug.Log($"{currentFloor}층으로 이동합니다.");
            ReturnToHallway(); 
        }
        else
        {
            Debug.Log("게임 클리어! 모든 층을 정복했습니다.");
        }
    }
}
