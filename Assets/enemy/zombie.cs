using UnityEngine;

public class zombie : MonoBehaviour, IDamageable
{
    public PlayerControler playerControler;
    public int health;
    public int attackDamage;
    public float speed;
    public float atackSpeed;
    public float dodgeChance;
    public int expDrop;
    public float range;
    public float attackRange;

    private float currentHealth;
    private float lastAttackTime;
    private Transform player;
    private enum State { Idle, Chase, Attack, Dead }
    private State currentState = State.Idle;

    void Start()
    {
        currentHealth = health;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (currentState == State.Dead)
            return;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attack;
        }
        
        else if (currentState == State.Attack && distanceToPlayer <= attackRange + 0.2f)
        {
            currentState = State.Attack;
        }
        else if (distanceToPlayer <= range)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Idle;
        }

        switch (currentState)
        {
            case State.Idle:
                //애니메이션
                break;
            case State.Chase:
                ChasePlayer();
                break;
            case State.Attack:
                AttackPlayer();
                break;
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    void AttackPlayer()
    {
        if (Time.time - lastAttackTime >= atackSpeed)
        {
            // playerControler.TakeDamage(attackDamage);
            player.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float damage)
    {
        if (Random.value < dodgeChance)
        {
            return;
        }
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        currentState = State.Dead;
        
       
        RoomControl room = GetComponentInParent<RoomControl>();
        if (room != null)
        {
            room.OnEnemyKilled();
        }

        Destroy(gameObject, 2f); 
    }
}
