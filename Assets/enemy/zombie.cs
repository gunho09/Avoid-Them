using UnityEngine;
using System.Collections.Generic;

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
    private Pathfinding pathfinding;
    private List<Node> path;
    private int targetIndex;
    private float pathUpdateTimer;
    private const float pathupdateInterval = 0.3f;

    void Start()
    {
        currentHealth = health;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 
        {
            player = playerObj.transform;
        }
        else
        {
           Debug.LogError("Zombie Error: Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }

        pathfinding = FindFirstObjectByType<Pathfinding>();
        if (pathfinding == null)
        {
             Debug.LogError("Zombie Error: Scene에 'Pathfinding' 컴포넌트가 없습니다. GridManager 오브젝트에 Pathfinding 스크립트를 추가했는지 확인해주세요.");
        }
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
            transform.position += dir * speed * Time.deltaTime;
            if (Vector3.Distance(transform.position, targetPosition) < 0.4f)
            {
                targetIndex++;
            }
        }
        else
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * (speed * 0.5f) * Time.deltaTime;
        }
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
        
       
        RoomControl room = GetComponentInParent<RoomControl>();
        if (room != null)
        {
            room.OnEnemyKilled();
        }

        Destroy(gameObject, 2f); 
    }
}
