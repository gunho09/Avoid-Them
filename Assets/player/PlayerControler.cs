using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;



public class PlayerControler : MonoBehaviour, IDamageable
{
    public static PlayerControler Instance;

    [Header("기본 스탯")]
    public float playerSpeed = 5f;
    public float playerStartHp = 100f;
    public float playerStartPw = 30f;
    public float plusHp = 0f, numHp = 0f;
    public float plusPW = 0f, numPW = 0f;

    [Header("사운드 타이머")]
    private float walkSoundTimer = 0f;
    private float walkSoundInterval = 0.4f; // 걷는 소리 간격 (조절 가능)

    [Header("공격 설정 (쨉/찌르기)")]
    public float attackDistance = 2.0f; 
    public float attackWidth = 0.8f;    
    public float attackDamage = 10f;
    public int attackNum = 2;
    public LayerMask enemy;             
    private bool isAttacking = false;

    [Header("계산된 스탯")]
    public float PlayerMaxHp;
    public float PlayerDamage;
    public float PlayerCurrentHp;
    public float PlayerCurrentShield = 0f;

    [Header("UI 연결")]
    public Slider hpSlider;
    public Slider shieldSlider; // [New] 쉴드 슬라이더 (회색)
    public GameObject deathUI;
    public Slider ExpSlider;
    public Slider BoostTime;
    public TextMeshProUGUI LvlText;
    public TextMeshProUGUI hpText;   
    public TextMeshProUGUI expText;
    public TextMeshProUGUI CoolDownText1; // 평타
    public TextMeshProUGUI CoolDownText2; // 훅
    public TextMeshProUGUI CoolDownText3; // 대시
    public TextMeshProUGUI CoolDownText4; // 부스트
    public Image boostIconOverlay;
    public Image hookIconOverlay;
    public Image bloodOverlay;
    public TextMeshProUGUI AttackDamageText;

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
    public float PlayerLvl = 1;
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
    public float attackRange = 1.5f; 
    private float hookTimer, cooldownTimerHook;
    public float hookDuration = 0.3f;

    private Rigidbody2D rb;
    private Vector2 inputMovement;

    // --- [아이템 관련 변수] ---
    [Header("아이템/더미 설정")]
    public GameObject dummyPrefab; 
    public GameObject magneticFieldVisual; // 자기장 이펙트 오브젝트 

    private float reflectCooldownTimer = 0f;
    private float reflectDurationTimer = 0f;
    private bool isReflecting = false;

    private float magneticCooldownTimer = 0f;
    private float magneticDurationTimer = 0f;
    private bool isMagnetic = false;

    private float shockwaveTimer = 0f;
    
    private int attackHitCount = 0;
    private int driveHitCount = 0;
    private bool isSlayerActive = false;
    private float driveSpeedBonus = 0f;
    private float driveTimer = 0f;
    // -------------------------
    private bool facingLeft = false;
    private bool lockFacing = false;
    private int num = 0;
    
    // [Fix] 쉴드 무한 회복 버그 수정용
    private float lastShieldBonus = 0f;

