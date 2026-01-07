using UnityEngine;
using UnityEngine.UI;

public class PlayerControler : MonoBehaviour, IDamageable
{
    [Header("기본 스탯")]
    public float playerSpeed = 5f;
    public float playerStartHp = 100f;
    public float playerStartPw = 30f;
    public float plusHp = 0f, numHp = 0f;
    public float plusPW = 0f, numPW = 0f;

    [Header("공격 설정 (쨉/찌르기)")]
    public float attackDistance = 2.0f; // 쨉 거리
    public float attackWidth = 0.8f;    // 쨉 너비
    public float attackDamage = 10f;
    public LayerMask enemy;             // 적 레이어 설정 필수

    [Header("계산된 스탯")]
    public float PlayerMaxHp;
    public float PlayerDamage;
    public float PlayerCurrentHp;

    [Header("UI 연결")]
    public Slider hpSlider;
    public GameObject deathUI;

    [Header("기술/상태 변수")]
    private bool Guarding = false;
    private bool isDashing = false;
    private bool isBoost = false;
    private bool isHook = false;

    [Header("레벨관리")]
    public float exp = 0;
    public float Lvl = 1;
    public float MaxExp = 20;

    // 쿨타임 및 타이머
    public float dashCooldown = 2f;
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;
    private float dashTimer, cooldownTimerDashDash;
    private Vector3 dashDirection;

    public float boostCooldown = 30f;
    private float boostTimer, cooldownTimerBoost;
    public float boostDuration = 5f;

    public float hookCooldown = 5f;
    public float attackRange = 1.5f; // 훅 범위
    private float hookTimer, cooldownTimerHook;
    public float hookDuration = 0.3f;

    private Rigidbody2D rb;
    private Vector2 inputMovement;

    void Start()
    {
        // 스탯 초기화
        PlayerMaxHp = plusHp + numHp + playerStartHp;
        PlayerDamage = plusPW + numPW + playerStartPw;
        PlayerCurrentHp = PlayerMaxHp;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (hpSlider != null)
        {
            hpSlider.maxValue = PlayerMaxHp;
            hpSlider.value = PlayerCurrentHp;
        }
    }

    void Update()
    {
        CoolDownMananger();

        // 이동 입력
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        inputMovement = new Vector2(moveX, moveY);


        // 입력 처리
        if (Input.GetMouseButtonDown(0)) Attack(); // 마우스 왼쪽: 쨉
        if (Input.GetKeyDown(KeyCode.E)) LeftHook(); // E: 레프트훅

        if (Input.GetMouseButtonDown(1)) Guarding = true;
        if (Input.GetMouseButtonUp(1)) Guarding = false;

        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownTimerDashDash <= 0)
        {
            Dash(new Vector3(moveX, moveY, 0));
        }

        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimerBoost <= 0) Boost();

        // 시각화 디버깅 (항상 마우스 방향으로 빨간 선 표시)
        Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 d = ((Vector2)mPos - (Vector2)transform.position).normalized;
        Debug.DrawRay(transform.position, d * attackDistance, Color.red);
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
            rb.linearVelocity = inputMovement.normalized * playerSpeed;
        }
    }

  

    public void Attack()
    {
        // 1. 마우스 방향 계산
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 attackDir = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

        // 2. 공격 박스 설정 (쨉)
        Vector2 boxCenter = (Vector2)transform.position + attackDir * (attackDistance / 2f);
        Vector2 boxSize = new Vector2(attackWidth, attackDistance);
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg - 90f;

        // 3. 판정 및 데미지
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle, enemy);
        foreach (Collider2D hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
            Debug.Log($"적이 공격 받음! 남은 체력 확인 필요");
        }

        // 4. 공격 시각화 (네모 상자 그리기)
        Vector2 rightEdge = new Vector2(-attackDir.y, attackDir.x) * (attackWidth / 2f);
        Debug.DrawLine((Vector2)transform.position + rightEdge, (Vector2)transform.position - rightEdge, Color.cyan, 0.2f);
        Debug.DrawLine((Vector2)transform.position + rightEdge, (Vector2)transform.position + attackDir * attackDistance + rightEdge, Color.cyan, 0.2f);
        Debug.DrawLine((Vector2)transform.position - rightEdge, (Vector2)transform.position + attackDir * attackDistance - rightEdge, Color.cyan, 0.2f);
        Debug.DrawLine((Vector2)transform.position + attackDir * attackDistance + rightEdge, (Vector2)transform.position + attackDir * attackDistance - rightEdge, Color.cyan, 0.2f);
    }

    public void LeftHook()
    {
        // 레프트훅은 전방위 혹은 넓은 부채꼴 (여기선 전방위 원형 예시)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemy);
        foreach (Collider2D hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage * 2f);
        }

        isHook = true;
        hookTimer = hookDuration;
        cooldownTimerHook = hookCooldown;
    }

    public void Dash(Vector3 direction)
    {
        if (direction == Vector3.zero) direction = transform.up; // 입력 없으면 정면으로

        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimerDashDash = dashCooldown;
        dashDirection = direction.normalized;
        Debug.Log("대쉬!");
    }

    public void Boost()
    {
        isBoost = true;
        boostTimer = boostDuration;
        cooldownTimerBoost = boostCooldown;
        Debug.Log("금강불괴 활성화!");
    }

    public float Guard()
    {
        if (Guarding && isBoost) return 0.1f; // 90% 감소
        if (Guarding) return 0.2f;            // 80% 감소
        return 1f;                            // 그대로 받음
    }

    public void TakeDamage(float damage)
    {
        float finalDamage = damage * Guard();
        PlayerCurrentHp -= finalDamage;
        Debug.Log($"플레이어 데미지 입음: {finalDamage}, 남은 체력: {PlayerCurrentHp}");

        if (hpSlider != null) hpSlider.value = PlayerCurrentHp;

        if (PlayerCurrentHp <= 0)
        {
            PlayerCurrentHp = 0;
            Die();
        }
    }

    void Die()
    {
        if (deathUI != null) deathUI.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("플레이어 사망");
        // gameObject.SetActive(false); // 필요시 주석 해제
    }

    public void TakeExp(float exp)
    {
        
    }

    void CurrentPlayer()
    {

    }

    void LevelUp()
    {

    }

    private void CoolDownMananger()
    {
        float dt = Time.deltaTime;

        if (cooldownTimerBoost > 0) cooldownTimerBoost -= dt;
        if (isBoost) { boostTimer -= dt; if (boostTimer <= 0) isBoost = false; }

        if (cooldownTimerDashDash > 0) cooldownTimerDashDash -= dt;
        if (isDashing) { dashTimer -= dt; if (dashTimer <= 0) isDashing = false; }

        if (cooldownTimerHook > 0) cooldownTimerHook -= dt;
        if (isHook) { hookTimer -= dt; if (hookTimer <= 0) isHook = false; }
    }
}