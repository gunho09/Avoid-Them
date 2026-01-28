using UnityEngine;
using System.Collections;

public class DeathEffect : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 1.0f; // 효과 지속 시간
    public Color deathColor = new Color(1f, 0.3f, 0.3f, 1f); // 약간 밝은 빨강

    public void PlayEffect(System.Action onComplete = null)
    {
        // 이미 돌고 있으면 무시
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(DeathRoutine(onComplete));
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    private IEnumerator DeathRoutine(System.Action onComplete)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            // 1. 빨갛게 변하기
            sr.color = deathColor;
        }

        // 물리 충돌 끄기 (시체가 길막하면 안 되니까)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero; // 미끄러짐 방지

        float timer = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0, 0, 90); // 90도 회전 (눕기)
        
        Color startColor = (sr != null) ? sr.color : Color.white;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 투명

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // 2. 옆으로 쓰러지기 (회전)
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);

            // 3. 페이드 아웃 (투명해지기)
            if (sr != null)
            {
                sr.color = Color.Lerp(startColor, targetColor, t);
            }

            yield return null;
        }

        // 끝
        onComplete?.Invoke();
    }
}
