using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Game Progression")]
    public int currentFloor = 1;         // 현재 층 (1 ~ 4)
    public int maxFloors = 4;            // 최대 층
    public int clearedRooms = 0;         // 현재 층에서 깬 방 개수
    public int totalRoomsPerFloor = 5;   // 층당 방 개수 (보스 방 진입 조건)
    public bool isBossDead = false;     // 보스 처치 여부 (보스 방 진입 조건)

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
    
    // [방문한 문 위치 저장용]
    private System.Collections.Generic.List<Vector3> visitedDoors = new System.Collections.Generic.List<Vector3>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (ScreenFader.Instance == null)
        {
            Debug.LogWarning("[MapManager] ScreenFader Instance is missing. Transitions will be instantaneous.");
        }

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
        // [강제 설정] 인스펙터 실수 방지: 무조건 5개로 고정
        totalRoomsPerFloor = 5;

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

        // [카메라 타겟 해제] 복도에서는 고정 카메라
        if (mainCamera != null)
        {
            mainCamera.target = null;
            mainCamera.MoveCamera(hallwaySpawnPosition); // 복도 위치로 카메라 이동
        }

        SpawnHallway();
        
        // 초기 층 표시
        if (FloorUI.Instance != null)
        {
            FloorUI.Instance.UpdateFloor(currentFloor);
        }
    }

    // [Sound] 플레이어 발소리 체크용
    public bool IsInHallway => currentHallwayInstance != null;

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

            // [Sound] 복도 BGM
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBGM("1-1");

            // [이미 들어갔던 문 끄기]
            DisableVisitedDoors();
        }
        else
        {
            Debug.LogError("MapManager Error: HallwayPrefab이 할당되지 않았습니다!");
        }
    }

    public void EnterRoom(Vector3 doorPos, bool forceBoss = false)
    {
        StartCoroutine(EnterRoomRoutine(doorPos, forceBoss));
    }

    private IEnumerator EnterRoomRoutine(Vector3 doorPos, bool forceBoss)
    {
        // [New] 방 이동 시 필드에 남아있는 임시 오브젝트(더미 등) 정리
        ClearTemporaryObjects();

        PlayerControler pc = player != null ? player.GetComponent<PlayerControler>() : null;
        if (pc != null) pc.canMove = false;

        bool fadeCompleted = false;
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeOut(() => fadeCompleted = true);
            while (!fadeCompleted) yield return null;
        }

        lastDoorPosition = doorPos;
        
        if (!visitedDoors.Contains(doorPos))
        {
            visitedDoors.Add(doorPos);
        }

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

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeIn();
        }

        if (pc != null) pc.canMove = true;
    }

    private void SpawnRoom(bool isBoss)
    {
        currentStageIsBoss = isBoss; // 보스 방 여부 저장
        GameObject prefabToSpawn;
        
        if (isBoss) 
        {
            // [Sound] 보스방 BGM
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBGM("1-3");

            int index = Mathf.Clamp(currentFloor - 1, 0, bossRoomPrefabs.Length - 1);
            prefabToSpawn = bossRoomPrefabs[index];
            Debug.Log($"보스 방 진입! 층: {currentFloor}, 프리팹 Index: {index}");
        }
        else 
        {
            // [Sound] 일반방 BGM
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBGM("1-2");

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
                
                if (mainCamera != null)
                {
                    // [카메라 타겟 해제] 방에서는 고정
                    mainCamera.target = null;

                    // [카메라 위치 이동]
                    Vector3 camCenter = (roomCtrl.cameraPoint != null) ? roomCtrl.cameraPoint.position : roomCtrl.transform.position;
                    mainCamera.MoveCamera(camCenter);

                    // [카메라 크기 적용]
                    mainCamera.SetCameraToFit(roomCtrl.viewWidth, roomCtrl.viewHeight);
                }
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
        StartCoroutine(ReturnToHallwayRoutine());
    }

    private IEnumerator ReturnToHallwayRoutine()
    {
        // [New] 복도로 돌아갈 때도 더미 등 정리
        ClearTemporaryObjects();

        PlayerControler pc = player != null ? player.GetComponent<PlayerControler>() : null;
        if (pc != null) pc.canMove = false;

        bool fadeCompleted = false;
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeOut(() => fadeCompleted = true);
            while (!fadeCompleted) yield return null;
        }

        Debug.Log($"[MapManager] ReturnToHallway called. Target Pos: {lastDoorPosition}");

        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
            currentRoomInstance = null;
        }
        
        // 보스 방에서 나왔다면 층 이동 처리
        if (currentStageIsBoss)
        {
            if (!isBossDead)
            {
                currentStageIsBoss = false; 
            }
            else
            {
                currentStageIsBoss = false; 
                isBossDead = false;

                if (currentFloor < maxFloors)
                {
                    NextFloor();
                    if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();
                    yield break;
                }
                else
                {
                    GameClear();
                    if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();
                    yield break;
                }
            }
        }

        if (hallwayPrefab != null)
        {
            if (currentHallwayInstance != null) Destroy(currentHallwayInstance);
            currentHallwayInstance = Instantiate(hallwayPrefab, hallwaySpawnPosition, Quaternion.identity);
            currentHallwayInstance.name = "CurrentHallway"; 

            if (SoundManager.Instance != null) SoundManager.Instance.PlayBGM("1-1");
            
            DisableVisitedDoors();
            
            if (mainCamera != null)
            {
                mainCamera.target = null;
                mainCamera.MoveCamera(hallwaySpawnPosition);
                mainCamera.ResetCamera();
            }
        }

        if (player != null)
        {
            player.transform.position = new Vector3(lastDoorPosition.x, lastDoorPosition.y, -1f);
            player.SetActive(true);
            
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeIn();
        }

        if (pc != null) pc.canMove = true;
    }

   
    public void NextFloor()
    {
        currentFloor++;
        clearedRooms = 0;
        visitedDoors.Clear(); // [새 층이므로 방문 기록 초기화]
        Debug.Log($"=== {currentFloor}층으로 이동합니다. ===");
        
        // 층 표시 UI 업데이트
        if (FloorUI.Instance != null)
        {
            FloorUI.Instance.UpdateFloor(currentFloor);
        }

        lastDoorPosition = hallwaySpawnPosition;
        
        // 층 이동 시 복도로 돌아가는 로직 실행
        // 주의: ReturnToHallway를 직접 호출하면 위에서 currentStageIsBoss 체크 로직과 꼬일 수 있음.
        // 하지만 위에서 이미 currentStageIsBoss = false로 만들고 호출했으므로 괜찮음.
        ReturnToHallway(); 
    }

    private void GameClear()
    {
        Debug.Log("!!! GAME CLEAR !!!");
        Debug.Log("축하합니다! 모든 보스를 처치했습니다.");
        
        // 게임 클리어 UI 씬 호출
        UnityEngine.SceneManagement.SceneManager.LoadScene("ClearUI");
    }

    // [방문한 문 비활성화 함수]
    private void DisableVisitedDoors()
    {
        if (currentHallwayInstance == null) return;
        
        Door[] doors = currentHallwayInstance.GetComponentsInChildren<Door>();
        foreach (Door door in doors)
        {
            // Door.cs에서 보내는 위치는 transform.position + returnOffset 이었습니다.
            // 하지만 우리는 비교를 위해, '입장했을 때 저장된 위치(lastDoorPosition)'와
            // '지금 이 문의 복귀 예상 위치(transform.position + returnOffset)'가 같은지 봅니다.
            // 혹은 더 간단히: lastDoorPosition은 '들어갔던 문의 복귀 위치'입니다.
            // 이 문이 그 문인지 확인하려면:
            
            Vector3 thisDoorReturnPos = door.transform.position + door.returnOffset;
            
            // 위치 비교 (오차 감안)
            foreach (Vector3 visitedPos in visitedDoors)
            {
                if (Vector3.Distance(thisDoorReturnPos, visitedPos) < 0.1f)
                {
                    // 방문했던 문 처리
                    // 1. 기능 끄기 (열지 못하게)
                    door.SetStatus(false);
                    
                    // 2. 시각적으로 '닫힘/어두움' 표시
                    if (door.spriteRenderer != null)
                    {
                        door.spriteRenderer.color = Color.gray; // 회색으로 어둡게 처리
                    }
                    
                    // 3. 더 이상 상호작용 안 되게 Collider 끄기 (선택 사항)
                    Collider2D col = door.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;

                    break;
                }
            }
        }
    }

    // [New] 방/복도 이동 시 남아있는 소환물(더미 등)을 강제로 제거
    private void ClearTemporaryObjects()
    {
        DummyItem[] dummies = FindObjectsByType<DummyItem>(FindObjectsSortMode.None);
        foreach (var d in dummies)
        {
            if (d != null) Destroy(d.gameObject);
        }
        Debug.Log("[MapManager] Temporary objects cleared.");
    }
}
