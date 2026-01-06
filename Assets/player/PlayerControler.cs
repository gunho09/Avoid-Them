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
    public float attackDistance = 5.0f;
    public float attackWidth = 0.5f;
    public float attackRange = 1.5f;
    public float attackDamage = 30f;
    public LayerMask enemy;

    // 계산된 스탯
    public float PlayerMaxHp;
    public float PlayerDamage;
    public float PlayerCurrentHp;

    // 가드
    private bool Guarding = false;

    // 대쉬
    public float dashCooldown = 2f;
    public float dashSpeed = 30f;
    public float dashDuration = 1f;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float cooldownTimerDashDash = 0f;
    private Vector3 dashDirection;

    //죽었을 때 UI
    public GameObject deathUI;

    //금강불괴
    private float cooldownTimerBoost = 0f;
    private bool isBoost = false;
    public float boostCooldown = 30f;
    public float boostTimer = 0f;
    public float boostDuration = 0f;

    //레프트훅
    private float cooldownTimerHook = 0f;
    private bool isHook = false;
    public float hookCooldown = 30f;
    public float hookTimer = 0f;
    public float hookDuration = 0f;

    public Slider hpSlider;


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

        if(hpSlider != null)
        {
            hpSlider.maxValue = PlayerMaxHp;
            hpSlider.value = PlayerCurrentHp;
        }
    }

    void Update()
    {

        CoolDownMananger();

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

        if (Input.GetKeyDown(KeyCode.E)) LeftHook();

        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownTimerDashDash <= 0)
        {
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

        Vector2 boxCenter = (Vector2)transform.position + (Vector2)transform.right * (attackDistance / 2);
        Vector2 boxSize = new Vector2(attackWidth, attackDistance);
        float angle = transform.eulerAngles.z;
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle, enemy);
        foreach (Collider2D hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        // 1. Z값을 0으로 고정해서 플레이어와 같은 평면에 있게 합니다.
        Vector3 boxCenter = transform.position + transform.right * (attackDistance / 2);

        // 2. Gizmos.matrix를 설정하기 전에 현재 matrix를 저장해두는 것이 좋습니다.
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // 3. TRS 설정 (위치, 회전, 스케일)
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);

        // 4. Z축 두께를 1f 정도로 줘서 겹치더라도 보이게 합니다.
        // (0, 0, 0)인 이유는 위에서 matrix의 위치를 boxCenter로 잡았기 때문입니다.
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(attackWidth, attackDistance, 1f));

        // 5. 원래 matrix로 복구
        Gizmos.matrix = oldMatrix;
    }

    public int Guard()
    {
        return Guarding ? 80 : 1;
    }

    public void LeftHook()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemy);
        foreach (Collider hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage * 2);
        }

        isHook = true;
        hookTimer = hookDuration;
        cooldownTimerHook = hookCooldown;

    }

    public void Dash(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimerDashDash = dashCooldown;
        dashDirection = direction.normalized;

        UnityEngine.Debug.Log("대쉬!");
    }

    public void Boost()
    {
        isBoost = true;
        boostTimer = boostDuration;
        cooldownTimerBoost = boostCooldown;

    }

    public void TakeDamage(float damage)
    {
        PlayerCurrentHp -= damage / Guard();
        UnityEngine.Debug.Log("플레이어 공격 받음!");

        if (hpSlider != null)
        {
            hpSlider.value = PlayerCurrentHp;
        }

        if (PlayerCurrentHp <= 0)
        {
            PlayerCurrentHp = 0;
            Die();
        }
    }

    void Die()
    {

        deathUI.SetActive(true);

        Time.timeScale = 0f;

        UnityEngine.Debug.Log("플레이어 죽었다~!");

    }

    public void CoolDownMananger()
    {
        //금강불괴
        if (cooldownTimerBoost > 0)
        {
            cooldownTimerBoost -= Time.deltaTime;
        }

        if (isBoost)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
            {
                isBoost = false;
            }
            return;
        }

        //대쉬
        if (cooldownTimerDashDash > 0)
        {
            cooldownTimerDashDash -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
            {
                isDashing = false;
            }
            return;
        }

        //레프트 훅
        if (cooldownTimerHook > 0)
        {
            cooldownTimerHook -= Time.deltaTime;
        }

        if (isHook)
        {
            hookTimer -= Time.deltaTime;

            if (hookTimer <= 0)
            {
                isHook = false;
            }
            return;
        }

    }

}