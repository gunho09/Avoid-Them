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
    public Vector2 roomCameraOffset;       // 방 카메라 위치 보정값 (중앙 맞추기용)
    public GameObject[] roomPrefabs;       // 방 프리팹 목록 (랜덤 선택)
    public GameObject bossRoomPrefab;      // 보스방 프리팹
    public CameraFollow mainCamera;        // 메인 카메라 (Inspector 할당 or 자동 찾기)

    private GameObject currentRoomInstance; // 현재 생성된 방 인스턴스
    private Vector3 lastDoorPosition;       // 들어왔던 문 위치 기억

    private Vector3 initialCameraPosition; // 게임 시작 시 카메라 위치 (복도)

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

        // 카메라 자동 할당 시도
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<CameraFollow>();
            if (mainCamera == null)
            {
                // 백업: 태그로 찾아서 컴포넌트 가져오기
                GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
                if (camObj != null)
                    mainCamera = camObj.GetComponent<CameraFollow>();
            }
        }

        // 시작 시점의 카메라 위치 저장 (복도 위치로 가정)
        if (mainCamera != null)
        {
            initialCameraPosition = mainCamera.transform.position;
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
        Debug.Log($"방 생성 완료! 위치: {roomSpawnPoint.position}");

        // 2. 플레이어 순간이동 (문의 위치로)
        if (player != null)
        {
            Vector3 spawnPos = roomSpawnPoint.position; // 기본값 (실패 시)

            // 방 설정(RoomControl) 확인
            RoomControl roomCtrl = currentRoomInstance.GetComponent<RoomControl>();
            if (roomCtrl == null)
            {
                // 루트에 없으면 자식에서 찾기 시도
                roomCtrl = currentRoomInstance.GetComponentInChildren<RoomControl>();
            }

            if (roomCtrl != null)
            {
                Debug.Log("MapManager: RoomControl 찾음.");
                
                // 1순위: 지정된 플레이어 스폰 포인트가 있는지 확인
                if (roomCtrl.playerSpawnPoint != null)
                {
                    spawnPos = roomCtrl.playerSpawnPoint.position;
                    Debug.Log($"MapManager: Player Spawn Point 발견! 위치: {spawnPos}, 이름: {roomCtrl.playerSpawnPoint.name}");
                }
                // 2순위: 스폰 포인트가 없으면 문(Door) 위치 확인
                else if (roomCtrl.returnDoor != null)
                {
                    Debug.Log("MapManager: Player Spawn Point 없음. Return Door 위치 사용 시도.");
                    Door doorScript = roomCtrl.returnDoor.GetComponent<Door>();
                    if (doorScript != null)
                    {
                        spawnPos = roomCtrl.returnDoor.transform.position + doorScript.returnOffset;
                    }
                    else
                    {
                        spawnPos = roomCtrl.returnDoor.transform.position + new Vector3(0, -1.5f, 0);
                    }
                }
                else
                {
                    Debug.LogWarning("MapManager: RoomControl에 SpawnPoint도 없고 ReturnDoor도 연결되지 않았습니다!");
                }
            }
            else
            {
                Debug.LogError("MapManager: 생성된 방에 RoomControl 컴포넌트가 없습니다!");
            }

            Debug.Log($"플레이어 최종 이동 위치: {spawnPos}");
            player.transform.position = spawnPos;

            // 3. 카메라 이동 (방 위치로 - 방 생성 포인트 기준 + 보정)
            if (mainCamera != null)
            {
                // 방 생성 위치 + 보정값(Offset)으로 카메라 이동
                Vector3 camPos = roomSpawnPoint.position + (Vector3)roomCameraOffset;
                mainCamera.MoveCamera(camPos);
            }
            else
            {
                Debug.LogWarning("MapManager: MainCamera가 연결되지 않았습니다!");
            }
        }
        else
        {
            Debug.LogError("Player가 null입니다!");
        }
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

        // 3. 카메라 복귀 (저장해둔 복도 위치로)
        if (mainCamera != null)
        {
            mainCamera.MoveCamera(initialCameraPosition);
        }
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
