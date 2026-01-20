public interface IDamageable
{
    void TakeDamage(float damage);
    float GetHpRatio(); // 0.0 ~ 1.0 (정밀 타격 구현용)
}