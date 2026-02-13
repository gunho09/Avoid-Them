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

    [Header("Ranges")]
    public float meleeRange = 1.8f;
    public float rangedMinRange = 2.2f;
    public float faceDeadzone = 0.05f;

    [Header("Cooldowns")]
    public float poisonCooldown = 6f;
    public float spawnerCooldown = 8f;
    private float nextPoisonTime = 0f;
    private float nextSpawnerTime = 0f;

    [Header("Wander")]
    public float wanderDurationMin = 0.5f;
    public float wanderDurationMax = 1.2f;
    public float wanderPauseMin = 0.1f;
    public float wanderPauseMax = 0.35f;
    public float wanderSpeedMultiplier = 0.7f;
    public float wanderRadius = 1.5f;

    private Vector2 spawnPos;

    [Header("Poison Cloud Prefab")]
    public GameObject poisonCloudPrefab;
    public Transform poisonN, poisonS, poisonE, poisonW;
    public float poisonLifeTime = 5f;

    [Header("Spawner")]
    public GameObject spawner;
    public float spawnerOffDelay = 5f;

    [Header("Skill Chances")]
    [Range(0f, 1f)] public float poisonChance = 0.25f;
    [Range(0f, 1f)] public float spawnerChance = 0.20f;

    [Header("Melee Attack")]
    public Vector2 boxCenter;
    public Vector2 boxSize = new Vector2(2f, 2f);
    public float angle = 0f;
    public LayerMask player;
    public float knockbackPower = 8f;

    [Header("Ranged Attack")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    private Transform firePoint;

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

            // 1) 랜덤 이동
            yield return StartCoroutine(WanderRandom());

            currentState = State.Acting;

            // 2) 거리 및 쿨타임 계산
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            bool canPoison = (Time.time >= nextPoisonTime) && (poisonCloudPrefab != null);
            bool canSpawner = (Time.time >= nextSpawnerTime) && (spawner != null);

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
                if (dist <= meleeRange)
                    yield return StartCoroutine(DoMelee());
                else
                    yield return StartCoroutine(DoRanged());
            }

            currentState = State.Idle;
            yield return new WaitForSeconds(globalCooldown);
        }
    }

    IEnumerator WanderRandom()
    {
        float moveTime = Random.Range(wanderDurationMin, wanderDurationMax);
        float pauseTime = Random.Range(wanderPauseMin, wanderPauseMax);

        Vector2 randDir = Random.insideUnitCircle.normalized;
        Vector2 target = (Vector2)transform.position + randDir * Random.Range(0.5f, wanderRadius);

        Vector2 toSpawn = target - spawnPos;
        if (toSpawn.magnitude > wanderRadius)
            target = spawnPos + toSpawn.normalized * wanderRadius;

        float end = Time.time + moveTime;
        while (Time.time < end && currentState != State.Dead)
        {
            if (playerTransform == null) break;
            Vector2 dir = (target - rb.position);
            if (dir.magnitude < 0.05f) break;

            rb.linearVelocity = dir.normalized * (speed * wanderSpeedMultiplier);
            yield return null;
        }

        StopMove();
        if (pauseTime > 0f) yield return new WaitForSeconds(pauseTime);
    }

    void StopMove()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    /* ============================================================
     * ACTION COROUTINES (애니메이션 트리거 전송)
     * ============================================================ */

    IEnumerator DoMelee()
    {
        StopMove();
        FaceToPlayer();
        anim.SetTrigger("Attack1");
        yield return new WaitForSeconds(0.6f); // 애니메이션이 끝날 때까지 대기용
    }

    IEnumerator DoRanged()
    {
        StopMove();
        FaceToPlayer();
        anim.SetTrigger("Attack2");
        yield return new WaitForSeconds(0.6f);
    }

    IEnumerator DoPoisonCloud()
    {
        StopMove();
        anim.SetTrigger("Skill2");
        yield return new WaitForSeconds(0.8f);
    }

    IEnumerator DoSpawner()
    {
        if (spawner.activeSelf) yield break;
        StopMove();
        anim.SetBool("Skiil1", true);
        spawner.SetActive(true);
        yield return new WaitForSeconds(spawnerOffDelay);
        anim.SetBool("Skiil1", false);
        spawner.SetActive(false);
    }

    /* ============================================================
     * ANIMATION EVENTS (유니티 애니메이션 창에서 연결할 함수들)
     * ============================================================ */

    public void OnMeleeHit() // Attack1 애니메이션의 타격 프레임에 추가
    {
        if (playerController != null)
        {
            // 오버랩 박스 체크 (보스 위치 + 오프셋 기준)
            Vector2 worldBoxCenter = (Vector2)transform.position + boxCenter;
            Collider2D[] hits = Physics2D.OverlapBoxAll(worldBoxCenter, boxSize, angle, player);

            foreach (Collider2D hit in hits)
            {
                IDamageable dmg = hit.GetComponentInParent<IDamageable>();
                dmg?.TakeDamage(attackDamage);

                Rigidbody2D hitRb = hit.GetComponent<Rigidbody2D>();
                if (hitRb != null)
                {
                    Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                    hitRb.linearVelocity = Vector2.zero;
                    hitRb.AddForce(dir * knockbackPower, ForceMode2D.Impulse);
                }
            }
        }
    }

    public void OnRangedShoot() // Attack2 애니메이션의 발사 프레임에 추가
    {
        if (bulletPrefab != null && firePoint != null && playerTransform != null)
        {
            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)firePoint.position).normalized;
            float rotAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0, 0, rotAngle);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rot);
            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
            {
                b.speed = bulletSpeed;
                b.Init(dir);
            }
        }
    }

    public void OnPoisonSpawn() // Skill2 애니메이션의 바닥 찍는 프레임에 추가
    {
        SkillPoisonCloud4();
    }

    /* =======================
     * Utility
     * ======================= */

    void FaceToPlayer()
    {
        if (playerTransform == null) return;
        float dx = playerTransform.position.x - transform.position.x;
        if (Mathf.Abs(dx) < faceDeadzone) return;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (dx >= 0 ? 1f : -1f);
        transform.localScale = s;
    }

    void SkillPoisonCloud4()
    {
        if (poisonCloudPrefab == null) return;
        SpawnCloud(poisonN); SpawnCloud(poisonS); SpawnCloud(poisonE); SpawnCloud(poisonW);
    }

    void SpawnCloud(Transform point)
    {
        if (point == null) return;
        GameObject cloud = Instantiate(poisonCloudPrefab, point.position, Quaternion.identity);
        PoisonCloud pc = cloud.GetComponent<PoisonCloud>();
        if (pc != null) pc.lifeTime = poisonLifeTime;
    }

    public void TakeDamage(float damage)
    {
        if (Random.value < dodgeChance) return;
        currentHealth -= damage;
        GetComponent<HitFlashController>()?.Flash();
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        currentState = State.Dead;
        StopMove();
        GetComponentInParent<RoomControl>()?.OnEnemyKilled();
        playerController?.TakeExp(expDrop);
        Destroy(gameObject, 1f);
    }

    public float GetHpRatio() => currentHealth / maxHealth;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 worldBoxCenter = (Vector2)transform.position + boxCenter;
        Gizmos.matrix = Matrix4x4.TRS(worldBoxCenter, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}