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
        }
    }

        public void TakeDamage(float damage)
        {
            if (parentObject != null)
            {
                parentObject.TakeDamage(damage);
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
