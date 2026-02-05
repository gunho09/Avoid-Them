using UnityEngine;

public class DummyItem : MonoBehaviour, IDamageable
{
    public float GetHpRatio() 
    {
        if (maxHealth <= 0) return 0f;
        return currentHealth / maxHealth;
    }

    private float maxHealth; 
    public float lifeTime = 10f; // 10초 후 자동 파괴 
    
    private float currentHealth;

    // 외부에서 초기화할 때 호출
    public void Setup(float hp)
    {
        maxHealth = hp;
        currentHealth = maxHealth;
        
        Debug.Log($"Dummy Spawned with HP: {currentHealth}");
    }

    void Start()
    {
        // Setup이 호출되지 않았을 경우를 대비해 기본값 설정
        if (currentHealth <= 0) currentHealth = maxHealth;
        
        if (!gameObject.CompareTag("Player"))
        {
            gameObject.tag = "Player";
        }
        
        PullAggro(); // 생성 시 어그로 획득
        Destroy(gameObject, lifeTime);
    }

    private void OnDestroy()
    {
        ReturnAggro(); // 파괴 시 어그로 반환
    }

    // 주변 좀비들의 타겟을 자신으로 변경
    void PullAggro()
    {
        // 범위 15f 내의 모든 콜라이더 검사 (좀비 레이어 필터링이 없으므로 모든 콜라이더 체크 후 컴포넌트 확인)
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, 15f);
        foreach (var col in cols)
        {
            zombie z = col.GetComponent<zombie>();
            if (z != null)
            {
                z.SetTarget(this.transform);
            }
        }
    }

    // 자신을 타겟으로 하고 있던 좀비들을 다시 플레이어로 복귀
    void ReturnAggro()
    {
        // 씬 내의 모든 활성화된 좀비를 찾아서 확인 (범위 제한 없이 확실하게 복구)
        zombie[] allZombies = FindObjectsOfType<zombie>();
        foreach (var z in allZombies)
        {
            // 아직 살아있고, 나(Dummy)를 타겟팅하고 있는 경우에만 리셋
            if (z.IsTargeting(transform))
            {
                z.ResetTarget();
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
