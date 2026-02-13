using UnityEngine;
using System.Collections;

public class SecondBoss : MonoBehaviour, IDamageable
{
    public enum State { Idle, Move, Attack, Skill1, Skill2, Die }
    public State currentState = State.Idle;

    public float maxHp = 4000f;
    public float hp;
    public float damage = 60f;
    public float moveSpeed = 1.5f;
    public float attackRange = 2.5f;
    public float attackCooldown = 2.0f;
    public int expDrop = 80;

    public float magneticDuration = 5.0f;
    public float damageReductionPercent = 0.5f;
    public float magneticCooldown = 12.0f;
    private bool isMagneticActive = false;

    public float gasRadius = 5.0f;
    public float gasDamage = 10f;
    public float gasDuration = 4.0f;
    public float gasCooldown = 8.0f;

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerControler player;
    
    private float lastAttackTime;
    private float lastMagneticTime;
    private float lastGasTime;
    private bool isDead = false;

    public ParticleSystem gasParticle;

    void Start()
    {
        hp = maxHp;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        if (rb != null) {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }

        FindPlayer();
        lastMagneticTime = Time.time;
        lastGasTime = Time.time;
    }

    void Update()
    {
        if (isDead || player == null) {
            FindPlayer();
            return;
        }

        switch (currentState)
        {
            case State.Idle:
                CheckNextAction();
                anim?.SetBool("isMoving", false);
                break;
            case State.Move:
                HandleMove();
                anim?.SetBool("isMoving", true);
                break;
        }
    }

    void CheckNextAction()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (Time.time >= lastMagneticTime + magneticCooldown && hp < maxHp * 0.8f)
        {
            StartCoroutine(MagneticFieldRoutine());
            return;
        }

        if (Time.time >= lastGasTime + gasCooldown && distance <= gasRadius)
        {
            StartCoroutine(PoisonGasRoutine());
            return;
        }

        if (distance <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
                StartCoroutine(StabAttackRoutine());
            else
                currentState = State.Idle;
        }
        else
        {
            currentState = State.Move;
        }
    }

    void HandleMove()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= attackRange * 0.8f) {
            currentState = State.Idle;
            return;
        }

        Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);


        transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);
    }


    IEnumerator StabAttackRoutine()
    {
        currentState = State.Attack;
        lastAttackTime = Time.time;

        anim?.SetTrigger("Attack");
        yield return new WaitForSeconds(0.4f);

        if (Vector2.Distance(transform.position, player.transform.position) <= attackRange)
        {
            player.TakeDamage(damage);
        }

        yield return new WaitForSeconds(0.6f);
        currentState = State.Idle;
    }


    IEnumerator MagneticFieldRoutine()
    {
        currentState = State.Skill1;
        lastMagneticTime = Time.time;
        isMagneticActive = true;

        anim?.SetBool("isMagnetic", true); 

        yield return new WaitForSeconds(magneticDuration);

        anim?.SetBool("isMagnetic", false); 
        
        isMagneticActive = false;
        currentState = State.Idle;
    }
    IEnumerator PoisonGasRoutine()
    {
        currentState = State.Skill2;
        lastGasTime = Time.time;

        if (gasParticle) gasParticle.Play();
        anim?.SetTrigger("Skill2");

        float timer = 0;
        float damageInterval = 0.5f;
        float damagePerTick = 10f;

        while (timer < gasDuration)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            
            if (dist <= gasRadius)
            {
                player.TakeDamage(damagePerTick);
            }


            yield return new WaitForSeconds(damageInterval);
            timer += damageInterval;
        }

        if (gasParticle) gasParticle.Stop();
        currentState = State.Idle;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        float finalDamage = isMagneticActive ? amount * (1f - damageReductionPercent) : amount;
        hp -= finalDamage;
        GetComponent<HitFlashController>()?.Flash();

        if (hp <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        currentState = State.Die;

        
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 2.0f);
    }

    void FindPlayer()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj) player = obj.GetComponent<PlayerControler>();
    }

    public float GetHpRatio()
    {
        if (maxHp <= 0) return 1f;
        return hp / maxHp;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, gasRadius);
    }
}