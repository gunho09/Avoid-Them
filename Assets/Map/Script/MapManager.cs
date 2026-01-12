using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Game Progression")]
    public int currentFloor = 1;         // 현재 층 (1 ~ 4)
    public int maxFloors = 4;            // 최대 층
    public int clearedRooms = 0;         // 현재 층에서 깬 방 개수
    public int totalRoomsPerFloor = 5;   // 층당 방 개수 (보스 방 진입 조건)

    [Header("Single Scene Settings")]
    public GameObject player;              
    public GameObject hallwayPrefab;       
    
    [Tooltip("방이 화면 정중앙에 안 올 때, 이 값을 조절해서 방 위치를 맞추세요.")]
    public Vector3 roomPositionCorrection; 
    public Vector3 hallwaySpawnPosition;   
    
    public GameObject[] roomPrefabs;     
    public GameObject[] bossRoomPrefabs;      
    public CameraFollow mainCamera;        

    private bool currentStageIsBoss = false; // 현재 방이 보스 방인지 여부
    private GameObject currentRoomInstance; 
    private GameObject currentHallwayInstance; 
    private Vector3 lastDoorPosition;

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

    private void Start()
    {
        // 안전장치: 플레이어가 연결 안 되어 있으면 찾기
        if (player == null)
        {
            GameObject found = GameObject.Find("MainChar"); // 활성화된 것만 찾음
            if (found == null)
            {
                // 비활성화된 것도 찾기 (Transform 검색)
                // Scene 전체에서 찾기는 비용이 크지만 Reset 1회성이므로 시도
                PlayerControler prefabScript = FindFirstObjectByType<PlayerControler>(); // 스크립트 타입으로 찾기 (Unity 2023+)
                if (prefabScript != null) player = prefabScript.gameObject;
            }
            else player = found;
        }

        // 시작 시 플레이어 켜주기
        if (player != null)
        {
            player.SetActive(true);
            // [Revert] 다시 플레이어를 맵 시작 위치(0,0)로 이동시킵니다.
            player.transform.position = new Vector3(hallwaySpawnPosition.x, hallwaySpawnPosition.y, -1f);
        }
        else
        {
            Debug.LogError("MapManager: Player/MainChar를 찾을 수 없습니다! Inspector에서 할당해주세요.");
        }

        SpawnHallway();
        
        // 초기 층 표시
        if (FloorUI.Instance != null)
        {
            FloorUI.Instance.UpdateFloor(currentFloor);
        }
    }

    private void SpawnHallway()
    {
        if (hallwayPrefab != null)
        {
            
            if (currentHallwayInstance != null) Destroy(currentHallwayInstance);
            
           
            GameObject existingHallway = GameObject.Find("CurrentHallway");
            if (existingHallway != null) Destroy(existingHallway);

            currentHallwayInstance = Instantiate(hallwayPrefab, hallwaySpawnPosition, Quaternion.identity);
            currentHallwayInstance.name = "CurrentHallway";
            Debug.Log("복도 생성 완료 (게임 시작)");
        }
        else
        {
            Debug.LogError("MapManager Error: HallwayPrefab이 할당되지 않았습니다!");
        }
    }

    public void EnterRoom(Vector3 doorPos, bool forceBoss = false)
    {
        lastDoorPosition = doorPos;

        
        if (currentHallwayInstance != null)
        {
            Destroy(currentHallwayInstance);
            currentHallwayInstance = null;
        }

        GameObject leftoverHallway = GameObject.Find("CurrentHallway");
        if (leftoverHallway != null)
        {
             Destroy(leftoverHallway);
             Debug.Log("이름으로 찾아낸 잔여 복도 삭제됨");
        }

        if (forceBoss || clearedRooms >= totalRoomsPerFloor) SpawnRoom(true);
        else SpawnRoom(false);
    }

    private void SpawnRoom(bool isBoss)
    {
        currentStageIsBoss = isBoss; // 보스 방 여부 저장
        GameObject prefabToSpawn;
        
        if (isBoss) 
        {
            int index = Mathf.Clamp(currentFloor - 1, 0, bossRoomPrefabs.Length - 1);
            prefabToSpawn = bossRoomPrefabs[index];
            Debug.Log($"보스 방 진입! 층: {currentFloor}, 프리팹 Index: {index}");
        }
        else 
        {
            prefabToSpawn = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
        }
       
        Vector3 spawnPos = hallwaySpawnPosition + roomPositionCorrection;

        if (currentRoomInstance != null) DestroyImmediate(currentRoomInstance);
        
        currentRoomInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        Debug.Log($"방 생성 완료 (위치: {spawnPos})");

       
        if (player != null)
        {
            Vector3 playerTargetPos = spawnPos; 

            RoomControl roomCtrl = currentRoomInstance.GetComponent<RoomControl>();
            if (roomCtrl == null) roomCtrl = currentRoomInstance.GetComponentInChildren<RoomControl>();

            if (roomCtrl != null && roomCtrl.playerSpawnPoint != null)
            {
                playerTargetPos = roomCtrl.playerSpawnPoint.position;
            }
            else
            {
                playerTargetPos = spawnPos + new Vector3(0, -2f, 0); 
            }

            
            player.transform.position = new Vector3(playerTargetPos.x, playerTargetPos.y, -1f);
            player.SetActive(true);
        }
    }

    public void OnRoomCleared()
    {
        if (!currentStageIsBoss) // 보스 방이 아닐 때만 카운트 증가
        {
            clearedRooms++;
            Debug.Log($"방 클리어! 현재 층 완료한 방: {clearedRooms}/{totalRoomsPerFloor}");
        }
        else
        {
            Debug.Log("보스 클리어! 복도로 돌아가면 다음 층으로 이동합니다.");
        }
    }

    public void ReturnToHallway()
    {
        Debug.Log($"[MapManager] ReturnToHallway called. Target Pos: {lastDoorPosition}");

        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
            currentRoomInstance = null;
        }
        
        // 보스 방에서 나왔다면 층 이동 처리
        if (currentStageIsBoss)
        {
            currentStageIsBoss = false; // 초기화
            if (currentFloor < maxFloors)
            {
                NextFloor(); // 다음 층으로 이동 (내부에서 ReturnToHallway 로직 일부 수행하지 않도록 주의하거나, 여기서 복도 생성)
                // NextFloor 함수가 아래에 있으니, 여기서 복도를 또 생성하면 중복될 수 있음.
                // NextFloor에서 ReturnToHallway를 호출하고 있음 -> 무한 재귀 위험?
                // 아니요, NextFloor -> ReturnToHallway 호출 구조이므로, 
                // 여기서 NextFloor를 부르면 -> 다시 ReturnToHallway가 불림 -> currentStageIsBoss가 false이므로 일반 복도 생성 루틴으로 감.
                // 괜찮음. 하지만 NextFloor 호출 후 바로 리턴해야 함.
                return;
            }
            else
            {
                GameClear();
                return;
            }
        }

        
        if (hallwayPrefab != null)
        {
            currentHallwayInstance = Instantiate(hallwayPrefab, hallwaySpawnPosition, Quaternion.identity);
            Debug.Log("복도 생성 완료");
        }
        else
        {
            Debug.LogError("MapManager Error: HallwayPrefab이 할당되지 않았습니다!");
        }

        if (player != null)
        {
           
            player.transform.position = new Vector3(lastDoorPosition.x, lastDoorPosition.y, -1f);
            player.SetActive(true);
            
            Debug.Log($"Player returned to {player.transform.position}");

            // 혹시 대시 중이면 멈추게 가속도 초기화
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
        else
        {
            Debug.LogError("MapManager Error: Player is missing!");
        }
    }

   
    public void NextFloor()
    {
        currentFloor++;
        clearedRooms = 0;
        Debug.Log($"=== {currentFloor}층으로 이동합니다. ===");
        
        // 층 표시 UI 업데이트
        if (FloorUI.Instance != null)
        {
            FloorUI.Instance.UpdateFloor(currentFloor);
        }
        
        // 층 이동 시 복도로 돌아가는 로직 실행
        // 주의: ReturnToHallway를 직접 호출하면 위에서 currentStageIsBoss 체크 로직과 꼬일 수 있음.
        // 하지만 위에서 이미 currentStageIsBoss = false로 만들고 호출했으므로 괜찮음.
        ReturnToHallway(); 
    }

    private void GameClear()
    {
        Debug.Log("!!! GAME CLEAR !!!");
        Debug.Log("축하합니다! 모든 보스를 처치했습니다.");
        
        // 게임 정지 (필요 시 UI 띄우기)
        Time.timeScale = 0f;
        
        // TODO: 게임 클리어 UI 호출 등
    }
}
