using UnityEngine;

public class HitBox : MonoBehaviour
{
    private zombie parentZombie;

        void Start()
        {
            parentZombie = GetComponentInParent<zombie>();
        }

        public void TakeDamage(float damage)
        {
            if (parentZombie != null)
            {
                parentZombie.TakeDamage(damage);
            }
        }
}
