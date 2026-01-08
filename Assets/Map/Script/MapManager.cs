using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Game Progression")]
    public int currentFloor = 1;         // 현재 층 (1 ~ 4)
    public int maxFloors = 4;            // 최대 층
    public int clearedRooms = 0;         // 현재 층에서 깬 방 개수
    public int totalRoomsPerFloor = 6;   // 층당 방 개수

    [Header("Single Scene Settings")]
    public GameObject player;              
    public GameObject hallwayPrefab;       
    
    [Tooltip("방이 화면 정중앙에 안 올 때, 이 값을 조절해서 방 위치를 맞추세요.")]
    public Vector3 roomPositionCorrection; 
    public Vector3 hallwaySpawnPosition;   
    
    public GameObject[] roomPrefabs;     
    public GameObject bossRoomPrefab;      
    public CameraFollow mainCamera;        

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
      
        SpawnHallway();
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
        GameObject prefabToSpawn = isBoss ? bossRoomPrefab : roomPrefabs[Random.Range(0, roomPrefabs.Length)];
       
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
        clearedRooms++;
        Debug.Log($"방 클리어! 현재 층 완료한 방: {clearedRooms}/{totalRoomsPerFloor}");
    }

    public void ReturnToHallway()
    {
        Debug.Log($"[MapManager] ReturnToHallway called. Target Pos: {lastDoorPosition}");

        
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
            currentRoomInstance = null;
        }

        
        if (hallwayPrefab != null)
        {
            currentHallwayInstance = Instantiate(hallwayPrefab, hallwaySpawnPosition, Quaternion.identity);
            Debug.Log("복도 생성 완료했다 ㅆ벌");
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
        if (currentFloor < maxFloors)
        {
            currentFloor++;
            clearedRooms = 0;
            Debug.Log($"{currentFloor}층으로 이동합니다.");
            ReturnToHallway(); 
        }
        else
        {
            Debug.Log("게임 클리어!");
        }
    }
}
