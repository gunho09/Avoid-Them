using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
    public float lifeTime = 3f;

    private Vector2 moveDir;
    private Rigidbody2D rb;

    void Awake()
    {
        // 리지드바디 참조 (없으면 에러 방지를 위해 체크)
        rb = GetComponent<Rigidbody2D>();

        // 생성 후 일정 시간이 지나면 자동 삭제 (메모리 관리)
        Destroy(gameObject, lifeTime);
    }

    // 보스가 발사할 때 호출하는 초기화 함수
    public void Init(Vector2 dir)
    {
        moveDir = dir.normalized;

        // 1) 총알이 진행 방향을 바라보게 회전 설정
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 2) 리지드바디가 있다면 즉시 속도 할당
        if (rb != null)
        {
            rb.linearVelocity = moveDir * speed;
        }
    }

    void FixedUpdate()
    {
        // 리지드바디가 없는 경우를 대비한 백업 이동 로직
        if (rb == null)
        {
            transform.Translate(moveDir * speed * Time.fixedDeltaTime, Space.World);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1) IDamageable 인터페이스를 사용하는 방식 (권장)
        IDamageable dmg = other.GetComponentInParent<IDamageable>();

        if (dmg != null)
        {
            dmg.TakeDamage(damage);
            Destroy(gameObject);
        }
        // 2) 혹은 기존처럼 Tag로 체크할 경우 (인터페이스가 없는 경우 대비)
        else if (other.CompareTag("Player"))
        {
            // 부모나 자식에게서 PlayerControler를 찾아 데미지 전달
            var pc = other.GetComponentInParent<PlayerControler>();
            if (pc != null) pc.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}