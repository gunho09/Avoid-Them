using UnityEngine;

public class RoomControl : MonoBehaviour
{
    [Header("Room Settings")]
    public Transform playerSpawnPoint;   
    public Transform cameraPoint;        
    
    //
    
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
        
    }
}
