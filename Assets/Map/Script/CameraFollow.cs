using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    private Camera cam;
    private Vector3 shakeOffset = Vector3.zero;
    private Coroutine shakeCoroutine;

    // 흔들림 없는 "진짜 카메라 위치"
    private Vector3 basePosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.backgroundColor = Color.black;
        }

        basePosition = transform.position;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, -10f) + offset;

            // 흔들림 없는 위치끼리만 보간
            basePosition = Vector3.Lerp(basePosition, desiredPosition, smoothSpeed);
        }
        else
        {
            // target이 없으면 현재 basePosition 유지
            basePosition = new Vector3(basePosition.x, basePosition.y, -10f);
        }

        // 마지막에만 흔들림 추가
        transform.position = basePosition + shakeOffset;
    }

    public void MoveCamera(Vector3 targetPosition)
    {
        basePosition = new Vector3(targetPosition.x, targetPosition.y, -10f);
        transform.position = basePosition + shakeOffset;
    }

    public void SetCameraSize(float size)
    {
        if (cam != null)
        {
            cam.orthographicSize = size;
        }
    }

    public void SetCameraToFit(float width, float height)
    {
        if (cam != null)
        {
            float screenAspect = cam.aspect;
            float sizeBasedOnHeight = height / 2f;
            float sizeBasedOnWidth = width / screenAspect / 2f;
            cam.orthographicSize = Mathf.Max(sizeBasedOnHeight, sizeBasedOnWidth);
        }
    }

    public void ResetCamera()
    {
        SetCameraToFit(18f, 10f);
    }

    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float damper = 1f - Mathf.Clamp01(elapsed / duration);

            float x = Random.Range(-1f, 1f) * magnitude * damper;
            float y = Random.Range(-1f, 1f) * magnitude * damper;
            shakeOffset = new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeCoroutine = null;
    }
}