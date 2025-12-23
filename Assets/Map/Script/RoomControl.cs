using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Room Type")]
    public bool isBossRoom = false;

    [Header("Spawn Points")]
    public Transform[] enemySpawnPoints;   // 일반 몹 스폰 위치 (4개)
    public Transform bossSpawnPoint;        // 보스 스폰 위치 (보스룸 전용)

    [Header("Door")]
    public Door[] doors;

    [Header("Enemy")]
    [SerializeField] private int enemyCount = 4; // 고정 몹 수

    [Header("Chest")]
    public GameObject chestPrefab;
    public Transform chestSpawnPoint;       // 방 중앙

    private bool isEntered = false;
    private bool isCleared = false;

    // =========================
    // 방 진입
    // =========================
    public void OnRoomEntered()
    {
        if (isEntered) return;
        isEntered = true;

        Debug.Log(isBossRoom ? "보스룸 진입" : "일반 방 진입");

        // 문 닫기
        foreach (var door in doors)
            door.Close();

        // 여기서 몹 개발자가 스폰 시작하면 됨
        if (isBossRoom)
            Debug.Log("보스 스폰 시작 위치: " + bossSpawnPoint.position);
        else
            Debug.Log("일반 몹 스폰 시작");
    }

    // =========================
    // 몹 사망 보고 (몹 개발자가 호출)
    // =========================
    public void OnEnemyKilled()
    {
        if (isCleared) return;

        enemyCount--;
        Debug.Log("남은 몹 수: " + enemyCount);

        if (enemyCount <= 0)
            OnRoomCleared();
    }

    // =========================
    // 방 클리어
    // =========================
    void OnRoomCleared()
    {
        if (isCleared) return;
        isCleared = true;

        Debug.Log("방 클리어!");

        // 일반방만 상자 생성
        if (!isBossRoom)
            SpawnChest();

        // 문 열기
        foreach (var door in doors)
            door.Open();
    }

    // =========================
    // 상자 생성
    // =========================
    void SpawnChest()
    {
        if (chestPrefab == null || chestSpawnPoint == null)
        {
            Debug.LogWarning("ChestPrefab 또는 ChestSpawnPoint 없음");
            return;
        }

        Instantiate(chestPrefab, chestSpawnPoint.position, Quaternion.identity);
        Debug.Log("상자 생성");
    }
}
