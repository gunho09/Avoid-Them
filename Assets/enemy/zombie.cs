using UnityEngine;
using System.Collections;

public class zombie : MonoBehaviour, IDamageable
{
    // ...
    public float GetHpRatio()
    {
        if (health <= 0) return 0f;
        return currentHealth / (float)health;
    }

    [Header("Stats")]
    public int health;          // 인스펙터에서 설정
    public int attackDamage;    // 인스펙터에서 설정
    public float speed;         // 인스펙터에서 설정
    public float atackSpeed;    // 인스펙터에서 설정
    public int expDrop;         // 인스펙터에서 설정

    [Header("Detection")]
    public float range;         // 인스펙터에서 설정 (예: 10)
    public float attackRange;   // 인스펙터에서 설정 (예: 1.5)

    [Header("References")]
    public PlayerControler playerCtrl;

    private float currentHealth;
    private float lastAttackTime;
    private Transform targetCharacter;
    private enum State { Idle, Chase, Attack, Dead }
    private State currentState = State.Idle;
    private Rigidbody2D rb;

    private bool canAct = false; // 0.5초 경직 플래그
    private bool isStunned = false; // 스턴 상태 플래그

    public void ApplyCcKnockback(Vector2 force)
    {
        if (rb != null)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    public void ApplyCcStun(float duration)
    {
        if (!gameObject.activeSelf) return;
        StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        // 색상 변경 등 시각효과 추가 가능
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    // 타겟 강제 변경 (더미용)
    public void SetTarget(Transform newTarget)
    {
        targetCharacter = newTarget;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // [Golden Balance] 층별 성장 적용
        if (MapManager.Instance != null)
        {
            int floor = MapManager.Instance.currentFloor;
            if (floor > 1)
            {
                // HP: 층당 +55%
                health = Mathf.RoundToInt(health * (1 + 0.55f * (floor - 1)));
                
                // ATK: 층당 +20%
                attackDamage = Mathf.RoundToInt(attackDamage * (1 + 0.20f * (floor - 1)));
            }
        }

        currentHealth = health; // 인스펙터에서 넣은 health 값이 적용됨

        // "Player" 태그를 가진 부모 오브젝트를 찾습니다.
        GameObject playerParent = GameObject.FindGameObjectWithTag("Player");
        if (playerParent != null)
        {
            playerCtrl = playerParent.GetComponentInChildren<PlayerControler>();

            if (playerCtrl != null)
            {
                targetCharacter = playerCtrl.transform;
            }
            else
            {
                targetCharacter = playerParent.transform;
            }
        }

        StartCoroutine(SpawnDelay());
    }

    IEnumerator SpawnDelay()
    {
        yield return new WaitForSeconds(0.5f);
        canAct = true;
    }

    void Update()
    {
        if (currentState == State.Dead || targetCharacter == null || !canAct || isStunned) return;

        float distToPlayer = Vector2.Distance(transform.position, targetCharacter.position);

        if (distToPlayer <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (distToPlayer <= range)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Idle;
        }

        switch (currentState)
        {
            case State.Idle:
                rb.linearVelocity = Vector2.zero;
                break;
            case State.Attack:
                rb.linearVelocity = Vector2.zero;
                AttackPlayer();
                break;
            case State.Chase:
                MoveToPlayer();
                break;
        }
    }

    void MoveToPlayer()
    {
        Vector2 direction = ((Vector2)targetCharacter.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    void AttackPlayer()
    {
        if (Time.time - lastAttackTime >= atackSpeed)
        {
            if (playerCtrl != null)
            {
                playerCtrl.TakeDamage(attackDamage);
                lastAttackTime = Time.time;
            }
        }
    }

    public void TakeDamage(float damage)
    {

        currentHealth -= damage;

        StartCoroutine(HitFlashRoutine());

        if (currentHealth <= 0 && currentState != State.Dead)
        {
            Die();
        }
    }

    void Die()
    {
        if (currentState == State.Dead) return;
        currentState = State.Dead;

        rb.linearVelocity = Vector2.zero;

        RoomControl room = GetComponentInParent<RoomControl>();
        if (room != null)
        {
            room.OnEnemyKilled();
        }

        if (playerCtrl != null)
        {
            playerCtrl.TakeExp(expDrop);
        }

        Destroy(gameObject, 0.2f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    IEnumerator HitFlashRoutine()
    {
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            yield return new WaitForSeconds(0.1f);

            sprite.color = originalColor;
        }
    }
}