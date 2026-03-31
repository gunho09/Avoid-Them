using UnityEngine;
using System.Collections;
public class HitBox : MonoBehaviour
{
    private IDamageable parentObject;

        void Start()
        {
            parentObject = GetComponentInParent<IDamageable>();
        }

        public void TakeDamage(float damage)
        {
            Debug.Log($"[HitBox] 맞음! 데미지: {damage}");
            if (parentObject != null)
            {
                parentObject.TakeDamage(damage);
                Debug.Log("[HitBox] 부모에게 전달 완료!");
            }
        }

}
