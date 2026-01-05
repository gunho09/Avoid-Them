using System.Diagnostics;
using UnityEngine;

public class PlayerControler : MonoBehaviour, IDamageable
{
    // 기본 스탯
    public float playerSpeed = 5f;
    public float playerLevel = 1f;
    public float plusHp = 1f;
    public float plusPW = 1f;
    public float numHp = 1f;
    public float numPW = 1f;
    public float exp = 0;
    public float playerStartHp = 100;
    public float playerStartPw = 30;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public LayerMask enemy;

    // 계산된 스탯
    public float PlayerMaxHp;
    public float PlayerDamage;
    public float PlayerCurrentHp;

    // 가드
    private bool Guarding = false;

    // 대쉬
    public float dashCooldown = 1f;    
    public float dashSpeed = 30f;
    public float dashDuration = 1f;


    private bool isDashing = false;
    private float dashTimer = 0f;
    private float cooldownTimer = 0f;
    private Vector3 dashDirection;

    // 물리 이동을 위한 컴포넌트 추가
    private Rigidbody2D rb;
    private Vector2 inputMovement;

    void Start()
    {
        PlayerMaxHp = plusHp + numHp + playerStartHp;
        PlayerDamage = plusPW + numPW + playerStartPw;
        PlayerCurrentHp = PlayerMaxHp;

        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        // 쿨타임 감소
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // 대쉬 중일 때
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
            {
                isDashing = false;
            }
            return;
        }

        // 일반 이동 입력 읽기
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        inputMovement = new Vector2(moveX, moveY);

        // 입력 처리
        if (Input.GetMouseButtonDown(0)) Attack();

        if (Input.GetMouseButtonDown(1))
        {
            Guarding = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            Guarding = false;
        }

        if (Input.GetKeyDown(KeyCode.E)) RightHook();

        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownTimer <= 0)
        {
            // Dash 호출 시 inputMovement 사용 (movement 대신)
            Dash(new Vector3(moveX, moveY, 0));
        }

        if (Input.GetKeyDown(KeyCode.Q)) Boost();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
        }
        else
        {
            rb.linearVelocity = inputMovement * playerSpeed;
        }
    }

    public void Attack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemy);
        foreach (Collider hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
        }
    }

    public int Guard()
    {
        return Guarding ? 80 : 1;
    }

    public void RightHook()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemy);
        foreach (Collider hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage * 2);
        }
    }

    public void Dash(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        isDashing = true;
        dashTimer = dashDuration; 
        cooldownTimer = dashCooldown;
        dashDirection = direction.normalized;
                                                                                                                                           
        UnityEngine.Debug.Log("대쉬!");
    }

    public void Boost()
    {
        // 나중에 구현
    }

    public void TakeDamage(float damage)
    {
        PlayerCurrentHp -= damage / Guard();
        UnityEngine.Debug.Log("플레이어 공격 받음!");

        if (PlayerCurrentHp <= 0)
        {
            PlayerCurrentHp = 0;
            Die();
        }
    }

    void Die()
    {

        //deathUI.SetActive(true);

        //Time.timeScale = 0f;

        //gameObject.SetActive(false);

        UnityEngine.Debug.Log("플레이어 죽었다~!");

    }
}