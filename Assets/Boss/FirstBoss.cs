using UnityEngine;
using System.Collections;

public class FirstBoss : MonoBehaviour, IDamageable
{
    public enum State { Idle, Move, Attack, Skill, Die }
    public State currentState = State.Idle;
    public float hp = 1000f;
    public float damage = 100f;
    public float moveSpeed = 3.0f;
    public float attackRange = 2.0f;
    public float attackCooldown = 3.0f;
    public int expDrop = 50;
    public float zonnahitDuration = 3.0f;
    public float zonnahitDamageInterval = 0.3f;
    public float zonnahitRadius = 4.0f;
    public float skillCooldown = 10.0f;
    public ParticleSystem punchVFX;
    public ParticleSystem zonnahitVFX;

    private Rigidbody2D rb;
    private float lastAttackTime;
    private float lastSkillTime;
    private bool isDead = false;
    private PlayerControler playerControler;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null) {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
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
        if (isDead) return;

        if (playerControler == null)
        {
            FindPlayerAutomatically();
            return; 
        }

        switch (currentState)
        {
            case State.Idle:   CheckNextAction(); break;
            case State.Move:   HandleMove();      break;
            case State.Attack: break;
            case State.Skill:  break;
            case State.Die:    break;
        }
    }

    void CheckNextAction()
    {
        float distance = Vector2.Distance(transform.position, playerControler.transform.position);

        if (Time.time >= lastSkillTime + skillCooldown)
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
    }

    IEnumerator BasicAttackRoutine()
    {
        currentState = State.Attack;
        lastAttackTime = Time.time;
        
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("3-1"); // 1층 보스 기본 공격
        if (punchVFX != null) punchVFX.Play();

        if (Vector2.Distance(transform.position, playerControler.transform.position) <= attackRange)
        {
            playerControler.TakeDamage(damage);
        }

        yield return new WaitForSeconds(1.0f);
        currentState = State.Idle;
    }

    IEnumerator zonnahitRoutine()
    {
        Debug.Log("Zonnahit Skill Activated");
        currentState = State.Skill;
        lastSkillTime = Time.time;

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("3-2"); // 1층 보스 난타
        if (zonnahitVFX != null) zonnahitVFX.Play();

        float timer = 0;
        while (timer < zonnahitDuration)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, zonnahitRadius, LayerMask.GetMask("Player"));

            if (hit != null)
            {
                playerControler.TakeDamage(damage / 5);
            }
            timer += zonnahitDamageInterval;
            yield return new WaitForSeconds(zonnahitDamageInterval);
        }

        if (zonnahitVFX != null) zonnahitVFX.Stop();
        currentState = State.Idle;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        hp -= amount;
        if (hp <= 0) StartCoroutine(DieRoutine());
    }

    public float GetHpRatio()
    {
        if (hp <= 0) return 0f;
        return hp / 1000f;
    }

    IEnumerator DieRoutine()
    {
        isDead = true;
        currentState = State.Die;
        
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-11"); // 보스 사망

        Collider2D col = GetComponent<Collider2D>();
        if(col != null) col.enabled = false;

        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, zonnahitRadius);
    }
}