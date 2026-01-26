using UnityEngine;

public class ElectricFieldVisual : MonoBehaviour
{
    // 설정 변수
    public float rotationSpeed = 360f;     // 회전 속도 (빠르게)
    public float scaleJitter = 0.2f;       // 크기 떨림 정도
    public float alphaJitter = 0.2f;       // 투명도 깜빡임 정도
    public Color electricColor = new Color(0.4f, 0.6f, 1f); // 밝은 하늘색 (전기 느낌)

    private Vector3 initialScale;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
        
        if (sr != null)
        {
            sr.color = electricColor;
        }
    }

    void Update()
    {
        // 1. 미친듯이 회전 (전기장 느낌)
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime * (Random.value > 0.5f ? 1 : -1));

        // 2. 크기 찌릿찌릿 (Jitter)
        float s = Random.Range(1f - scaleJitter, 1f + scaleJitter);
        transform.localScale = initialScale * s;

        // 3. 투명도 깜빡임
        if (sr != null)
        {
            float a = Random.Range(1f - alphaJitter, 1f);
            Color c = sr.color;
            c.a = a * 0.5f; // 기본적으로 반투명
            sr.color = c;
        }
    }
}
