using UnityEngine;
using System.Collections.Generic;

public class zombie : MonoBehaviour
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
    private Pathfinding pathfinding;
    private List<Node> path;
    private int targetIndex;
    private float pathUpdateTimer;
    private const float pathupdateInterval = 0.3f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = health;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerControler = playerObj.GetComponent<PlayerControler>();
        }
        pathfinding = FindFirstObjectByType<Pathfinding>();
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
                rb.linearVelocity = Vector2.zero;
                break;
            case State.Chase:
                ChasePlayer();
                break;
            case State.Attack:
                rb.linearVelocity = Vector2.zero;
                AttackPlayer();
                break;
        }
    }

    void ChasePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attack;
            return;
        }
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathupdateInterval)
        {
            pathUpdateTimer = 0f;
            if (pathfinding != null && player != null)
            {
                path = pathfinding.FindPath(transform.position, player.position);
                targetIndex = 0;
            }
        }

        if (path != null && targetIndex < path.Count)
        {
            Vector3 targetPosition = path[targetIndex].worldPosition;
            targetPosition.z = transform.position.z;
            Vector3 dir = (targetPosition - transform.position).normalized;
            rb.linearVelocity = dir * speed;
            if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
            {
                targetIndex++;
            }
        }
        else if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * (speed * 0.5f);
        }
    }

    void AttackPlayer()
    {
        if (Time.time - lastAttackTime >= atackSpeed)
        {
            //playerControler.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log(damage);
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
        rb.linearVelocity = Vector2.zero;
        RoomControl room = GetComponentInParent<RoomControl>();
        if (room != null)
        {
            room.OnEnemyKilled();
        }

        Destroy(gameObject, 2f); 

        playerControler.TakeExp(expDrop);
    }
}
