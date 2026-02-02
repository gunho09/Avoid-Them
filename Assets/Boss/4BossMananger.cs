using UnityEngine;
using System.Collections;

public class Boss4 : MonoBehaviour, IDamageable
{
    /* =======================
     * Stats
     * ======================= */
    [Header("Stats")]
    public int maxHealth = 100;
    public int attackDamage = 10;
    public float speed = 3f;
    public float attackCooldown = 1.5f;
    public float dodgeChance = 0.1f;
    public int expDrop = 100;

    private float currentHealth;
    private float lastAttackTime;

    /* =======================
     * Player References
     * ======================= */
    private Transform playerTransform;
    private PlayerControler playerController;

    /* =======================
     * Movement
     * ======================= */
    [Header("Movement")]
    public float stopDistance = 1.5f;
    private Rigidbody2D rb;

    /* =======================
     * State
     * ======================= */
    private enum State { Idle, Combo1, Combo2, Combo3, Combo4, Dead }
    private State currentState = State.Idle;
    private bool canAct = false;

    /* =======================
     * Poison Skill
     * ======================= */
    [Header("Poison")]
    public GameObject poison1;
    public GameObject poison2;
    public GameObject poison3;
    public GameObject poison4;
    public float poisonOffDelay = 5f;

    /* =======================
     * Spawner Skill
     * ======================= */
    [Header("Spawner")]
    public GameObject spawner;
    public float spawnerOffDelay = 5f;

    /* =======================
     * Melee Attack
     * ======================= */
    [Header("Melee Attack")]
    public Vector2 boxCenter;
    public Vector2 boxSize = new Vector2(2f, 2f);
    public float angle = 0f;
    public LayerMask playerLayer;
    public float knockbackPower = 8f;

    /* =======================
     * Ranged Attack
     * ======================= */
    [Header("Ranged Attack")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    private Transform firePoint;

    /* =======================
     * Init
     * ======================= */
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObj.transform;
        playerController = playerObj.GetComponentInChildren<PlayerControler>();

        firePoint = transform.Find("FirePoint"); // 보스 자식 빈 오브젝트

        StartCoroutine(SpawnDelay());
    }

    IEnumerator SpawnDelay()
    {
        yield return new WaitForSeconds(0.5f);
        canAct = true;
    }

    /* =======================
     * Update
     * ======================= */
    void Update()
    {
        if (!canAct || currentState == State.Dead) return;

        if (Time.time - lastAttackTime < attackCooldown) return;

        int r = Random.Range(1, 5);
        currentState = (State)r;

        switch (currentState)
        {
            case State.Idle:
                rb.linearVelocity = Vector2.zero;
                break;
            case State.Combo1:
                Combo1();
                break;
            case State.Combo2:
                Combo2();
                break;
            case State.Combo3:
                Combo3();
                break;
            case State.Combo4:
                Combo4();
                break;
        }
    }

    /* =======================
     * Movement
     * ======================= */
    void MoveToPlayer()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance > stopDistance)
        {
            Vector2 dir =
                ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    /* =======================
     * Attacks
     * ======================= */
    void MeleeAttack()
    {
        playerController.TakeDamage(attackDamage);

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(boxCenter, boxSize, angle, playerLayer);

        foreach (Collider2D hit in hits)
        {
            Rigidbody2D hitRb = hit.GetComponent<Rigidbody2D>();
            if (hitRb == null) continue;

            Vector2 dir =
                ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;

            hitRb.linearVelocity = Vector2.zero;
            hitRb.AddForce(dir * knockbackPower, ForceMode2D.Impulse);
        }

        lastAttackTime = Time.time;
    }

    void RangedAttack()
    {
        Vector2 dir =
            ((Vector2)playerTransform.position - (Vector2)firePoint.position).normalized;

        GameObject bullet =
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Bullet b = bullet.GetComponent<Bullet>();
        b.speed = bulletSpeed;
        b.Init(dir);

        lastAttackTime = Time.time;
    }

    /* =======================
     * Skills
     * ======================= */
    void SkillPoison()
    {
        poison1.SetActive(true);
        poison2.SetActive(true);
        poison3.SetActive(true);
        poison4.SetActive(true);
        StartCoroutine(PoisonOff());
    }

    void SkillSpawner()
    {
        spawner.SetActive(true);
        StartCoroutine(SpawnerOff());
    }

    /* =======================
     * Combos
     * ======================= */
    void Combo1()
    {
        MoveToPlayer();
        MeleeAttack();
        SkillPoison();
    }

    void Combo2()
    {
        MoveToPlayer();
        MeleeAttack();
        RangedAttack();
    }

    void Combo3()
    {
        RangedAttack();
        SkillSpawner();
    }

    void Combo4()
    {
        RangedAttack();
        SkillPoison();
        SkillSpawner();
    }

    /* =======================
     * Damage / Death
     * ======================= */
    public void TakeDamage(float damage)
    {
        if (Random.value < dodgeChance) return;

        currentHealth -= damage;
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        currentState = State.Dead;
        rb.linearVelocity = Vector2.zero;

        RoomControl room = GetComponentInParent<RoomControl>();
        if (room != null)
            room.OnEnemyKilled();

        playerController.TakeExp(expDrop);
        Destroy(gameObject, 1f);
    }

    public float GetHpRatio()
    {
        return currentHealth / maxHealth;
    }

    /* =======================
     * Coroutines
     * ======================= */
    IEnumerator HitFlash()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color origin = sr.color;
            sr.color = Color.gray;
            yield return new WaitForSeconds(0.1f);
            sr.color = origin;
        }
    }

    IEnumerator PoisonOff()
    {
        yield return new WaitForSeconds(poisonOffDelay);
        poison1.SetActive(false);
        poison2.SetActive(false);
        poison3.SetActive(false);
        poison4.SetActive(false);
    }

    IEnumerator SpawnerOff()
    {
        yield return new WaitForSeconds(spawnerOffDelay);
        spawner.SetActive(false);
    }
}
