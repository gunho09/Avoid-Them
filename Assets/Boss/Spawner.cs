using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject zombiePrefab;
    public Transform spawnPoint; // 비워두면 스포너 위치에서 생성
    public float spawnInterval = 1.5f;

    [Header("Life Time")]
    public float lifeTime = 5f;

    private Coroutine spawnRoutine;

    void OnEnable()
    {
        // 켜질 때마다 루틴 시작 (보스가 SetActive(true) 할 때 동작)
        spawnRoutine = StartCoroutine(SpawnLoop());

        // lifeTime 후 스포너 종료
        StartCoroutine(DieAfterTime());
    }

    void OnDisable()
    {
        // 혹시 중간에 꺼지면 코루틴 정리
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }

    IEnumerator SpawnLoop()
    {
        // 바로 한 마리 나오게 하고 싶으면 이 줄을 지워도 됨
        // yield return null;

        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOne()
    {
        if (zombiePrefab == null) return;

        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : transform.position;
        Instantiate(zombiePrefab, pos, Quaternion.identity);
    }

    IEnumerator DieAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);

        // 1) 그냥 사라지게: 비활성
        gameObject.SetActive(false);

        // 2) 완전히 제거하고 싶으면 위 줄 대신 이걸 써:
        // Destroy(gameObject);
    }
}