using UnityEngine;
using System.Collections;

public class zombie : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int health;          // 인스펙터에서 설정
    public int attackDamage;    // 인스펙터에서 설정
    public float speed;         // 인스펙터에서 설정
    public float atackSpeed;    // 인스펙터에서 설정
    public float dodgeChance;   // 인스펙터에서 설정
    public int expDrop;         // 인스펙터에서 설정

    [Header("Detection")]
    public float range;         // 인스펙터에서 설정 (예: 10)
    public float attackRange;   // 인스펙터에서 설정 (예: 1.5)

    [Header("References")]
    public PlayerControler PlayerControler;

    private float currentHealth;
    private float lastAttackTime;
    private Transform targetCharacter;
    private enum State { Idle, Chase, Attack, Dead }
    private State currentState = State.Idle;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = health; // 인스펙터에서 넣은 health 값이 적용됨

        // "Player" 태그를 가진 부모 오브젝트를 찾습니다.
        GameObject playerParent = GameObject.FindGameObjectWithTag("Player");
        if (playerParent != null)
        {
            PlayerControler = playerParent.GetComponentInChildren<PlayerControler>();

            if (PlayerControler != null)
            {
                targetCharacter = PlayerControler.transform;
            }
            else
            {
                targetCharacter = playerParent.transform;
            }
        }
    }

    void Update()
    {
        if (currentState == State.Dead || targetCharacter == null) return;

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
            if (PlayerControler != null)
            {
                PlayerControler.TakeDamage(attackDamage);
                lastAttackTime = Time.time;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (Random.value < dodgeChance) return;

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

        if (PlayerControler != null)
        {
            PlayerControler.TakeExp(expDrop);
        }

        Destroy(gameObject, 1f);
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