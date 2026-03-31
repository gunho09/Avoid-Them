using UnityEngine;
using System.Collections;

public class PoisonStatus : MonoBehaviour
{
    private float damagePerTick;
    private int remainingTicks;
    private IDamageable target;

    public void ApplyPoison(float tickDamage, int durationSeconds)
    {
        damagePerTick = tickDamage;
        remainingTicks = durationSeconds;
        target = GetComponent<IDamageable>();

        // 기존에 돌고 있는 독이 있다면 멈추고 새로 시작 (중첩이 아닌 초기화 방식)
        StopAllCoroutines();
        if (remainingTicks > 0)
        {
            StartCoroutine(PoisonRoutine());
        }
    }

    private IEnumerator PoisonRoutine()
    {
        while (remainingTicks > 0)
        {
            yield return new WaitForSeconds(1.0f);
            
            if (target != null)
            {
                target.TakeDamage(damagePerTick);
                // 독 데미지 이펙트 (초록색 등) 추가 가능
            }
            
            remainingTicks--;
        }
        
        Destroy(this); // 독이 끝나면 컴포넌트 삭제
    }
}
