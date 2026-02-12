using UnityEngine;
using System.Collections;

public class Boss4 : MonoBehaviour, IDamageable
{
    /* =======================
     * Stats
     * ======================= */
    [Header("Stats")]
    public int maxHealth;
    public int attackDamage;
    public float speed;
    public float globalCooldown;
    public float dodgeChance;
    public int expDrop;

    private float currentHealth;
    private Animator anim;
    private Rigidbody2D rb;

    /* =======================
     * Player References
     * ======================= */
    private Transform playerTransform;
    private PlayerControler playerController;

    /* =======================
     * Distance 판단
     * ======================= */
    [Header("Ranges")]
    public float meleeRange = 1.8f;     // 근접 공격 기준 거리
    public float rangedMinRange = 2.2f; // 원거리 공격 최소 거리(너무 가까우면 쏘기 애매해서)
    public float faceDeadzone = 0.05f;

    [Header("Cooldowns")]
    public float poisonCooldown = 6f;
    public float spawnerCooldown = 8f;
    private float nextPoisonTime = 0f;
    private float nextSpawnerTime = 0f;


    /* =======================
     * Random Wander
     * ======================= */
    [Header("Wander")]
    public float wanderDurationMin = 0.5f;
    public float wanderDurationMax = 1.2f;
    public float wanderPauseMin = 0.1f;
    public float wanderPauseMax = 0.35f;
    public float wanderSpeedMultiplier = 0.7f;
    public float wanderRadius = 1.5f;   // 너무 멀리 튀지 않게

    private Vector2 spawnPos;

    /* =======================
     * Poison Cloud Prefab
     * ======================= */
    [Header("Poison Cloud Prefab")]
    public GameObject poisonCloudPrefab;
    public Transform poisonN, poisonS, poisonE, poisonW;
    public float poisonLifeTime = 5f;

    /* =======================
     * Spawner Skill
     * ======================= */
    [Header("Spawner")]
    public GameObject spawner;
    public float spawnerOffDelay = 5f;

    /* =======================
     * Skill Chances
     * ======================= */
    [Header("Skill Chances")]
    [Range(0f, 1f)] public float poisonChance = 0.25f;   // 사이클마다 독구름 시도 확률
    [Range(0f, 1f)] public float spawnerChance = 0.20f;  // 사이클마다 스포너 시도 확률

    /* =======================
     * Melee Attack
     * ======================= */
    [Header("Melee Attack")]
    public Vector2 boxCenter;
    public Vector2 boxSize = new Vector2(2f, 2f);
    public float angle = 0f;
    public LayerMask player;
    public float knockbackPower = 8f;

