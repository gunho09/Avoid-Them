using UnityEngine;
using System.Collections;

public class Boss4 : MonoBehaviour, IDamageable
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
    public float attackRange1;   // 인스펙터에서 설정 (예: 1.5)
    public float attackRange2;   // 인스펙터에서 설정 (예: 1.5)

    [Header("References")]
    public PlayerControler PlayerControler;

    private float currentHealth;
    private float lastAttackTime;
    private Transform targetCharacter;
    private enum State { Idle, Chase, Attack1, Attack2, Dead, Skill1, Skill2, Skill3, Move }
    private State currentState = State.Idle;
    private Rigidbody2D rb;

    private bool canAct = false; // 0.5초 경직 플래그

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

        StartCoroutine(SpawnDelay());
    }

    IEnumerator SpawnDelay()
    {
        yield return new WaitForSeconds(0.5f);
        canAct = true;
    }

    void Update()
    {
        
        if (currentState == State.Dead || targetCharacter == null || !canAct) return;

        float distToPlayer = Vector2.Distance(transform.position, targetCharacter.position);

        if (distToPlayer <= attackRange2 && attackRange2 > attackRange1)
        {
            currentState = State.Attack2;
        }
        else if (distToPlayer <= attackRange1)
        {
            currentState = State.Attack1;
        }
        

        else if (distToPlayer <= range)
        {
            currentState = State.Skill1;
        }

        else if (distToPlayer <= range)
        {
            currentState = State.Skill2;
        }

        else if (distToPlayer <= range)
        {
            currentState = State.Skill3;
        }

        else
        {
            currentState = State.Move;
        }



        switch (currentState)
        {
            case State.Idle:
                rb.linearVelocity = Vector2.zero;
                break;
            case State.Attack2:
                rb.linearVelocity = Vector2.zero;
                AttackPlayer2();
                break;
            case State.Attack1:
                rb.linearVelocity = Vector2.zero;
                AttackPlayer1();
                break;

            case State.Move:
                MoveToPlayer();
                break;
        }
    }

    void MoveToPlayer()
    {
        Vector2 direction = ((Vector2)targetCharacter.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    void AttackPlayer1()
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

    void AttackPlayer2()
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

    void Skill1()
    {
        

    }

    void Skill2()
    {


    }

    void Skill3()
    {


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