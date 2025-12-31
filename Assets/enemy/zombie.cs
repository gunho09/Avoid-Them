using UnityEngine;

public class zombie : MonoBehaviour
{
    [SerializeField]private int health;
    [SerializeField]private int attackDamage;
    [SerializeField]private float speed;
    [SerializeField]private float atackSpeed;
    [SerializeField]private float dodgeChance;
    [SerializeField]private int expDrop;
    [SerializeField]private float range;
    [SerializeField]private float attackRange;

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
            //플레이어 때찌
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(int damage)
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
