using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem.iOS;
public class HitBox : MonoBehaviour, IDamageable
#pragma warning restore format
{
    private IDamageable parentObject;

    void Start()
    {
        if (transform.parent != null)
        {
            parentObject = transform.parent.GetComponentInParent<IDamageable>();
            if (parentObject != null)
            {
                Debug.Log($"[HitBox] 나랑 연결된 진짜 보스 찾음: {((MonoBehaviour)parentObject).gameObject.name} ! 야호!");
            }
            else
            {
                Debug.LogWarning("[HitBox] 에러! 내 부모 중에 보스(IDamageable)가 안 보여 ㅠㅠ");
            }
        }
    }

        public void TakeDamage(float damage)
        {
            Debug.Log($"[HitBox 탐정] 칼에 찔렸음! 데미지: {damage}");
            if (parentObject != null)
            {
                parentObject.TakeDamage(damage);
                Debug.Log("[HitBox] 부모에게 전달 완료!");
            }
        }

        public float GetHpRatio()
        {
            if (parentObject != null)
            {
                return parentObject.GetHpRatio();
            }
            return 1f;
        }
}
