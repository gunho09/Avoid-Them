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

    private Animator anim;
    private Vector2 lastLookDirection = Vector2.down;

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

    public void ResetTarget()
    {
        if (playerCtrl != null)
        {
            targetCharacter = playerCtrl.transform;
        }
    }

    public bool IsTargeting(Transform target)
    {
        return targetCharacter == target;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // [Golden Balance] 층별 성장 적용
        if (MapManager.Instance != null)
        {
            int floor = MapManager.Instance.currentFloor;
            if (floor > 1)
            {
                // HP: 층당 +80%
                health = Mathf.RoundToInt(health * (1 + 0.8f * (floor - 1)));
                
                // ATK: +10 * (Floor - 1)
                attackDamage = attackDamage + (10 * (floor - 1));
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
        if (targetCharacter == null && currentState != State.Dead)
        {
            ResetTarget();
            if(targetCharacter == null) return; // 여전히 없으면 리턴
        }

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

        UpdateAnimations();

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

    void UpdateAnimations()
    {
        if (anim == null) return;

        if (currentState == State.Chase && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            lastLookDirection = rb.linearVelocity.normalized;
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }

        anim.SetFloat("InputX", lastLookDirection.x);
        anim.SetFloat("InputY", lastLookDirection.y);
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
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-9", 0.6f); // 좀비 공격 (소리 줄임)

                // 반사 데미지 구현을 위해 공격자(자신) 정보를 함께 전달
                playerCtrl.TakeDamage(attackDamage, this.gameObject);
                if (anim != null)
                {
                    anim.SetTrigger("Attack");
                }
                lastAttackTime = Time.time;
            }
        }
    }
    public void TakeDamage(float damage)
    {
        if (currentState == State.Dead) return;
        
        currentHealth -= damage;
        GetComponent<HitFlashController>()?.Flash();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

// ...
    void Die()
    {
        if (currentState == State.Dead) return;
        currentState = State.Dead;

        playerCtrl.TakeExp(expDrop);

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-10"); // 좀비 사망

        rb.linearVelocity = Vector2.zero;
// ...
        // [NEW] 사망 이펙트 재생
        DeathEffect effect = GetComponent<DeathEffect>();
        if (effect == null) effect = gameObject.AddComponent<DeathEffect>();
        
        // 이펙트 재생 후 삭제
        effect.PlayEffect(() => 
        {
            Destroy(gameObject);
        });
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
        if (currentState == State.Dead) yield break; // 죽었을 땐 깜빡임 스킵 (빨간색 유지 위해)

        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            yield return new WaitForSeconds(0.1f);

            if (currentState != State.Dead) // 살아있을 때만 복구
                sprite.color = originalColor;
        }
    }
}