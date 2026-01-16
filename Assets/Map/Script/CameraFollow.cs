using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset; // 필요 시 사용

    [Header("Background Settings")]
    public Sprite backgroundSprite; // 인스펙터에서 '벽 타일' 이미지를 넣어주세요
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f); // 어두운 회색

    // [단순 이동용] 카메라를 특정 위치로 즉시 이동 (Z값은 -10 유지)
    public void MoveCamera(Vector3 targetPosition)
    {
        transform.position = new Vector3(targetPosition.x, targetPosition.y, -10f);
        // Debug.Log($"Camera Moved to: {transform.position}");
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            // 부드러운 이동 (Smooth Damp 또는 Lerp)
            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, -10f) + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }

    private void Awake()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.backgroundColor = Color.black; // 기본은 검은색
        }
    }

    // [카메라 크기 조절]
    public void SetCameraSize(float size)
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographicSize = size;
        }
    }

    // [가로/세로 길이에 맞춰 카메라 크기 자동 조절]
    public void SetCameraToFit(float width, float height)
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            float screenAspect = cam.aspect;
            float sizeBasedOnHeight = height / 2f;
            float sizeBasedOnWidth = width / screenAspect / 2f;

            // 둘 중 더 큰 값을 선택해야 잘리지 않음
            cam.orthographicSize = Mathf.Max(sizeBasedOnHeight, sizeBasedOnWidth);
        }
    }

    // [카메라 초기화] - 복도로 돌아갈 때 호출
    public void ResetCamera()
    {
        // 복도 기본 사이즈 (가로 18, 세로 10)에 맞춤
        SetCameraToFit(18f, 10f);
    }
}
