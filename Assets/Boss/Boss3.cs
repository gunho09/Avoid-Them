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

    private readonly List<DamageRecord> damageRecords = new List<DamageRecord>();

    /* =======================
     * Stats
     * ======================= */
    [Header("Stats")]
    public int maxHealth ;
    public int attackDamage;
    public float speed;                 // (필요하면 나중에 이동에 사용)
    public float globalCooldown;      // 한 턴 끝나고 쉬는 시간
    public float dodgeChance;
    public int expDrop;

    private float currentHealth;
    [Header("Flip")]
    public bool facePlayer = true;       // 필요하면 끌 수 있게
    public bool flipWhenPlayerOnLeft = true; // 네 스프라이트 기본 방향에 맞춰 조절
    private SpriteRenderer sr;
    /* =======================
     * Player References
     * ======================= */
    private Transform playerTransform;
    private PlayerControler playerController;
    [Header("FirePoint Flip")]
    public bool flipFirePoint = true;
    public Vector2 firePointLocalRight = new Vector2(0.6f, 0.1f);
    public Vector2 firePointLocalLeft = new Vector2(-0.6f, 0.1f);

    /* =======================
     * State
     * ======================= */
    private enum State { Idle, Acting, Dead }
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

    private Animator anim;

    /* =======================
     * Skills
     * ======================= */
    [Header("Skill Windows")]
    public float reflectWindow = 5f;   // 최근 5초 데미지 반사
    public float healWindow = 1f;      // 최근 1초 데미지로 회복

    [Header("Skill Chances")]
    [Range(0f, 1f)] public float reflectChance = 0.25f;
    [Range(0f, 1f)] public float healChance = 0.20f;

    [Header("Skill Cooldowns")]
    public float reflectCooldown = 7f;
    public float healCooldown = 6f;
    private float nextReflectTime = 0f;
    private float nextHealTime = 0f;

    /* =======================
     * Init
     * ======================= */
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerController = playerObj.GetComponentInChildren<PlayerControler>();
        }

        firePoint = transform.Find("FirePoint");

        StartCoroutine(SpawnDelay());
    }

    IEnumerator SpawnDelay()
    {
        yield return new WaitForSeconds(0.5f);
        canAct = true;
        StartCoroutine(AILoop());
    }

    /* =======================
     * AI Loop
     * ======================= */
    IEnumerator AILoop()
    {
        while (currentState != State.Dead)
        {
            FaceToPlayer();

            if (!canAct || playerTransform == null || playerController == null)
            {
                yield return null;
                continue;
            }

            currentState = State.Acting;

            // 턴당 1행동만: (반사) or (힐) or (화살)
            bool canReflect = Time.time >= nextReflectTime;
            bool canHeal = Time.time >= nextHealTime;

            // 스킬 우선순위: 반사 먼저, 그 다음 힐, 아니면 공격
            bool doReflect = canReflect && (Random.value < reflectChance);
            bool doHeal = !doReflect && canHeal && (Random.value < healChance);

            if (doReflect)
            {
                nextReflectTime = Time.time + reflectCooldown;
                yield return StartCoroutine(DoSkillDamageReflect(reflectWindow));
            }
            else if (doHeal)
            {
                nextHealTime = Time.time + healCooldown;
                yield return StartCoroutine(DoSkillHeal(healWindow));
            }
            else
            {
                yield return StartCoroutine(DoShootArrow());
            }

            currentState = State.Idle;
            yield return new WaitForSeconds(globalCooldown);
        }
    }

    /* =======================
     * Actions
     * ======================= */
    IEnumerator DoShootArrow()
    {
        if (arrowPrefab == null || firePoint == null || playerTransform == null)
            yield break;

        FaceToPlayer();

        if (anim != null) anim.SetTrigger("Attack");

        // 애니메이션이 절반 정도 진행될 때까지 기다림 (예: 0.5초)
        // 이 시간을 조절해서 발사 타이밍을 맞추세요.
        yield return new WaitForSeconds(0.5f);

        // 여기서 dir을 선언합니다. (위에 다른 dir 선언이 없어야 함)
        Vector2 dir = ((Vector2)playerTransform.position - (Vector2)firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion rot = Quaternion.Euler(0, 0, angle);
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, rot);

        Allow arrowScript = arrow.GetComponent<Allow>();
        if (arrowScript != null)
        {
            arrowScript.speed = arrowSpeed;
            arrowScript.Init(dir);
        }
    }

    void FaceToPlayer()
    {
        if (!facePlayer) return;
        if (sr == null || playerTransform == null) return;

        float dx = playerTransform.position.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.01f) return;

        bool playerIsLeft = dx < 0f;

        sr.flipX = flipWhenPlayerOnLeft ? playerIsLeft : !playerIsLeft;

        if (flipFirePoint && firePoint != null)
        {
            firePoint.localPosition = sr.flipX ? firePointLocalLeft : firePointLocalRight;
        }
    }

    IEnumerator DoSkillDamageReflect(float window)
    {
        FaceToPlayer();

        // 스킬 애니
        if (anim != null) anim.SetTrigger("Skill1");

        // 연출 텀
        yield return new WaitForSeconds(0.1f);

        float damage = GetDamageLastSeconds(window);

        // 0이면 굳이 때리지 않게
        if (damage > 0f)
            playerController.TakeDamage(damage);

        // 후딜
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator DoSkillHeal(float window)
    {
        FaceToPlayer();

        if (anim != null) anim.SetTrigger("Skill2");

        yield return new WaitForSeconds(0.1f);

        float heal = GetDamageLastSeconds(window);
        if (heal > 0f)
        {
            currentHealth += heal;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        yield return new WaitForSeconds(0.1f);
    }

    /* =======================
     * Damage Handling
     * ======================= */
    public void TakeDamage(float damage)
    {
        if (currentState == State.Dead) return;
        if (Random.value < dodgeChance) return;

        currentHealth -= damage;

        RecordDamage(damage);
        GetComponent<HitFlashController>()?.Flash();

        if (currentHealth <= 0)
            Die();
    }

    void RecordDamage(float damage)
    {
        damageRecords.Add(new DamageRecord { damage = damage, time = Time.time });
    }

    public float GetDamageLastSeconds(float seconds)
    {
        float now = Time.time;

        damageRecords.RemoveAll(r => now - r.time > seconds);

        float sum = 0f;
        for (int i = 0; i < damageRecords.Count; i++)
            sum += damageRecords[i].damage;

        return sum;
    }

    /* =======================
     * Death
     * ======================= */
    void Die()
    {
        currentState = State.Dead;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        RoomControl room = GetComponentInParent<RoomControl>();
        if (room != null)
            room.OnEnemyKilled();

        if (playerController != null)
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
        if (sr != null)
        {
            Color origin = sr.color;
            sr.color = Color.gray;
            yield return new WaitForSeconds(0.1f);
            sr.color = origin;
        }
    }

}
