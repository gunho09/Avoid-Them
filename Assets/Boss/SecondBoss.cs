using UnityEngine;
using UnityEngine.UI;
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

    public Slider HpBar;
    public GameObject itemDropPrefab; // [New] 보성 드랍 프리팹

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

    private bool isAttacking = false;

    public ParticleSystem gasParticle;

    void Start()
    {
        hp = maxHp;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (HpBar != null)
        {
            HpBar.maxValue = maxHp;
            HpBar.value = hp;
        }
        
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
        if (HpBar != null)
        {
            HpBar.value = hp;
        }

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

        if (isDead || player == null || isAttacking) return;
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
        isAttacking = true; // 공격 시작!
        currentState = State.Attack;
        lastAttackTime = Time.time;

        anim?.SetTrigger("Attack");
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("3-3");
        
        yield return new WaitForSeconds(0.4f);

        if (player != null && Vector2.Distance(transform.position, player.transform.position) <= attackRange)
        {
            player.TakeDamage(damage);
        }

        yield return new WaitForSeconds(0.6f);
        
        isAttacking = false;
        currentState = State.Idle;
    }

    IEnumerator MagneticFieldRoutine()
    {
        currentState = State.Skill1;
        lastMagneticTime = Time.time;
        isMagneticActive = true;
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("3-4");
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
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("3-5");
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

        if (hp <= 0)
        {
            StopAllCoroutines();
            Die();
        }
    }

    void Die()
    {
        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        isDead = true;
        currentState = State.Die;

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-11");
        GetComponent<Collider2D>().enabled = false;

        // [New] 보스 전용 아이템 하나 드랍
        if (ItemDatabase.Instance != null && itemDropPrefab != null)
        {
            ItemData bossItem = ItemDatabase.Instance.GetRandomBossItem();
            if (bossItem != null)
            {
                GameObject drop = Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
                ItemPickup pickup = drop.GetComponent<ItemPickup>();
                if (pickup != null) pickup.Setup(bossItem, null);
            }
        }

        yield return new WaitForSeconds(2.0f);

        if (MapManager.Instance != null)
        {
            MapManager.Instance.isBossDead = true;
        }

        Destroy(gameObject);
    }

    void FindPlayer()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) 
        {
            player = obj.GetComponent<PlayerControler>();
            if (player == null) player = obj.GetComponentInChildren<PlayerControler>();
        }
    }

    public float GetHpRatio()
    {
        return maxHp <= 0 ? 1f : hp / maxHp;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, gasRadius);
    }
}