    void Start()
    {
        if (Instance == null) Instance = this;
        
        // [New Formula] Start시 경험치 테이블 초기화 (Lv 1 -> 100)
        // 공식: 100 + ((Lv-1) * 50)
        MaxExp = 20f + (Mathf.Max(0, PlayerLvl - 1) * 1f);
        if (BoostTime != null) BoostTime.gameObject.SetActive(false);
    
    // 초기화 시점에는 Stats 계산 (이벤트 전)
    RecalculateStats();
        PlayerCurrentHp = PlayerMaxHp; 
        
        LvlText.text = $"{PlayerLvl}";
        if (expText != null) expText.text = $"{currentExp} / {MaxExp}";
        UpdateHpUI();

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (hpSlider != null) 
        {
            hpSlider.maxValue = PlayerMaxHp;
            hpSlider.value = PlayerCurrentHp;
        }

        // 이벤트 구독
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged += RecalculateStats;
            RecalculateStats(); // 초기 계산
        }
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged -= RecalculateStats;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // 1) 입력 받기
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        inputMovement = new Vector2(moveX, moveY);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 2) 마우스 / 바라보는 방향(애니메이터용)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = ((Vector2)mousePos - (Vector2)transform.position).normalized;
        if (anim != null)
        {
            anim.SetFloat("hInput", h);
            anim.SetFloat("vInput", v);
            anim.SetFloat("MouseX", lookDir.x);
            anim.SetFloat("MouseY", lookDir.y);
        }
        // 3) 아이템 타이머
        UpdateItemTimers(dt);

        // 4) 가드 (우클릭)
        if (Input.GetMouseButtonDown(1))
        {
            Guarding = true;
            anim.SetBool("isGuarding", true);
            playerSpeed = 1f;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            Guarding = false;
            anim.SetBool("isGuarding", false);
            playerSpeed = 5f + driveSpeedBonus;
        }

        // 5) 방향 결정: 공격/훅 입력이 있으면 "마우스 방향" 우선 + 잠금
        bool attackPressed = Input.GetMouseButtonDown(0) && cooldownTimerAttack <= 0;
        bool hookPressed = Input.GetKeyDown(KeyCode.E) && cooldownTimerHook <= 0;

        if (attackPressed || hookPressed)
        {
            lockFacing = true;
            facingLeft = (mousePos.x < transform.position.x);
        }
        else if (!lockFacing)
        {
            // 공격/훅 중이 아닐 때만 이동 방향으로 갱신
            if (moveX > 0) facingLeft = false;
            else if (moveX < 0) facingLeft = true;
            // moveX == 0이면 기존 방향 유지
        }

        // 6) 실제 행동 실행 (트리거)
        if (attackPressed)
        {
            if (num%2==0) { anim.SetTrigger("Attack"); num++; Debug.Log("원"); }

            else { anim.SetTrigger("Attack2"); num++; Debug.Log("투"); }

            Attack1();
            StartCoroutine(UnlockFacingAfter(AttackDuration)); // 0.2f 하드코딩 말고 변수로!
        }

        if (hookPressed)
        {
            anim.SetTrigger("Hook");
            LeftHook();
            StartCoroutine(UnlockFacingAfter(hookDuration));   // 0.3f 하드코딩 말고 변수로!
        }

        // 7) 대시 / 부스트
        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownTimerDashDash <= 0) Dash(inputMovement);
        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimerBoost <= 0) Boost();

        // 8) flip 적용은 여기 한 줄만
        sr.flipX = facingLeft;

        // 9) 쿨타임/사운드/디버그
        CoolDownMananger();
        UpdateWalkSound(dt);

        Vector2 d = ((Vector2)mousePos - (Vector2)transform.position).normalized;
        Debug.DrawRay(transform.position, d * attackDistance, Color.red);
    }


    void FixedUpdate()
    {
        if (rb == null) return;

        Vector2 desiredVelocity = Vector2.zero;

        if (isDashing)
        {
            desiredVelocity = dashDirection * dashSpeed;
        }
        else if (isAttacking || isAttack)
        {
            desiredVelocity = Vector2.zero;
        }
        else
        {
            desiredVelocity = inputMovement.normalized * (playerSpeed + driveSpeedBonus);
        }

        // [New] 벽 충돌 체크 (Is Trigger ON 상태에서도 벽을 못 뚫게)
        if (desiredVelocity.sqrMagnitude > 0.01f)
        {
            float moveDistance = desiredVelocity.magnitude * Time.fixedDeltaTime;
            Vector2 moveDir = desiredVelocity.normalized;

            // 플레이어 크기에 맞는 BoxCast (콜라이더 크기 사용)
            Collider2D col = GetComponent<Collider2D>();
            Vector2 boxSize = col != null ? col.bounds.size : new Vector2(0.5f, 0.5f);
            boxSize *= 0.9f; // 살짝 줄여서 끼임 방지

            RaycastHit2D hit = Physics2D.BoxCast(
                rb.position, boxSize, 0f, moveDir, moveDistance, wallLayer
            );

            if (hit.collider != null)
            {
                // 벽에 닿으면: 벽 바로 앞까지만 이동
                float safeDistance = Mathf.Max(0, hit.distance - 0.02f);
                
                // 축별로 분리해서 벽을 타고 미끄러지게 처리
                Vector2 velX = new Vector2(desiredVelocity.x, 0);
                Vector2 velY = new Vector2(0, desiredVelocity.y);

                // X축 체크
                if (velX.sqrMagnitude > 0.01f)
                {
                    RaycastHit2D hitX = Physics2D.BoxCast(rb.position, boxSize, 0f, velX.normalized, 
                        velX.magnitude * Time.fixedDeltaTime, wallLayer);
                    if (hitX.collider != null) velX = Vector2.zero;
                }

                // Y축 체크
                if (velY.sqrMagnitude > 0.01f)
                {
                    RaycastHit2D hitY = Physics2D.BoxCast(rb.position, boxSize, 0f, velY.normalized, 
                        velY.magnitude * Time.fixedDeltaTime, wallLayer);
                    if (hitY.collider != null) velY = Vector2.zero;
                }

                desiredVelocity = velX + velY;
            }
        }

        rb.linearVelocity = desiredVelocity;
    }

    // --- [쿨타임 매니저] ---
    void CoolDownMananger()
    {
        float dt = Time.deltaTime;

        // 1. 공격 (Attack)
        if (cooldownTimerAttack > 0) cooldownTimerAttack -= dt;
        if (AttackTimer > 0) AttackTimer -= dt;
        else if(isAttack) isAttack = false;

        if (CoolDownText1 != null)
        {
            if (cooldownTimerAttack > 0) CoolDownText1.text = $"{cooldownTimerAttack}";
            else CoolDownText1.text = "";
        }

        // 2. 훅 (Hook E)
        if (cooldownTimerHook > 0) cooldownTimerHook -= dt;
        if (hookTimer > 0) hookTimer -= dt;
        else if(isHook) isHook = false;

        if (CoolDownText2 != null)
        {
            if (cooldownTimerHook > 0) CoolDownText2.text = $"훅/ {(int)cooldownTimerHook}";
            else CoolDownText2.text = "";
        }

        // 3. 대시 (Shift)
        if (cooldownTimerDashDash > 0) cooldownTimerDashDash -= dt;
        if (dashTimer > 0) dashTimer -= dt;
        else if(isDashing) isDashing = false;

        if (CoolDownText3 != null)
        {
            if (cooldownTimerDashDash > 0) CoolDownText3.text = $"{cooldownTimerDashDash}";
            else CoolDownText3.text = "";
        }

        // 4. 부스트 (Boost Q)
        if (cooldownTimerBoost > 0) cooldownTimerBoost -= dt;
        if (boostTimer > 0) boostTimer -= dt;
        else if(isBoost) isBoost = false;

        if (CoolDownText4 != null)
        {
            if (cooldownTimerBoost > 0) {
                CoolDownText4.text = $"금강불괴/{(int)cooldownTimerBoost}";
            }
            else CoolDownText4.text = "";
        }

        // 부스트 지속시간 바
        if (BoostTime != null)
        {
            bool show = boostTimer > 0;
            BoostTime.gameObject.SetActive(show);

            if (show)
            {
                BoostTime.maxValue = boostDuration;
                BoostTime.value = boostTimer;
            }
        }

    }

    void UpdateItemTimers(float dt)
    {
        if (Inventory.Instance == null) return;

        // 1. 반사 
        if (Inventory.Instance.GetStackCount(ItemEffectType.Reflection) > 0)
        {
            if (isReflecting)
            {
                reflectDurationTimer -= dt;
                if (reflectDurationTimer <= 0)
                {
                    isReflecting = false;
                    reflectCooldownTimer = 10f;
                }
            }
            else
            {
                reflectCooldownTimer -= dt;
                if (reflectCooldownTimer <= 0)
                {
                    isReflecting = true;
                    reflectDurationTimer = 3f;
                    StartCoroutine(ScreenFlash(Color.yellow));
                }
            }
        }

        // 2. 자기장
        if (Inventory.Instance.GetStackCount(ItemEffectType.DamageReduction) > 0)
        {
            if (isMagnetic)
            {
                if (magneticFieldVisual != null) magneticFieldVisual.SetActive(true);

                magneticDurationTimer -= dt;
                if (magneticDurationTimer <= 0)
                {
                    isMagnetic = false;
                    magneticCooldownTimer = 20f;
                }
            }
            else
            {
                if (magneticFieldVisual != null) magneticFieldVisual.SetActive(false);

                magneticCooldownTimer -= dt;
                if (magneticCooldownTimer <= 0)
                {
                    isMagnetic = true;
                    magneticDurationTimer = 5f;
                    StartCoroutine(ScreenFlash(Color.blue));
                }
            }
        }
        else
        {
             if (magneticFieldVisual != null) magneticFieldVisual.SetActive(false);
        }

        // 3. 충격파
        if (Inventory.Instance.GetStackCount(ItemEffectType.Knockback) > 0)
        {
            shockwaveTimer -= dt;
            if (shockwaveTimer <= 0)
            {
                TriggerShockwave();
                shockwaveTimer = 15f;
            }
        }

        // 4. 드라이브
        if (driveTimer > 0)
        {
            driveTimer -= dt;
            if (driveTimer <= 0)
            {
                driveSpeedBonus = 0f;
                driveHitCount = 0;
            }
        }
    }

    void TriggerShockwave()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 4f, enemy); 
        foreach (var hit in hits)
        {
            float shockDamage = PlayerDamage * 0.5f; 
            hit.GetComponent<IDamageable>()?.TakeDamage(shockDamage);
            
            var zombie = hit.GetComponent<zombie>();
            if (zombie != null)
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                zombie.ApplyCcKnockback(dir * 5f);
                zombie.ApplyCcStun(2f);
            }
        }
        StartCoroutine(ScreenFlash(Color.white));
    }

    public void Attack1()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-3", 1.0f); // 기본 공격

        attackNum++;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 attackDir = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

        Vector2 boxCenter = (Vector2)transform.position + attackDir * (attackDistance / 2f);
        Vector2 boxSize = new Vector2(attackWidth, attackDistance);
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg - 90f;

        // ... (omitted)
        StartCoroutine(AttackStopRoutine());

        float dashBonus = 0f;
        bool hasQuickAttack = Inventory.Instance != null && Inventory.Instance.GetStackCount(ItemEffectType.DashAttackUp) > 0;
        if (hasQuickAttack && (isDashing || dashTimer > 0 || cooldownTimerDashDash > (dashCooldown - 1f)))
        {
            dashBonus = 0.2f;
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle, enemy);
        foreach (Collider2D hit in hits)
        {
            float finalDamage = PlayerDamage * (1f + dashBonus) * (isBoost ? 2 : 1);

            if (Inventory.Instance != null && Inventory.Instance.GetStackCount(ItemEffectType.DoubleAttack) > 0)
            {
                if (isSlayerActive)
                {
                    finalDamage *= 2f;
                    isSlayerActive = false;
                    attackHitCount = 0;
                }
                else
                {
                    attackHitCount++;
                    if (attackHitCount >= 3) isSlayerActive = true;
                }
            }

            if (Inventory.Instance != null && Inventory.Instance.GetStackCount(ItemEffectType.InstantKill) > 0)
            {
                if (Random.value < 0.05f) finalDamage = 9999f;
            }

            if (Inventory.Instance != null)
            {
                float amp = Inventory.Instance.GetTotalStatBonus(ItemEffectType.DamageAmplification);
                finalDamage *= (1f + amp);

                // 정밀 타격 (ConditionalDamageUp): 적 체력 80% 이상일 때
                IDamageable target = hit.GetComponent<IDamageable>();
                if (target != null && target.GetHpRatio() > 0.8f)
                {
                    float precisionBonus = Inventory.Instance.GetTotalStatBonus(ItemEffectType.ConditionalDamageUp);
                    finalDamage *= (1f + precisionBonus);
                }

                float vamp = Inventory.Instance.GetTotalStatBonus(ItemEffectType.Vampirism);
                if (vamp > 0) Heal(finalDamage * vamp);
            }

            hit.GetComponent<IDamageable>()?.TakeDamage(finalDamage);

            if (Inventory.Instance != null && Inventory.Instance.GetStackCount(ItemEffectType.Drive) > 0)
            {
                driveHitCount++;
                if (driveHitCount >= 5)
                {
                    driveSpeedBonus = 2f;
                    driveTimer = 2f;
                    driveHitCount = 0;
                }
            }
        }

        DebugDrawBox(transform.position, attackDir, attackDistance, attackWidth);

        isAttack = true;
        AttackTimer = AttackDuration;

        cooldownTimerAttack = AttackCooldown * (1f - GetTotalCooldownReduction());
        // ... (omitted)
    }

    public void LeftHook()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 attackDir = (mousePos - transform.position).normalized;

        DebugDrawFan(attackDir, 60f, attackRange);
        StartCoroutine(AttackStopRoutine());
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-6"); // 레프트 훅


        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemy);
        foreach (Collider2D hit in hits)
        {
            Vector2 dirToEnemy = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            float angle = Vector2.Angle(attackDir, dirToEnemy);

            if (angle <= 60f)
            {
                float finalDamage = PlayerDamage * 2f * (isBoost ? 2 : 1);
                hit.GetComponent<IDamageable>()?.TakeDamage(finalDamage);
            }
        }

        isHook = true;
        hookTimer = hookDuration;
        cooldownTimerHook = hookCooldown * (1f - GetTotalCooldownReduction());
    }


    public void Boost()
    {
        isBoost = true;
        boostTimer = boostDuration;

        if (Inventory.Instance != null)
        {
            if (Inventory.Instance.GetStackCount(ItemEffectType.MoveSpeedUp) > 0)
            {
                PlayerCurrentShield += PlayerCurrentHp * 0.2f;
                StartCoroutine(ScreenFlash(Color.cyan));
            }
        }

        float reduction = 0f;
        if (Inventory.Instance != null)
            reduction = Inventory.Instance.GetTotalStatBonus(ItemEffectType.CooldownReduction);

        cooldownTimerBoost = boostCooldown * (1f - reduction);
        // 부스트 소리는 매핑에 없으므로 일단 보류하거나 대쉬 소리 사용 가능
        // if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-4");

    }

    public LayerMask wallLayer; // [New] 벽 레이어 설정 필요

    public void Dash(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;

        // [New] 벽 뚫기 방지 (Raycast)
        float dashDist = dashSpeed * dashDuration; // 예상 이동 거리
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, dashDist, wallLayer);
        
        if (hit.collider != null)
        {
            // 벽이 있으면 벽 바로 앞까지만 이동하거나, 대시를 막음
            // 여기서는 "벽에 부딪히면 멈춘다"는 느낌으로, 이동은 하되 벽에서 멈추게 처리하거나
            // 단순히 물리 엔진에게 맡기되 Continuous 설정을 믿음.
            // 하지만 확실히 하기 위해 거리를 제한할 수도 있음.
            
            // 일단은 경고 로그만 찍고, 물리 엔진(Rigidbody) 설정(Continuous)을 믿어봅니다.
            // 만약 그래도 뚫리면 여기 코드를 'rb.MovePosition' 등으로 바꿔야 함.
            Debug.DrawLine(transform.position, hit.point, Color.red, 1f);
        }
        
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-4"); // 대쉬

        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimerDashDash = dashCooldown * (1f - GetTotalCooldownReduction()); 
        dashDirection = direction.normalized;
    }

    float GetTotalCooldownReduction()
    {
        if (Inventory.Instance == null) return 0f;
        float cdr = Inventory.Instance.GetTotalStatBonus(ItemEffectType.CooldownReduction);
        float aspd = Inventory.Instance.GetTotalStatBonus(ItemEffectType.AttackSpeedUp);
        return Mathf.Clamp(cdr + aspd, 0f, 0.8f);
    }

    public float Guard()
    {
        if (Guarding && isBoost) return 0.1f;
        if (Guarding) return 0.2f;           
        return 1f;                           
    }

    public void Slow()
    {
        playerSpeed = 1f;
    }

    public void Fast()
    {
        playerSpeed = 5f;
    }

    void UpdateWalkSound(float dt)
    {
        // 입력이 있고, 복도에 있을 때만 소리 재생
        if (inputMovement.sqrMagnitude > 0.01f)
        {
            if (MapManager.Instance != null && MapManager.Instance.IsInHallway)
            {
                walkSoundTimer -= dt;
                if (walkSoundTimer <= 0)
                {
                    if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-1"); // 걷는 소리
                    walkSoundTimer = walkSoundInterval;
                }
            }
        }
        else
        {
            // 멈추면 타이머 리셋 (다시 걸을 때 바로 소리나게)
            walkSoundTimer = 0f;
        }
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, null);
    }

    public void TakeDamage(float damage, GameObject attacker)
    {
        if (isReflecting) 
        {
             Debug.Log("반사!");
             if (attacker != null)
             {
                 var target = attacker.GetComponent<IDamageable>();
                 if (target != null)
                 {
                     // 반사 데미지: 받은 데미지를 그대로 돌려줌 (또는 증폭 가능)
                     target.TakeDamage(damage);
                 }
             }
             return; 
        }

        float reduction = 1f;
        if (isMagnetic) reduction = 0.5f; 
        float finalDamage = damage * Guard() * reduction;

        if (finalDamage > 0 && Inventory.Instance != null && Inventory.Instance.GetStackCount(ItemEffectType.AggroDistribution) > 0)
        {
            if (Random.value < 0.1f)
            {
                 // DummyItem 찾기
                 if (dummyPrefab != null && FindFirstObjectByType<DummyItem>() == null)
                 {
                     GameObject dummyObj = Instantiate(dummyPrefab, transform.position, Quaternion.identity);
                     DummyItem dummyScript = dummyObj.GetComponent<DummyItem>();
                     
                     if (dummyScript != null)
                     {
                         // 플레이어 최대 체력의 50%로 설정
                         dummyScript.Setup(PlayerMaxHp * 0.4f);
                     }
                 }
            }
        }

        if (PlayerCurrentShield > 0)
        {
            if (PlayerCurrentShield >= finalDamage)
            {
                PlayerCurrentShield -= finalDamage;
                finalDamage = 0f;
            }
            else
            {
                finalDamage -= PlayerCurrentShield;
                PlayerCurrentShield = 0f;
            }
        }

        if (finalDamage > 0)
        {
            PlayerCurrentHp -= finalDamage;
            StartCoroutine(ScreenFlash(Color.red));
            
            if (Inventory.Instance != null && Inventory.Instance.GetStackCount(ItemEffectType.RecoveryUp) > 0)
            {
                if (PlayerCurrentHp < PlayerMaxHp * 0.1f)
                {
                    Heal(PlayerMaxHp * 0.5f);
                    Inventory.Instance.DecreaseItemCount(ItemEffectType.RecoveryUp); 
                    Debug.Log("비상식량 사용!");
                }
            }
        }

        UpdateHpUI();

        if (PlayerCurrentHp <= 0)
        {
            bool hasBand = Inventory.Instance != null && Inventory.Instance.GetStackCount(ItemEffectType.Resurrection) > 0;
            if (hasBand)
            {
                PlayerCurrentHp = 1f; 
                PlayerCurrentShield += 20f; 
                Inventory.Instance.DecreaseItemCount(ItemEffectType.Resurrection); 
                Debug.Log("밴드로 생존!");
                UpdateHpUI();
                return;
            }

            PlayerCurrentHp = 0;
            Die();
        }
    }

    void Die()
    {
        // 이미 죽었으면 무시
        if (PlayerCurrentHp > 0) return; // Die() 재호출 방지용 (TakeDamage에서 0 이하일때만 호출됨)
        
        UpdateHpUI();

        // [NEW] 사망 연출
        // 1. 물리/조작 비활성화 (이미 죽었으므로)
        if (rb != null) rb.linearVelocity = Vector2.zero;
        // 추가: 입력 막는 플래그가 필요하면 여기서 설정 (현재는 Update에서 Check 없음, Hp<=0이면 로직상 문제될 수 있음)
        // 간단히 Collider 꺼서 무적 처리
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 2. 이펙트 재생
        DeathEffect effect = GetComponent<DeathEffect>();
        if (effect == null) effect = gameObject.AddComponent<DeathEffect>();

        effect.PlayEffect(() => 
        {
            // 3. 연출 끝난 후 UI 표시 및 정지
            if (deathUI != null) deathUI.SetActive(true);
            Time.timeScale = 0f;
        });
    }

    public void Heal(float amount)
    {
        PlayerCurrentHp += amount;
        if (PlayerCurrentHp > PlayerMaxHp) PlayerCurrentHp = PlayerMaxHp;
        UpdateHpUI();
    }

    public void TakeExp(float amount)
    {
        currentExp += amount;
        
        if (expText != null) expText.text = $"{currentExp} / {MaxExp}";
        if (ExpSlider != null) ExpSlider.value = currentExp;

        if(currentExp >= MaxExp)
        {
            LevelUp();
        }
    }

    public void RecalculateStats()
    {
        float extraHpPercent = 0f;
        float extraAtkPercent = 0f;

        if (Inventory.Instance != null)
        {
            // 1. 유리 글로브
            if(Inventory.Instance.GetStackCount(ItemEffectType.GlassCannon) > 0) 
                 extraHpPercent = -(0.2f * Inventory.Instance.GetStackCount(ItemEffectType.GlassCannon));
            
            // 2. 강타자
            extraAtkPercent += Inventory.Instance.GetTotalStatBonus(ItemEffectType.AttackUp); 
            
            // 3. 부서진 배트
            if (PlayerMaxHp > 0)
            {
                float lostRatio = 1f - (PlayerCurrentHp / PlayerMaxHp);
                if (lostRatio > 0)
                {
                    float batBonus = Inventory.Instance.GetTotalStatBonus(ItemEffectType.ConditionalAttackUp);
                    extraAtkPercent += lostRatio * batBonus; 
                }
            }
        }

        // [New Formula] HP: 기본 + ((레벨-1) * 20)
        // Lv 1일 때는 보너스 없음 (300 유지)
        float levelBonusHp = Mathf.Max(0, (PlayerLvl - 1) * 20f);
        float baseMaxHp = plusHp + numHp + playerStartHp + levelBonusHp;
        PlayerMaxHp = baseMaxHp * (1f + extraHpPercent);
        
        if (PlayerMaxHp < 10f) PlayerMaxHp = 10f; 

        // 4. 두꺼운 과잠
        if (Inventory.Instance != null)
        {
            float shieldBonus = Inventory.Instance.GetTotalStatBonus(ItemEffectType.MaxHpShield);
            
            // [Fix] 쉴드가 계속 차는 오류 수정
            // 단순 재계산일 때는 쉴드를 채우지 않고, '쉴드 보너스 비율'이 늘어났을 때만 그만큼 추가해줍니다.
            if (shieldBonus > lastShieldBonus)
            {
                float diff = shieldBonus - lastShieldBonus;
                PlayerCurrentShield += PlayerMaxHp * diff;
            }
            
            // 보너스가 줄어들었거나(아이템 버림) 그대로면 쉴드량 유지 (단, 최대치 캡 적용)
            float maxPossibleshield = PlayerMaxHp * shieldBonus;
            if (PlayerCurrentShield > maxPossibleshield) 
            {
                PlayerCurrentShield = maxPossibleshield;
            }
            
            lastShieldBonus = shieldBonus;
        }

        UpdateHpUI();

       
        // [New Formula] Atk: 기본 * (1 + (레벨-1)*0.05)
        extraAtkPercent += (Mathf.Max(0, PlayerLvl - 1) * 0.05f);

        float baseDamage = plusPW + numPW + playerStartPw;
        PlayerDamage = baseDamage * (1f + extraAtkPercent);
        AttackDamageText.text = $"공격력 : {Mathf.Ceil(PlayerDamage)}";
        
        if (ExpSlider != null) ExpSlider.value = currentExp;
    }

    void UpdateHpUI()
    {
         if (hpSlider != null) 
         {
             hpSlider.maxValue = PlayerMaxHp;
             hpSlider.value = PlayerCurrentHp; 
         }

         if (shieldSlider != null)
         {
             // 쉴드가 체력을 초과할 경우를 대비해 MaxValue 조정 (선택사항이나, 일단 기본 MaxHp 기준)
             float totalValue = PlayerCurrentHp + PlayerCurrentShield;
             
             // 만약 전체 양(체력+쉴드)이 MaxHp보다 크면 슬라이더 Max를 늘려줌 (그래야 쉴드가 보임)
             float displayMax = Mathf.Max(PlayerMaxHp, totalValue);

             shieldSlider.maxValue = displayMax;
             shieldSlider.value = totalValue;

             // HP 슬라이더도 비율을 맞추려면 같이 늘려줘야 함
             if (hpSlider != null) hpSlider.maxValue = displayMax;
         }
         
         string shieldStr = PlayerCurrentShield > 0 ? $" (+{Mathf.Ceil(PlayerCurrentShield)})" : "";
         if (hpText != null) hpText.text = $"{Mathf.Ceil(PlayerCurrentHp)}{shieldStr} / {Mathf.Ceil(PlayerMaxHp)}";
    }

    public void Quit() 
    { 
        Time.timeScale = 1f; // 일시정지 해제 후 종료
        Application.Quit(); 
    }
    public void GoMain() 
    { 
        Time.timeScale = 1f; // 일시정지 해제 후 메인으로
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI"); 
    }

    void LevelUp() 
    {
        while (currentExp >= MaxExp) { 
            currentExp -= MaxExp;
            PlayerLvl++;

            // [New Formula] Exp: 100 + ((Lv) * 50)
            MaxExp = 100f + (PlayerLvl * 50f);

            // 스탯 재계산 (레벨 반영)
            RecalculateStats();

            // [Change] 레벨업 시 체력 회복 제거
            // PlayerCurrentHp += PlayerMaxHp * 0.2f;
            // if(PlayerCurrentHp > PlayerMaxHp) PlayerCurrentHp = PlayerMaxHp;
        } 
        
        LvlText.text = $"{PlayerLvl}";
        AttackDamageText.text = $"공격력 : {Mathf.Ceil(PlayerDamage)}";
        if (expText != null) expText.text = $"{currentExp} / {MaxExp}";
        if (ExpSlider != null) 
        {
            ExpSlider.maxValue = MaxExp;
            ExpSlider.value = currentExp;
        }
        UpdateHpUI();
    }
    IEnumerator UnlockFacingAfter(float t)
    {
        yield return new WaitForSeconds(t);
        lockFacing = false;
    }
    IEnumerator AttackStopRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.2f); 
        isAttacking = false;
    }

    void DebugDrawBox(Vector2 pos, Vector2 dir, float dist, float width)
    {
        Vector2 rightEdge = new Vector2(-dir.y, dir.x) * (width / 2f);
        Debug.DrawLine(pos + rightEdge, pos - rightEdge, Color.cyan, 0.2f);
        Debug.DrawLine(pos + rightEdge, pos + dir * dist + rightEdge, Color.cyan, 0.2f);
        Debug.DrawLine(pos - rightEdge, pos + dir * dist - rightEdge, Color.cyan, 0.2f);
        Debug.DrawLine(pos + dir * dist + rightEdge, pos + dir * dist - rightEdge, Color.cyan, 0.2f);
    }
    void DebugDrawFan(Vector2 dir, float angle, float range)
    {
        Vector3 left = Quaternion.Euler(0,0,angle) * dir * range;
        Vector3 right = Quaternion.Euler(0,0,-angle) * dir * range;
        Debug.DrawLine(transform.position, transform.position + left, Color.cyan);
        Debug.DrawLine(transform.position, transform.position + right, Color.cyan);
    }
    IEnumerator ScreenFlash(Color c)
    {
        if(bloodOverlay == null) yield break;
        bloodOverlay.color = new Color(c.r,c.g,c.b, 0.6f);
        yield return new WaitForSeconds(0.3f);
        bloodOverlay.color = Color.clear;
    }
    
    // [NEW] 인터페이스 구현
    public float GetHpRatio()
    {
        if (PlayerMaxHp <= 0) return 0f;
        return PlayerCurrentHp / PlayerMaxHp;
    }
}
