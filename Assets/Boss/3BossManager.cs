using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boss3 : MonoBehaviour, IDamageable
{
    /* =======================
     * Damage Tracker
     * ======================= */
    private struct DamageRecord
    {
        public float damage;
        public float time;
    }

    private List<DamageRecord> damageRecords = new List<DamageRecord>();

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
     * State
     * ======================= */
    private enum State { Idle, Combo1, Combo2, Combo3, Combo4, Dead }
    private State currentState = State.Idle;
    private bool canAct = false;

    /* =======================
     * Movement
     * ======================= */
    private Rigidbody2D rb;

    /* =======================
     * Ranged Attack (Arrow)
     * ======================= */
    [Header("Arrow Attack")]
    public GameObject arrowPrefab;
    public float arrowSpeed = 10f;
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

        firePoint = transform.Find("FirePoint");

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

        currentState = (State)Random.Range(1, 5);

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
     * Attacks
     * ======================= */
    void ShootArrow()
    {
        Vector2 dir =
            ((Vector2)playerTransform.position - (Vector2)firePoint.position).normalized;

        GameObject arrow =
            Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        Allow arrowScript = arrow.GetComponent<Allow>();
        arrowScript.speed = arrowSpeed;
        arrowScript.Init(dir);

        lastAttackTime = Time.time;
    }

    /* =======================
     * Skills
     * ======================= */
    void SkillDamageReflect(float window)
    {
        float damage = GetDamageLastSeconds(window);
        playerController.TakeDamage(damage);
    }

    void SkillHeal(float window)
    {
        currentHealth += GetDamageLastSeconds(window);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    /* =======================
     * Combos
     * ======================= */
    void Combo1()
    {
        ShootArrow();
        SkillHeal(1f);
    }

    void Combo2()
    {
        ShootArrow();
    }

    void Combo3()
    {
        SkillDamageReflect(5f);
        ShootArrow();
    }

    void Combo4()
    {
        SkillDamageReflect(5f);
        SkillHeal(1f);
    }

    /* =======================
     * Damage Handling
     * ======================= */
    public void TakeDamage(float damage)
    {
        if (Random.value < dodgeChance) return;

        currentHealth -= damage;
        RecordDamage(damage);
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
            Die();
    }

    void RecordDamage(float damage)
    {
        damageRecords.Add(new DamageRecord
        {
            damage = damage,
            time = Time.time
        });
    }

    public float GetDamageLastSeconds(float seconds)
    {
        float now = Time.time;

        damageRecords.RemoveAll(
            r => now - r.time > seconds
        );

        float sum = 0f;
        foreach (var r in damageRecords)
            sum += r.damage;

        return sum;
    }

    /* =======================
     * Death
     * ======================= */
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
     * Visual
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
}
