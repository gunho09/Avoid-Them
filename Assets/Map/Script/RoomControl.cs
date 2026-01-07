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

        // 배치된 적 세기
      
        var enemies = GetComponentsInChildren<zombie>();
        livingEnemyCount = enemies.Length;

        Debug.Log($"방 진입: 배치된 적 {livingEnemyCount}마리 감지됨");

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
