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
        
        Destroy(gameObject, lifeTime);
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
