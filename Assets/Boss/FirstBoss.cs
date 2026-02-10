using UnityEngine;
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

    private Rigidbody2D rb;
    private float lastAttackTime;
    private float lastSkillTime;
    private bool isDead = false;
    private PlayerControler playerControler;

    void Start()
    {
        hp = maxHp;
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
    }

    IEnumerator BasicAttackRoutine()
    {
        currentState = State.Attack;
        lastAttackTime = Time.time;
        
        yield return new WaitForSeconds(0.5f); 

        if (WindPunchPrefab != null)
        {
            GameObject projObj = Instantiate(WindPunchPrefab, transform.position, Quaternion.identity);
            WindPunch projectile = projObj.GetComponent<WindPunch>();
            
            if (projectile != null && playerControler != null)
            {
                Vector2 dir = (playerControler.transform.position - transform.position).normalized;
                projectile.Setup(dir, damage);
            }
        }

        yield return new WaitForSeconds(1.0f);
        currentState = State.Idle;
    }

    IEnumerator zonnahitRoutine()
    {
        currentState = State.Skill;
        lastSkillTime = Time.time;

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("3-2");
        if (zonnahitParticle != null) zonnahitParticle.Play();

        float timer = 0;

        while (timer < zonnahitDuration)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, zonnahitRadius, LayerMask.GetMask("player"));

            if (hit != null)
            {
                if (playerControler != null) 
                {
                    playerControler.TakeDamage(damage * 0.5f);
                }
            }

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
        if (hp <= 0) StartCoroutine(DieRoutine());
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

        Collider2D col = GetComponent<Collider2D>();
        if(col != null) col.enabled = false;

        if (zonnahitParticle != null) zonnahitParticle.Stop();

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