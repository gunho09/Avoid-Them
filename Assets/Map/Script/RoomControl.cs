using UnityEngine;
using System.Collections.Generic;

public class RoomControl : MonoBehaviour
{
    [Header("Room Settings")]
    public Transform playerSpawnPoint;   
    public Transform cameraPoint;        

    [Tooltip("카메라가 비춰야 할 가로 길이 (World Unit)")]
    public float viewWidth = 18f;
    [Tooltip("카메라가 비춰야 할 세로 길이 (World Unit)")]
    public float viewHeight = 10f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = (cameraPoint != null) ? cameraPoint.position : transform.position;
        Gizmos.DrawWireCube(center, new Vector3(viewWidth, viewHeight, 1));
    }
    
    //
    
    // public GameObject rewardChest; // [삭제됨]
    public GameObject returnDoor;   
    
    [Header("Item Drop Settings")]
    public GameObject itemDropPrefab; // 아이템 드랍 프리팹 (ItemPickup 스크립트가 붙어있어야 함)
    private List<GameObject> activeDrops = new List<GameObject>(); // 현재 방에 떨어진 아이템들 추적   
    
    private int livingEnemyCount = 0; 
    private bool isCleared = false;

    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs;
    public int obstacleCountMin = 3;
    public int obstacleCountMax = 6;
    [Tooltip("체크하면 랜덤 생성, 끄면 직접 배치한 것만 사용")]
    public bool spawnRandomObstacles = true;

    void Start()
    {
        if (spawnRandomObstacles)
        {
            SpawnObstacles();
        }

        // [중요] 장애물 생성(혹은 수동 배치) 후 Grid(경로찾기) 갱신
        GridManager gridMgr = FindFirstObjectByType<GridManager>();
        if (gridMgr != null)
        {
            gridMgr.CreateGrid();
        }


        if (returnDoor != null)
        {
            returnDoor.SetActive(true); 
            Door doorScript = returnDoor.GetComponent<Door>();
            if (doorScript != null)
            {
                doorScript.SetStatus(false);
            }
        }

        // 초기 적 카운트 (참고용)
        CountEnemies();
        Debug.Log($"방 진입: 배치된 적 {livingEnemyCount}마리 감지됨 (Layer 기준)");

        // 1초마다 남은 적 확인 (Layer 기반, 가장 확실한 방법)
        InvokeRepeating(nameof(CheckEnemiesAlive), 1f, 1f);
    }

    void CheckEnemiesAlive()
    {
        if (isCleared) return;

        CountEnemies();

        if (livingEnemyCount <= 0)
        {
            RoomClear();
            CancelInvoke(nameof(CheckEnemiesAlive));
        }
    }

    void CountEnemies()
    {
        int count = 0;
        
        // [수정] 내 자식뿐만 아니라 내 부모(Room 전체)의 자식들도 다 뒤져야 함
        // (RoomControl 스크립트가 Room 루트가 아니라 자식 오브젝트에 붙어있는 경우 대비)
        Transform root = transform;
        if (transform.parent != null) root = transform.parent;

        foreach (Transform child in root.GetComponentsInChildren<Transform>())
        {
            // Layer 이름이 "enemy"인 것만 카운트 (대소문자 무시, 본인 제외)
            string layerName = LayerMask.LayerToName(child.gameObject.layer);
            
            // 자기 자신이나 루트는 제외
            if (child != transform && child != root && child.gameObject.activeInHierarchy && layerName.ToLower() == "enemy")
            {
                count++;
            }
        }
        livingEnemyCount = count;
        // Debug.Log($"[RoomControl] 현재 적 수: {count}");
    }



    
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

        // [변경] 물리 아이템 3개 드랍
        SpawnItemRewards();

        // 기존 UI 호출 코드 삭제/주석
        // if (ItemSelectUI.Instance != null) ...
            
        // if (rewardChest != null)
        //    rewardChest.SetActive(true);

        // 문 열기 로직은 OnItemPicked로 이동됨 (보상 먹어야 열림)
        /*
        if (returnDoor != null)
        {
            
            Door doorScript = returnDoor.GetComponent<Door>();
            if (doorScript != null)
            {
                doorScript.SetStatus(true);
            }
        }
        */

        
        if (MapManager.Instance != null)
            MapManager.Instance.OnRoomCleared();
    }

    void SpawnItemRewards()
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("SpawnItemRewards Failed: ItemDatabase.Instance is null! Scene에 ItemDatabase가 있는지 확인하세요.");
            return;
        }

        if (itemDropPrefab == null)
        {
            Debug.LogError($"SpawnItemRewards Failed: RoomControl({gameObject.name})에 'Item Drop Prefab'이 연결되지 않았습니다! 인스펙터에서 확인해주세요.");
            return;
        }

        List<ItemData> rewards = ItemDatabase.Instance.GetRandomItems(3);
        activeDrops.Clear();

        // 방 중앙 기준 왼쪽/중앙/오른쪽 offset
        Vector3[] offsets = { new Vector3(-2, 0, 0), Vector3.zero, new Vector3(2, 0, 0) };

        // 아이템 3개 생성
        for (int i = 0; i < rewards.Count; i++)
        {
            if (i >= offsets.Length) break;

            Vector3 spawnPos = transform.position + offsets[i];
            
            // 프리팹 생성
            GameObject drop = Instantiate(itemDropPrefab, spawnPos, Quaternion.identity);
            
            // 데이터 설정 & 본인(RoomControl) 연결
            ItemPickup pickup = drop.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.Setup(rewards[i], this);
            }

            activeDrops.Add(drop);
        }
    }

    public void OnItemPicked()
    {
        // 하나를 먹으면 나머지는 싹 없앰
        foreach (GameObject drop in activeDrops)
        {
            if (drop != null)
            {
                Destroy(drop);
            }
        }
        activeDrops.Clear();

        // 문 열기 (보상 획득 후에만 열리도록)
        if (returnDoor != null)
        {
            Door doorScript = returnDoor.GetComponent<Door>();
            if (doorScript != null)
            {
                doorScript.SetStatus(true);
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-2"); // 문 여는 소리
            }
        }
        Debug.Log("아이템 획득 완료! 문이 열립니다.");
    }
    
    public void OnRoomEntered()
    {
        Debug.Log("Player entered the room");
        
    }

    void SpawnObstacles()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        int count = Random.Range(obstacleCountMin, obstacleCountMax + 1);

        // 방 범위 내 랜덤 (여백 2f 줌)
        float xRange = (viewWidth / 2f) - 2f;
        float yRange = (viewHeight / 2f) - 2f;

        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-xRange, xRange),
                Random.Range(-yRange, yRange),
                0
            );

            // 플레이어 시작 위치나 문 근처에는 안 생기게 거리 체크 (2유닛 이내면 스킵)
            if (playerSpawnPoint != null && Vector3.Distance(randomPos, playerSpawnPoint.position) < 2f) continue;
            if (returnDoor != null && Vector3.Distance(randomPos, returnDoor.transform.position) < 2f) continue;

            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            Instantiate(prefab, randomPos, Quaternion.identity, transform); // 방의 자식으로 생성
        }
    }
}
