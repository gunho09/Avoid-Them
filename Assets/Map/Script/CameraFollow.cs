using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // [단순 이동용] 카메라를 특정 위치로 즉시 이동 (Z값은 -10 유지)
    public void MoveCamera(Vector3 targetPosition)
    {
        transform.position = new Vector3(targetPosition.x, targetPosition.y, -10f);
        Debug.Log($"Camera Moved to: {transform.position}");
    }
}
