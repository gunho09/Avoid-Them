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
                Debug.Log("CameraFollow: 플레이어를 찾았습니다!");
            }
            else
            {
                Debug.LogWarning("CameraFollow: Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
            }
            return;
        }

        // 목표 위치 계산 (플레이어 위치 + 오프셋)
        // 카메라의 Z값은 항상 -10으로 유지 (2D에서 필수!)
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x, 
            target.position.y + offset.y, 
            -10f  // 강제로 -10 고정
        );
        
        // 부드럽게 이동 (Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
        
        Debug.Log($"Camera Position: {transform.position}, Player Position: {target.position}");
    }
}
