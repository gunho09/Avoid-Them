using UnityEngine;

public enum ItemRarity
{
    Normal,
    Rare,
    Legend
}

public enum ItemEffectType
{
    None,
    Vampirism,      // 1. 흡혈
    DamageAmplification, // 2. 주문의 검 (데미지 증폭)
    MoveSpeedUp,    // 3. 에너지드링크 (이속 증가)
    ConditionalAttackUp, // 4. 부서진 배트 (조건부 공증)
    MaxHpShield,    // 5. 두꺼운 과잠 (최대체력 비례 보호막)
    ConditionalDamageUp, // 6. 정밀 타격
    DamageReduction, // 7. 자기장 (피해 감소)
    Reflection,     // 8. 반사
    AggroDistribution, // 9. 더미 (어그로 분산) - 구현 복잡도 높음, 일단 정의
    DoubleAttack,   // 10. 학살자 (3타마다 2배)
    Knockback,      // 11. 충격파
    Resurrection,   // 12. 밴드 (부활)
    DashAttackUp,   // 14. 속공 (대시 공격력)
    InstantKill,    // 15. 급사
    GlassCannon,    // 16. 유리 글로브 (HP감소/ATK대폭증가)
    CooldownReduction, // 17. 감소
    AttackUp,       // 18. 강타자 (기본 공증)
    AttackSpeedUp,  // 19. 가속
    RecoveryUp,      // 20. 비상식량 (회복량 증가)
    Drive // 13. 드라이브 (이속 유틸) - MoveSpeedUp과 유사할 수 있음
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public ItemRarity rarity;
    public ItemEffectType effectType;
    
    [Tooltip("중첩당 증가하는 수치 (예: 10% -> 0.1, 공격력 +5 -> 5)")]
    public float valuePerStack; 
}