    /* =======================
     * Ranged Attack
     * ======================= */
    [Header("Ranged Attack")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    private Transform firePoint;

    /* =======================
     * State
     * ======================= */
    private enum State { Idle, Acting, Dead }
    private State currentState = State.Idle;
    private bool canAct = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;

        spawnPos = transform.position;

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

    IEnumerator AILoop()
    {
        while (currentState != State.Dead)
        {
            if (!canAct || playerTransform == null)
            {
                yield return null;
                continue;
            }

            currentState = State.Acting;

            // 1) 랜덤 이동
            yield return StartCoroutine(WanderRandom());

            // 2) 이번 턴에 할 "행동 하나" 결정
            float dist = Vector2.Distance(transform.position, playerTransform.position);

            bool canPoison = (Time.time >= nextPoisonTime) && (poisonCloudPrefab != null);
            bool canSpawner = (Time.time >= nextSpawnerTime) && (spawner != null);

            // 스킬 우선순위/확률 (원하는대로 조절 가능)
            bool doPoison = canPoison && (Random.value < poisonChance);
            bool doSpawner = !doPoison && canSpawner && (Random.value < spawnerChance);

            if (doPoison)
            {
                nextPoisonTime = Time.time + poisonCooldown;
                yield return StartCoroutine(DoPoisonCloud());
            }
            else if (doSpawner)
            {
                nextSpawnerTime = Time.time + spawnerCooldown;
                yield return StartCoroutine(DoSpawner());
            }
            else
            {
                // 스킬 안 하는 턴이면 공격만
                if (dist <= meleeRange)
                    yield return StartCoroutine(DoMelee());
                else
                    yield return StartCoroutine(DoRanged());
            }

            currentState = State.Idle;
            yield return new WaitForSeconds(globalCooldown);
        }
    }

    /* =======================
 * Wander (랜덤 이동)
 * ======================= */
    /* =======================
  * Wander (랜덤 이동)
  * ======================= */
    IEnumerator WanderRandom()
    {
        float moveTime = Random.Range(wanderDurationMin, wanderDurationMax);
        float pauseTime = Random.Range(wanderPauseMin, wanderPauseMax);

        // 랜덤 목적지(스폰 위치 주변 제한)
        Vector2 randDir = Random.insideUnitCircle.normalized;
        Vector2 target = (Vector2)transform.position + randDir * Random.Range(0.5f, wanderRadius);

        // 스폰 주변으로 너무 멀리 가는 거 방지
        Vector2 toSpawn = target - spawnPos;
        if (toSpawn.magnitude > wanderRadius)
            target = spawnPos + toSpawn.normalized * wanderRadius;

        float end = Time.time + moveTime;

        while (Time.time < end && currentState != State.Dead)
        {
            if (playerTransform == null) break;

            Vector2 pos = rb.position;
            Vector2 dir = (target - pos);

            if (dir.magnitude < 0.05f) break;

            Vector2 vel = dir.normalized * (speed * wanderSpeedMultiplier);

            // ⚠️ Unity 버전에 따라 linearVelocity가 없을 수 있음
            // 에러 나면 아래 두 줄 중 "velocity" 버전으로 바꿔줘
            rb.linearVelocity = vel;
            // rb.velocity = vel;

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        // rb.velocity = Vector2.zero;

        if (pauseTime > 0f)
            yield return new WaitForSeconds(pauseTime);
    }



    void StopMove()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    /* =======================
     * Attacks
     * ======================= */
    IEnumerator DoMelee()
    {
        StopMove();

        // (선택) 보스가 플레이어 방향으로 살짝 "확정"하고 치는 느낌
        FaceToPlayer();

        anim.SetTrigger("Attack1");

        // 타격 프레임에 맞춰 조절
        yield return new WaitForSeconds(0.25f);

        // 데미지/넉백
        if (playerController != null)
            playerController.TakeDamage(attackDamage);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle, player);
        foreach (Collider2D hit in hits)
        {
            Rigidbody2D hitRb = hit.GetComponent<Rigidbody2D>();
            if (hitRb == null) continue;

            Vector2 dir =
                ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;

            hitRb.linearVelocity = Vector2.zero;
            hitRb.AddForce(dir * knockbackPower, ForceMode2D.Impulse);
        }

        // 후딜
        yield return new WaitForSeconds(0.15f);
    }

    IEnumerator DoRanged()
    {
        StopMove();
        FaceToPlayer();

        anim.SetTrigger("Attack2");
        yield return new WaitForSeconds(0.15f);

        if (bulletPrefab != null && firePoint != null && playerTransform != null)
        {
            Vector2 dir =
                ((Vector2)playerTransform.position - (Vector2)firePoint.position).normalized;

            GameObject bullet =
                Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
            {
                b.speed = bulletSpeed;
                b.Init(dir);
            }
        }

        // 후딜
        yield return new WaitForSeconds(0.1f);
    }

    void FaceToPlayer()
    {
        if (playerTransform == null) return;

        // 2D 스프라이트 flip 예시 (너 프로젝트 방식에 맞게 수정)
        float dx = playerTransform.position.x - transform.position.x;
        if (Mathf.Abs(dx) < faceDeadzone) return;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (dx >= 0 ? 1f : -1f);
        transform.localScale = s;
    }

    /* =======================
     * Poison Cloud (프리팹 4방향)
     * ======================= */
    IEnumerator DoPoisonCloud()
    {
        StopMove();
        anim.SetTrigger("Skill2");

        yield return new WaitForSeconds(0.1f);

        SkillPoisonCloud4();
    }

    void SkillPoisonCloud4()
    {
        if (poisonCloudPrefab == null) return;

        SpawnCloud(poisonN);
        SpawnCloud(poisonS);
        SpawnCloud(poisonE);
        SpawnCloud(poisonW);
    }

    void SpawnCloud(Transform point)
    {
        if (point == null) return;

        GameObject cloud = Instantiate(poisonCloudPrefab, point.position, Quaternion.identity);

        PoisonCloud pc = cloud.GetComponent<PoisonCloud>();
        if (pc != null) pc.lifeTime = poisonLifeTime;
    }

    /* =======================
     * Spawner
     * ======================= */
    IEnumerator DoSpawner()
    {
        if (spawner.activeSelf) yield break;

        StopMove();

        // 애니 파라미터 이름 오타면 여기서 안 돌아가니 주의: "Skiil1" 그대로 유지
        anim.SetBool("Skiil1", true);
        spawner.SetActive(true);

        yield return new WaitForSeconds(spawnerOffDelay);

        anim.SetBool("Skiil1", false);
        spawner.SetActive(false);
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

        StopMove();

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

    /* =======================
     * Gizmos (근접 박스 확인용)
     * ======================= */
    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}
