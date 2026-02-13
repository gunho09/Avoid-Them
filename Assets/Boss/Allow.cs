using UnityEngine;

public class Allow : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;

    [Header("Optional")]
    public bool destroyOnWall = true;       // 벽/바닥에 닿으면 파괴할지
    public LayerMask wallMask;              // Ground/Wall 레이어를 넣어두면 됨

    private Vector2 moveDir;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Boss가 넘겨준 방향
    public void Init(Vector2 dir)
    {
        moveDir = dir.normalized;

        // 화살 스프라이트가 이동 방향을 바라보게 회전 (기본이 오른쪽 기준)
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = moveDir * speed;   // 안 되면 rb.velocity로 바꿔
            // rb.velocity = moveDir * speed;
        }
        else
        {
            transform.Translate(moveDir * speed * Time.fixedDeltaTime, Space.World);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 무엇과 부딪혔는지 콘솔창에 찍어봅니다.
        Debug.Log("화살이 충돌함: " + other.name);

        // 1) IDamageable 찾기
        IDamageable dmg = other.GetComponentInParent<IDamageable>();

        if (dmg != null)
        {
            Debug.Log("IDamageable 인터페이스를 찾았습니다!");
            dmg.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 2) 벽 충돌 체크 (로그 추가)
        if (destroyOnWall && ((1 << other.gameObject.layer) & wallMask) != 0)
        {
            Debug.Log("벽에 부딪혀 사라짐");
            Destroy(gameObject);
        }
    }
}
