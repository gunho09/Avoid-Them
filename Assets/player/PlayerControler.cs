using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

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
    public int attackNum = 2;
    public LayerMask enemy;             // 적 레이어 설정 필수
    private bool isAttacking = false;

    [Header("계산된 스탯")]
    public float PlayerMaxHp;
    public float PlayerDamage;
    public float PlayerCurrentHp;

    [Header("UI 연결")]
    public Slider hpSlider;
    public GameObject deathUI;
    public Slider ExpSlider;
    public TextMeshProUGUI LvlText;
    public TextMeshProUGUI hpText;   
    public TextMeshProUGUI expText;
    public TextMeshProUGUI CoolDownText1;
    public TextMeshProUGUI CoolDownText2;
    public TextMeshProUGUI CoolDownText3;
    public TextMeshProUGUI CoolDownText4;
    public Image bloodOverlay;

    [Header("애니메이션 연결")]
    private Animator anim;
    private SpriteRenderer sr;

    [Header("기술/상태 변수")]
    private bool Guarding = false;
    private bool isDashing = false;
    private bool isBoost = false;
    private bool isHook = false;
    private bool isAttack = false;


    [Header("레벨관리")]
    public float exp;
    public float currentExp = 0;
    public float PlayerLvl = 0;
    public float MaxExp = 20;

    [Header("쿨타임")]
    public float dashCooldown = 2f;
    public float dashSpeed = 60f;
    public float dashDuration = 0.2f;
    private float dashTimer, cooldownTimerDashDash;
    private Vector3 dashDirection;

    public float boostCooldown = 30f;
    private float boostTimer, cooldownTimerBoost;
    public float boostDuration = 6f;

    public float AttackCooldown = 0.5f;
    private float AttackTimer, cooldownTimerAttack;
    public float AttackDuration = 0.2f;


    public float hookCooldown = 5f;
    public float attackRange = 1.5f; // 훅 범위
    private float hookTimer, cooldownTimerHook;
    public float hookDuration = 0.3f;

    private Rigidbody2D rb;
    private Vector2 inputMovement;

    SpriteRenderer spriter;


    void Start()
    {
        CurrentPlayer();
        
        
        LvlText.text = $"{PlayerLvl}";
        if (expText != null) expText.text = $"{currentExp} / {MaxExp}";
        if (hpText != null) hpText.text = $"{PlayerCurrentHp} / {PlayerMaxHp}";

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();


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

        // 이동 입력
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        inputMovement = new Vector2(moveX, moveY);

        
        
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        anim.SetFloat("hInput", h);
        anim.SetFloat("vInput", v);

        if (h > 0)
        {
            sr.flipX = false;
        }
        else if (h < 0)
        {
            sr.flipX = true;
        }



        //if (sr != null)
        //{
        //    if (moveX > 0) sr.flipX = false;
        //    else if (moveX < 0) sr.flipX = true;
        //}



        // 입력 처리
        

        if (Input.GetMouseButtonDown(1))
        {
            
            Guarding = true;
            playerSpeed = 1f;
            
        }
        if (Input.GetMouseButtonUp(1))
        {
            
            Guarding = false;
            playerSpeed = 5f;


        }

        if (Input.GetMouseButtonDown(0) && cooldownTimerAttack <= 0)
        {
            Attack1();
        }

        // 2. 훅 기술에 쿨타임 조건 추가 (기존에는 조건이 없었음)
        if (Input.GetKeyDown(KeyCode.E) && cooldownTimerHook <= 0)
        {
            LeftHook();
        }

        // 대쉬와 부스트는 이미 조건문이 잘 들어가 있습니다.
        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownTimerDashDash <= 0) Dash(inputMovement);
        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimerBoost <= 0) Boost();

        CoolDownMananger(); // 이 함수가 돌아가면서 수치를 깎음

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
        else if (isAttacking || isAttack)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.linearVelocity = inputMovement.normalized * playerSpeed;
        }
    }


    IEnumerator AttackStopRoutine()
    {
        isAttacking = true;

        rb.linearVelocity = Vector2.zero;

        // 2. 여기에 애니메이션 실행 코드 작성 (예: anim.SetTrigger("Attack"))

        yield return new WaitForSeconds(0.2f); // 0.2초간 멈춤

        isAttacking = false;
    }

    IEnumerator ScreenFlash()
    {
        float duration = 0.3f;
        float maxAlpha = 0.6f;

        bloodOverlay.color = new Color(0.4f, 0, 0, maxAlpha);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(maxAlpha, 0, elapsed / duration);
            bloodOverlay.color = new Color(0.4f, 0, 0, alpha);
            yield return null;
        }
        bloodOverlay.color = new Color(0, 0, 0, 0);
    }

   

    public void Attack1() //원
    {
        playerSpeed = 0f;
        attackNum++;
        // 1. 마우스 방향 계산
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 attackDir = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;



        // 2. 공격 박스 설정 (쨉)
        Vector2 boxCenter = (Vector2)transform.position + attackDir * (attackDistance / 2f);
        Vector2 boxSize = new Vector2(attackWidth, attackDistance);
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg - 90f;
        
       
        StartCoroutine(AttackStopRoutine());
        // 3. 판정 및 데미지
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle, enemy);
        foreach (Collider2D hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage * (isBoost ? 2 : 1));
            Debug.Log($"원");
        }

        // 4. 공격 시각화 (네모 상자 그리기)
        Vector2 rightEdge = new Vector2(-attackDir.y, attackDir.x) * (attackWidth / 2f);
        Debug.DrawLine((Vector2)transform.position + rightEdge, (Vector2)transform.position - rightEdge, Color.cyan, 0.2f);
        Debug.DrawLine((Vector2)transform.position + rightEdge, (Vector2)transform.position + attackDir * attackDistance + rightEdge, Color.cyan, 0.2f);
        Debug.DrawLine((Vector2)transform.position - rightEdge, (Vector2)transform.position + attackDir * attackDistance - rightEdge, Color.cyan, 0.2f);
        Debug.DrawLine((Vector2)transform.position + attackDir * attackDistance + rightEdge, (Vector2)transform.position + attackDir * attackDistance - rightEdge, Color.cyan, 0.2f);
        playerSpeed = 5f;

        isAttack = true;
        AttackTimer = AttackDuration;
        cooldownTimerAttack = AttackCooldown;
    }

    public void LeftHook()
    {

        // 1. 마우스 방향 계산
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 attackDir = (mousePos - transform.position).normalized;

        // --- [시각화: Debug.DrawLine] ---
        float angleRange = 60f; // 중심에서 좌우 60도 (총 120도)
        int segments = 10;  
        float duration = 0.2f;

        Vector3 leftRay = Quaternion.Euler(0, 0, angleRange) * attackDir;
        Vector3 rightRay = Quaternion.Euler(0, 0, -angleRange) * attackDir;

        Debug.DrawLine(transform.position, transform.position + leftRay * attackRange, Color.cyan, duration);
        Debug.DrawLine(transform.position, transform.position + rightRay * attackRange, Color.cyan, duration);

        Vector3 previousPoint = transform.position + leftRay * attackRange;
        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = Mathf.Lerp(angleRange, -angleRange, (float)i / segments);
            Vector3 nextDir = Quaternion.Euler(0, 0, currentAngle) * attackDir;
            Vector3 nextPoint = transform.position + nextDir * attackRange;
            Debug.DrawLine(previousPoint, nextPoint, Color.cyan, duration);
            previousPoint = nextPoint;
        }
        StartCoroutine(AttackStopRoutine());

        // --- [공격 판정: Physics2D] ---
        // 1. 먼저 내 주변 원형 범위의 적을 다 찾습니다.
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemy);

        foreach (Collider2D hit in hits)
        {
            // 2. 적이 내 앞에 있는지(각도 체크) 확인합니다.
            Vector2 dirToEnemy = (hit.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(attackDir, dirToEnemy);

            if (angle <= angleRange) // 부채꼴 범위 안에 들어와 있다면
            {
                hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage * 2f * (isBoost ? 2 : 1));
            }
        }

        // 상태 업데이트
        Debug.Log("훅훅!");
        isHook = true;
        hookTimer = hookDuration;
        cooldownTimerHook = hookCooldown;


    }

    private void OnDrawGizmos()
    {
        // 1. 마우스 방향 벡터 계산 (공격 로직과 동일하게)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 attackDir = (mousePos - transform.position).normalized;

        // 2. Gizmos 색상 설정
        Gizmos.color = Color.red;

        // 3. 중심선 그리기
        Gizmos.DrawRay(transform.position, attackDir * attackRange);

        // 4. 부채꼴의 양 끝선 계산 (60도씩 회전)
        Vector3 leftBoundary = Quaternion.Euler(0, 0, 60f) * attackDir;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -60f) * attackDir;

        // 5. 양 끝 경계선 그리기
        Gizmos.DrawRay(transform.position, leftBoundary * attackRange);
        Gizmos.DrawRay(transform.position, rightBoundary * attackRange);

        // 6. [심화] 끝부분을 곡선으로 연결 (원 모양처럼 보이게)
        int segments = 10; // 선을 10개로 쪼개서 곡선 만들기
        Vector3 previousPoint = leftBoundary;
        for (int i = 1; i <= segments; i++)
        {
            // 60도에서 -60도까지 순차적으로 회전하며 점 찍기
            float angle = Mathf.Lerp(60f, -60f, (float)i / segments);
            Vector3 nextPoint = Quaternion.Euler(0, 0, angle) * attackDir;

            Gizmos.DrawLine(transform.position + previousPoint * attackRange,
                            transform.position + nextPoint * attackRange);
            previousPoint = nextPoint;
        }
    }

    public void Dash(Vector3 direction)
    {
        if (direction == Vector3.zero) direction = transform.right;

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
        if (Guarding && isBoost) return 0.1f;
        if (Guarding) return 0.2f;           
        return 1f;                           
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
        if (hpText != null) hpText.text = $"{PlayerCurrentHp} / {PlayerMaxHp}";
    }

    void Die()
    {
        PlayerCurrentHp = 0; if (hpText != null) hpText.text = $"{PlayerCurrentHp} / {PlayerMaxHp}";
        if (deathUI != null) deathUI.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("플레이어 사망");
    }

    public void TakeExp(float exp)
    {      
        currentExp += exp;
        if (ExpSlider != null) ExpSlider.value = currentExp;
        if (expText != null) expText.text = $"{currentExp} / {MaxExp}";

        if(MaxExp <= currentExp)
        {
            LevelUp();
        }

    }

    void CurrentPlayer()
    {
        if (ExpSlider != null) ExpSlider.value = currentExp;
        PlayerMaxHp = plusHp + numHp + playerStartHp;
        PlayerDamage = plusPW + numPW + playerStartPw;
        PlayerCurrentHp = PlayerMaxHp;

    }

    void LevelUp()
    {

        while (currentExp >= 20) { 

            currentExp -= MaxExp;
            PlayerCurrentHp += PlayerMaxHp * 0.2f;
            if(PlayerCurrentHp > PlayerMaxHp) PlayerCurrentHp = PlayerMaxHp;
            PlayerDamage += 0.05f * PlayerDamage;
            PlayerLvl++;
            if (ExpSlider != null) ExpSlider.value = currentExp;
            if (expText != null) expText.text = $"{currentExp} / {MaxExp}";
            LvlText.text = $"{PlayerLvl}";
            

        }

    }

    void inventoryMananger()
    {
       // 나중에 아이템 
    }

    private void CoolDownMananger()
    {
        float dt = Time.deltaTime;

        if (cooldownTimerBoost > 0) 
        { 
            cooldownTimerBoost -= dt; 
            CoolDownText4.text = $"금광불괴\n{(int)cooldownTimerBoost}"; 
        }
        else
        {
            CoolDownText4.text = "금광불괴";
        }
        if (isBoost) { boostTimer -= dt; if (boostTimer <= 0) isBoost = false; }


        if (cooldownTimerDashDash > 0) 
        { 
            cooldownTimerDashDash -= dt; 
            CoolDownText1.text = ""; 
        }
        else
        {
            CoolDownText1.text = "";
        }
        if (isDashing) { dashTimer -= dt; if (dashTimer <= 0) isDashing = false; }
        

        if (cooldownTimerHook > 0) 
        {
            cooldownTimerHook -= dt; 
            CoolDownText3.text = $"훅\n{(int)cooldownTimerHook}"; 
        }
        else
        {
            CoolDownText3.text = "훅";
        }
        if (isHook) { hookTimer -= dt; if (hookTimer <= 0) isHook = false; }


        // 원투는 쿨타임 표시 제외
        if (cooldownTimerAttack > 0) 
        { 
            cooldownTimerAttack -= dt; 
            CoolDownText2.text = ""; 
        }
        else
        {
            CoolDownText2.text = "";
        }
        if (isAttack) { AttackTimer -= dt; if (AttackTimer <= 0) isAttack = false; }
    }

    public void GoMain()
        {
            if (deathUI != null) deathUI.SetActive(false);
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI");
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
}
