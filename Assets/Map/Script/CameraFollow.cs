using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;        // 따라갈 대상 (플레이어)
    public float smoothSpeed = 5f;  // 따라가는 속도 (낮을수록 부드럽지만 느림)
    public Vector3 offset;          // 플레이어로부터 떨어진 거리 (보통 Z축 -10 유지용)

    void LateUpdate()
    {
        // 타겟이 없으면 플레이어 태그로 찾기 시도
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) 
            {
                target = playerObj.transform;
                // 현재 카메라 위치와 플레이어 위치 차이를 오프셋으로 초기화 (원하면 직접 설정 가능)
                // offset = transform.position - target.position; 
                // 보통은 그냥 0,0,-10 정도로 둡니다.
            }
            return;
        }

        // 목표 위치 계산 (플레이어 위치 + 오프셋)
        // 카메라의 Z값은 유지 (-10)
        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        
        // 부드럽게 이동 (Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
