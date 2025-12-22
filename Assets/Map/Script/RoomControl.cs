using UnityEngine;

public class RoomController : MonoBehaviour
{
    public AreaType areaType = AreaType.Room;
    public Door[] doors;
    public int enemyCount;
    private bool isEntered = false;

    public void OnRoomEntered()
    {
        if (isEntered) return;
        isEntered = true;

        Debug.Log("방 진입 성공");

        foreach (var door in doors)
            door.Close();
    }

    public void OnEnemyKilled()
    {
        enemyCount--;
        Debug.Log("남은 몹 수: " + enemyCount);

        if (enemyCount <= 0)
            OnRoomCleared();
    }

    public void OnRoomCleared()
    {
        Debug.Log("방 클리어!");
        foreach (var door in doors)
            door.Open();
    }
}