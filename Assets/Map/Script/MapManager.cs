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
    public GameObject hallwayRoot;         // [NEW] 복도 맵 전체를 담고 있는 부모 오브젝트 (교체용)
    // public Transform roomSpawnPoint;    // (더 이상 사용 안 함: 제자리 교체)
    
    [Tooltip("방이 화면 정중앙에 안 올 때, 이 값을 조절해서 방 위치를 맞추세요.")]
    public Vector3 roomPositionCorrection; // 방 생성 위치 보정값 (카메라가 아닌 방을 이동시킴)
    
    public GameObject[] roomPrefabs;       // 방 프리팹 목록
    public GameObject bossRoomPrefab;      // 보스방 프리팹
    public CameraFollow mainCamera;        // 메인 카메라

    private GameObject currentRoomInstance; 
    private Vector3 lastDoorPosition;       

    // private Vector3 initialCameraPosition; // (카메라 이동 없음)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 카메라 자동 할당
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<CameraFollow>();
            if (mainCamera == null)
            {
                GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
                if (camObj != null) mainCamera = camObj.GetComponent<CameraFollow>();
            }
        }
    }

    public void EnterRoom(Vector3 doorPos)
    {
        lastDoorPosition = doorPos;

        // [Fix] 플레이어가 HallwayRoot의 자식으로 되어있다면, 같이 비활성화되므로 부모 해제
        if (player != null && hallwayRoot != null && player.transform.IsChildOf(hallwayRoot.transform))
        {
            player.transform.SetParent(null);
        }

        // 1. 복도 숨기기
        if (hallwayRoot != null)
        {
            hallwayRoot.SetActive(false);
        }
        else
        {
            Debug.LogError("MapManager: Hallway Root가 할당되지 않았습니다!");
        }

        if (clearedRooms < totalRoomsPerFloor) SpawnRoom(false);
        else SpawnRoom(true);
    }

    private void SpawnRoom(bool isBoss)
    {
        // 2. 제자리(복도 위치)에 방 생성
        GameObject prefabToSpawn = isBoss ? bossRoomPrefab : roomPrefabs[Random.Range(0, roomPrefabs.Length)];
        
        // 생성 위치는 복도 위치(또는 0,0,0) + 보정값
        Vector3 spawnPos = (hallwayRoot != null) ? hallwayRoot.transform.position : Vector3.zero;
        spawnPos += roomPositionCorrection; // 사용자가 설정한 보정값 적용

        if (currentRoomInstance != null) Destroy(currentRoomInstance);
        
        currentRoomInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        Debug.Log($"방 생성 (제자리 교체) 완료");

        // 3. 플레이어 위치 설정 (방 내부 스폰 포인트)
        if (player != null)
        {
            Vector3 playerTargetPos = spawnPos; // 기본값

            RoomControl roomCtrl = currentRoomInstance.GetComponent<RoomControl>();
            if (roomCtrl == null) roomCtrl = currentRoomInstance.GetComponentInChildren<RoomControl>();

            if (roomCtrl != null && roomCtrl.playerSpawnPoint != null)
            {
                playerTargetPos = roomCtrl.playerSpawnPoint.position;
            }
            else
            {
                // 스폰 포인트 없으면 그냥 중앙 근처 혹은 기존 로직
                playerTargetPos = spawnPos + new Vector3(0, -2f, 0); 
            }

            // [Z-Order Fix] 플레이어가 바닥 뒤에 가려지지 않도록 Z값을 앞으로 당김 (-1f)
            player.transform.position = new Vector3(playerTargetPos.x, playerTargetPos.y, -1f);
            
            // [Safety] 플레이어가 꺼져있을 수 있으므로 켜줌
            player.SetActive(true);
        }

        // 4. 카메라 이동 안 함 (복도 뷰 유지)
        // 만약 방 크기가 복도랑 완전히 같다면 카메라는 가만히 있어도 완벽하게 보입니다.
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

        // 2. 복도 다시 보이기
        if (hallwayRoot != null)
        {
            hallwayRoot.SetActive(true);
        }

        // 3. 플레이어를 아까 들어왔던 문 위치로 복귀
        if (player != null)
        {
             // [Z-Order Fix] 복도에서도 플레이어가 잘 보이게 Z값 -1 고정
            player.transform.position = new Vector3(lastDoorPosition.x, lastDoorPosition.y, -1f);
        }

        // 4. 카메라 이동 없음 (복도 위치 그대로)
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
