using UnityEngine;

public class RoomControl : MonoBehaviour
{
    [Header("Room Settings")]
    public Transform[] enemySpawnPoints; // 적들이 생성될 위치
    public GameObject[] enemyPrefabs;    // 생성할 적 프리팹 목록 
    
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
                doorScript.isOpen = false;
            }
        }

        
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
       
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || enemySpawnPoints == null)
        {
            if (livingEnemyCount == 0) RoomClear();
            return;
        }

        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            if (spawnPoint != null)
            {
               
                int randomIndex = Random.Range(0, enemyPrefabs.Length);
                GameObject selectedPrefab = enemyPrefabs[randomIndex];

                if (selectedPrefab != null)
                {
                    
                    GameObject enemy = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
                    
                    // 생성한 적을 방의 자식으로 설정
                    enemy.transform.SetParent(this.transform); 
                    
                    // 몹 수 증가
                    livingEnemyCount++;
                }
            }
        }

        
        if (livingEnemyCount == 0)
        {
            RoomClear();
        }
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

        if (rewardChest != null)
            rewardChest.SetActive(true);

        if (returnDoor != null)
        {
            
            Door doorScript = returnDoor.GetComponent<Door>();
            if (doorScript != null)
            {
                doorScript.isOpen = true;
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
