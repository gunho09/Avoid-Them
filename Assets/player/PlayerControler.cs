using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용
using System.Collections;
using System.Collections.Generic;

// 주의: SceneManager 이름 충돌 방지를 위해 네임스페이스 명시적 사용

public class PlayerControler : MonoBehaviour, IDamageable
{
    public static PlayerControler Instance;

    [Header("기본 스탯")]
    public float playerSpeed = 5f;
    public float playerStartHp = 100f;
    public float playerStartPw = 30f;
    public float plusHp = 0f, numHp = 0f;
    public float plusPW = 0f, numPW = 0f;

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

    void Start()
    {
        if (Instance == null) Instance = this;
        
        // [New Formula] Start시 경험치 테이블 초기화 (Lv 1 -> 100)
        // 공식: 100 + ((Lv-1) * 50)
        MaxExp = 100f + (Mathf.Max(0, PlayerLvl - 1) * 50f);

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

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        inputMovement = new Vector2(moveX, moveY);
       
        float h = Input.GetAxis("Horizontal"); 
        float v = Input.GetAxis("Vertical");

        anim.SetFloat("hInput", h);
        anim.SetFloat("vInput", v);

        if (h > 0) sr.flipX = false;
        else if (h < 0) sr.flipX = true;

        // CurrentPlayer() 삭제됨 -> RecalculateStats가 이벤트로 처리
        UpdateItemTimers(dt); 

        if (Input.GetMouseButtonDown(1))
        {
            Guarding = true;
            playerSpeed = 1f;
        }
        if (Input.GetMouseButtonUp(1))
        {
            Guarding = false;
            playerSpeed = 5f + driveSpeedBonus;
        }

        if (Input.GetMouseButtonDown(0) && cooldownTimerAttack <= 0) Attack1();
        if (Input.GetKeyDown(KeyCode.E) && cooldownTimerHook <= 0) LeftHook();
        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownTimerDashDash <= 0) Dash(inputMovement);
        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimerBoost <= 0) Boost();

        // 쿨타임 UI 및 타이머 관리 함수 호출
        CoolDownMananger(); 

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
            rb.linearVelocity = inputMovement.normalized * (playerSpeed + driveSpeedBonus);
        }
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
            if (cooldownTimerAttack > 0) CoolDownText1.text = $"{cooldownTimerAttack:F1}";
            else CoolDownText1.text = "";
        }

        // 2. 훅 (Hook E)
        if (cooldownTimerHook > 0) cooldownTimerHook -= dt;
        if (hookTimer > 0) hookTimer -= dt;
        else if(isHook) isHook = false;

        if (CoolDownText2 != null)
        {
            if (cooldownTimerHook > 0) CoolDownText2.text = $"{cooldownTimerHook:F1}";
            else CoolDownText2.text = "";
        }

        // 3. 대시 (Shift)
        if (cooldownTimerDashDash > 0) cooldownTimerDashDash -= dt;
        if (dashTimer > 0) dashTimer -= dt;
        else if(isDashing) isDashing = false;

        if (CoolDownText3 != null)
        {
            if (cooldownTimerDashDash > 0) CoolDownText3.text = $"{cooldownTimerDashDash:F1}";
            else CoolDownText3.text = "";
        }

        // 4. 부스트 (Boost Q)
        if (cooldownTimerBoost > 0) cooldownTimerBoost -= dt;
        if (boostTimer > 0) boostTimer -= dt;
        else if(isBoost) isBoost = false;

        if (CoolDownText4 != null)
        {
            if (cooldownTimerBoost > 0) CoolDownText4.text = $"{cooldownTimerBoost:F1}";
            else CoolDownText4.text = "";
        }
        
        // 부스트 지속시간 바
        if (BoostTime != null)
        {
            BoostTime.maxValue = boostDuration;
            BoostTime.value = boostTimer;
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
        attackNum++;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 attackDir = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

        Vector2 boxCenter = (Vector2)transform.position + attackDir * (attackDistance / 2f);
        Vector2 boxSize = new Vector2(attackWidth, attackDistance);
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg - 90f;
        
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
    }

    public void LeftHook() 
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 attackDir = (mousePos - transform.position).normalized;

        DebugDrawFan(attackDir, 60f, attackRange);
        StartCoroutine(AttackStopRoutine());

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemy);
        foreach (Collider2D hit in hits)
        {
            Vector2 dirToEnemy = (hit.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(attackDir, dirToEnemy);

            if (angle <= 60f) 
            {
                float finalDamage = PlayerDamage * 2f * (isBoost ? 2 : 1);
                
                if (Inventory.Instance != null)
                {
                    float amp = Inventory.Instance.GetTotalStatBonus(ItemEffectType.DamageAmplification); 
                    finalDamage *= (1f + amp);
                    
                    // 정밀 타격 (ConditionalDamageUp)
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
        if(Inventory.Instance != null)
            reduction = Inventory.Instance.GetTotalStatBonus(ItemEffectType.CooldownReduction); 

        cooldownTimerBoost = boostCooldown * (1f - reduction); 
    }

    public void Dash(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;
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

    public void TakeDamage(float damage)
    {
        if (isReflecting) 
        {
             Debug.Log("반사!");
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
                 if (dummyPrefab != null && FindObjectOfType<DummyItem>() == null)
                 {
                     GameObject dummyObj = Instantiate(dummyPrefab, transform.position, Quaternion.identity);
                     DummyItem dummyScript = dummyObj.GetComponent<DummyItem>();
                     
                     if (dummyScript != null)
                     {
                         // 플레이어 최대 체력의 50%로 설정
                         dummyScript.Setup(PlayerMaxHp * 0.5f);
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
            if (shieldBonus > 0)
            {
                float targetShield = PlayerMaxHp * shieldBonus;
                if (PlayerCurrentShield < targetShield)
                {
                    PlayerCurrentShield = targetShield;
                }
            }
        }

        UpdateHpUI();

       
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

    public void Quit() { Application.Quit(); }
    public void GoMain() 
    { 
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI"); 
    }

    void LevelUp() 
    {
        while (currentExp >= MaxExp) { 
            currentExp -= MaxExp;
            PlayerLvl++;

            // [New Formula] Exp: 100 + ((Lv-1) * 50)
            MaxExp = 100f + (Mathf.Max(0, PlayerLvl - 1) * 50f);

            // 스탯 재계산 (레벨 반영)
            RecalculateStats();

            // 체력 회복 (선택사항, 기존 유지)
            PlayerCurrentHp += PlayerMaxHp * 0.2f;
            if(PlayerCurrentHp > PlayerMaxHp) PlayerCurrentHp = PlayerMaxHp;
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
