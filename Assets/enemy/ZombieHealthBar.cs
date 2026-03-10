using UnityEngine;
using UnityEngine.UI;

public class ZombieHealthBar : MonoBehaviour
{
    private zombie targetZombie;
    private Image fillImage;

    void Start()
    {
        targetZombie = GetComponentInParent<zombie>();
        fillImage = GetComponent<Image>();
    }

    void LateUpdate()
    {
        if (targetZombie == null || fillImage == null) return;

        fillImage.fillAmount = targetZombie.GetHpRatio();
    }
}