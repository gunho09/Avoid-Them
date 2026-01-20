using UnityEngine;
using UnityEditor;
using System.IO;

public class ItemGenerator
{
#if UNITY_EDITOR
    [MenuItem("Tools/Generate All Items")]
    public static void GenerateItems()
    {
        string path = "Assets/Item/Data";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        CreateItem(path, "Vampirism", "흡혈", "공격 시 피해량의 일부를 회복합니다.", ItemRarity.Legend, ItemEffectType.Vampirism, 0.1f); // 10%
        CreateItem(path, "DamageAmplification", "주문의 검", "모든 피해량이 증가합니다.", ItemRarity.Rare, ItemEffectType.DamageAmplification, 0.2f); // 20%
        CreateItem(path, "MoveSpeedUp", "에너지드링크", "부스트 사용 시 쉴드를 획득합니다.", ItemRarity.Normal, ItemEffectType.MoveSpeedUp, 0.2f); // 20% Shield
        CreateItem(path, "ConditionalAttackUp", "부서진 배트", "잃은 체력에 비례해 공격력이 증가합니다.", ItemRarity.Rare, ItemEffectType.ConditionalAttackUp, 1.0f); // 100% (Ratio constant)
        CreateItem(path, "MaxHpShield", "두꺼운 과잠", "최대 체력 비례 쉴드를 상시 획득합니다.", ItemRarity.Rare, ItemEffectType.MaxHpShield, 0.3f); // 30% Shield
        CreateItem(path, "ConditionalDamageUp", "정밀 타격", "체력이 80% 이상인 적에게 더 큰 피해를 줍니다.", ItemRarity.Normal, ItemEffectType.ConditionalDamageUp, 0.5f); // 50% Dmg
        CreateItem(path, "DamageReduction", "자기장", "주기적으로 피해를 막아주는 자기장을 생성합니다.", ItemRarity.Legend, ItemEffectType.DamageReduction, 1.0f); // Stack (Bool)
        CreateItem(path, "Reflection", "반사", "주기적으로 피해를 무시하고 반사합니다.", ItemRarity.Legend, ItemEffectType.Reflection, 1.0f); // Stack (Bool)
        CreateItem(path, "AggroDistribution", "더미", "피격 시 확률적으로 더미를 소환합니다.", ItemRarity.Rare, ItemEffectType.AggroDistribution, 1.0f); // Stack (Bool)
        CreateItem(path, "DoubleAttack", "학살자", "3타마다 피해량이 2배가 됩니다.", ItemRarity.Legend, ItemEffectType.DoubleAttack, 1.0f); // Stack (Bool)
        CreateItem(path, "Knockback", "충격파", "주기적으로 주변 적을 기절시킵니다.", ItemRarity.Rare, ItemEffectType.Knockback, 1.0f); // Stack
        CreateItem(path, "Resurrection", "밴드", "사망 시 1회 생존합니다 (소모품).", ItemRarity.Normal, ItemEffectType.Resurrection, 1.0f); // Stack
        CreateItem(path, "DashAttackUp", "속공", "대시 공격 시 피해량이 증가합니다.", ItemRarity.Normal, ItemEffectType.DashAttackUp, 0.2f); // 20%
        CreateItem(path, "InstantKill", "급사", "공격 시 일정 확률로 적을 즉사시킵니다.", ItemRarity.Legend, ItemEffectType.InstantKill, 1.0f); // Stack (Bool)
        CreateItem(path, "GlassCannon", "유리 글로브", "최대 체력이 줄어들지만 공격력이 대폭 증가합니다.", ItemRarity.Rare, ItemEffectType.GlassCannon, 1.0f); // Ratio
        CreateItem(path, "CooldownReduction", "감소", "스킬 쿨타임이 감소합니다.", ItemRarity.Normal, ItemEffectType.CooldownReduction, 0.1f); // 10%
        CreateItem(path, "AttackUp", "강타자", "기본 공격력이 증가합니다.", ItemRarity.Normal, ItemEffectType.AttackUp, 0.1f); // 10%
        CreateItem(path, "AttackSpeedUp", "가속", "공격 속도(쿨타임)가 빨라집니다.", ItemRarity.Normal, ItemEffectType.AttackSpeedUp, 0.1f); // 10%
        CreateItem(path, "RecoveryUp", "비상식량", "체력이 낮을 때 자동 사용되어 회복합니다 (소모품).", ItemRarity.Normal, ItemEffectType.RecoveryUp, 1.0f); // Stack
        CreateItem(path, "Drive", "드라이브", "연속 타격 시 이동속도가 증가합니다.", ItemRarity.Normal, ItemEffectType.Drive, 2.0f); // Speed Value

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("20 Items Generated Successfully!");
    }

    static void CreateItem(string path, string fileName, string itemName, string desc, ItemRarity rarity, ItemEffectType type, float value)
    {
        string assetPath = $"{path}/{fileName}.asset";
        ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
        
        if (item == null)
        {
            item = ScriptableObject.CreateInstance<ItemData>();
            AssetDatabase.CreateAsset(item, assetPath);
        }

        item.itemName = itemName;
        item.description = desc;
        item.rarity = rarity;
        item.effectType = type;
        item.valuePerStack = value;
        
        EditorUtility.SetDirty(item);
    }
#endif
}
