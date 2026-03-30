using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FirstBoss : MonoBehaviour, IDamageable
{
    public enum State { Idle, Move, Attack, Skill, Die }
    public State currentState = State.Idle;

    public float maxHp = 1500f;
    public float hp;
    public float damage = 40f;
    public float moveSpeed = 2.0f;
    public float attackRange = 5.0f;
    public float attackCooldown = 3.0f;
    public int expDrop = 50;

    public float zonnahitDuration = 3.0f;
    public float zonnahitDamageInterval = 0.1f;
    public float zonnahitRadius = 4.0f;
    public float skillCooldown = 10.0f;

    public GameObject WindPunchPrefab;
    public ParticleSystem zonnahitParticle;

    private CameraFollow cameraFollow;
    public GameObject bloodVFXPrefab;
    public Slider HpBar;

    private Rigidbody2D rb;
    private float lastAttackTime;
    private float lastSkillTime;
    private bool isDead = false;
    private PlayerControler playerControler;
    private Animator anim;

    [Header("Hit Flash")]
    public float hitFlashDuration = 0.1f;
    public Color hitFlashColor = Color.gray;

    private SpriteRenderer hitSr;
    private Coroutine hitFlashCo;
    private LineRenderer lineRenderer;
    public int segmentCount = 50;
    public float warningDuration = 1.0f;

    void Start()
    {
        hp = maxHp;
        rb = GetComponent<Rigidbody2D>();
        hitSr = GetComponentInChildren<SpriteRenderer>();
        
        // 노란 줄(경고) 해결 및 중복 할당 제거
        cameraFollow = Object.FindAnyObjectByType<CameraFollow>();

        if (HpBar != null)
        {
            HpBar.maxValue = maxHp;
            HpBar.value = hp;
        }

        if (rb != null) 
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            // 클론 소환 시 물리 엔진이 잠드는 현상 방지
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep; 
        }

        anim = GetComponent<Animator>();

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = segmentCount;
            lineRenderer.enabled = false;
        }

        FindPlayerAutomatically();
        lastSkillTime = Time.time;
    }

    void FindPlayerAutomatically()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerControler = playerObj.GetComponent<PlayerControler>();
            if (playerControler == null) playerControler = playerObj.GetComponentInChildren<PlayerControler>();
            if (playerControler == null) playerControler = playerObj.GetComponentInParent<PlayerControler>();
        }
    }

    void Update()
    {
        if (HpBar != null) HpBar.value = hp;

        if (isDead) return;

        // [핵심 해결 포인트]
        // 클론 소환 시 플레이어를 아직 못 찾았더라도 애니메이션은 무조건 갱신되도록 순서를 바꿨습니다.
        UpdateAnimation(); 

        if (playerControler == null)
        {
            FindPlayerAutomatically();
            return; 
        }

        switch (currentState)
        {
            case State.Idle:
                CheckNextAction();
                break;
            case State.Move:
                HandleMove();
                break;
            case State.Attack:
            case State.Skill:
                break;
        }
    }

    void UpdateAnimation()
    {
        if (anim == null) return;

        // 현재 상태가 Move 이거나, 물리적으로 밀리는 속도가 있으면 무조건 true
        bool isMovingState = (currentState == State.Move);
        bool isPhysicsMoving = (rb != null && rb.linearVelocity.magnitude > 0.1f);

        anim.SetBool("isMoving", isMovingState || isPhysicsMoving);
    }

    void DrawPolygon(int segments, float radius)
    {
        lineRenderer.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0) + transform.position);
        }
    }

    void CheckNextAction()
    {
        float distance = Vector2.Distance(transform.position, playerControler.transform.position);

        if (Time.time >= lastSkillTime + skillCooldown && distance <= zonnahitRadius + 1.0f)
        {
            StartCoroutine(zonnahitRoutine());
            return;
        }

        if (distance <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(BasicAttackRoutine());
            }
            else
            {
                currentState = State.Idle;
            }
        }
        else
        {
            currentState = State.Move;
        }
    }

    void HandleMove()
    {
        if (playerControler == null) return;

        float distance = Vector2.Distance(transform.position, playerControler.transform.position);
        
        if (distance <= attackRange)
        {
            currentState = State.Idle;
            return;
        }
        
        Vector2 direction = ((Vector2)playerControler.transform.position - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);

        if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    IEnumerator BasicAttackRoutine()
    {
        currentState = State.Attack;
        lastAttackTime = Time.time;

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("3-1"); 
        if (anim != null) anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.7f);

        cameraFollow?.Shake(0.3f, 0.4f);

        Vector2 attackPos = (Vector2)transform.position + ((Vector2)playerControler.transform.position - (Vector2)transform.position).normalized * 1.0f;
        
        Collider2D hit = Physics2D.OverlapCircle(attackPos, 1.5f, LayerMask.GetMask("player"));

        if (hit != null && playerControler != null)
        {
            playerControler.TakeDamage(damage);
        }

        yield return new WaitForSeconds(1.0f);
        currentState = State.Idle;
    }

    IEnumerator zonnahitRoutine()
    {
        currentState = State.Skill;
        lastSkillTime = Time.time;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            float t = 0;
            while (t < warningDuration)
            {
                t += Time.deltaTime;
                float progress = t / warningDuration;

                DrawPolygon(segmentCount, zonnahitRadius);

                Color color = Color.Lerp(new Color(1, 0, 0, 0), new Color(1, 0, 0, 1), progress);
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;

                yield return null;
            }
        }

        if (anim != null) anim.SetTrigger("Skill");
        if (SoundManager.Instance != null)
        {
            StartCoroutine(PlayFlurrySounds());
        }
        if (zonnahitParticle != null) zonnahitParticle.Play();

        if (lineRenderer != null) lineRenderer.enabled = false;

        float timer = 0;

        while (timer < zonnahitDuration)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, zonnahitRadius, LayerMask.GetMask("player"));

            if (hit != null && playerControler != null)
            {
                playerControler.TakeDamage(damage * 0.7f);
            }

            cameraFollow?.Shake(0.1f, 0.1f);

            yield return new WaitForSeconds(zonnahitDamageInterval);
            timer += zonnahitDamageInterval;
        }

        if (zonnahitParticle != null) zonnahitParticle.Stop();
        
        currentState = State.Idle;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        hp -= amount;

        if (hitFlashCo != null) StopCoroutine(hitFlashCo);
        hitFlashCo = StartCoroutine(HitFlash());

        if (hp <= 0) 
        {
            StopAllCoroutines();
            if (lineRenderer != null) lineRenderer.enabled = false;
            if (zonnahitParticle != null) zonnahitParticle.Stop();
            StartCoroutine(DieRoutine());
        }
    }

    IEnumerator PlayFlurrySounds()
    {
        for (int i = 0; i < 3; i++)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("3-2");
            yield return new WaitForSeconds(0.1f);
        }
    }

    public float GetHpRatio()
    {
        if (maxHp <= 0) return 1f;
        return hp / maxHp;
    }

    IEnumerator DieRoutine()
    {
        isDead = true;
        currentState = State.Die;

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-11");

        GameObject vfx = Instantiate(bloodVFXPrefab, transform.position, Quaternion.identity);
        Destroy(vfx, 2.0f);

        Collider2D col = GetComponent<Collider2D>();
        if(col != null) col.enabled = false;

        if (zonnahitParticle != null) zonnahitParticle.Stop();

        yield return new WaitForSeconds(2.0f);
        
        if (MapManager.Instance != null)
        {
            MapManager.Instance.isBossDead = true;
        }

        Destroy(gameObject);
    }

    IEnumerator HitFlash()
    {
        if (hitSr == null) yield break;

        Color origin = hitSr.color;
        hitSr.color = hitFlashColor;

        yield return new WaitForSeconds(hitFlashDuration);

        if (hitSr != null) hitSr.color = origin;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, zonnahitRadius);
    }
}