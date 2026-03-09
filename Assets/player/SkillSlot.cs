using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlot : MonoBehaviour
{
    [Header("UI Elements")]
    public Image skillIcon;          // 메인 스킬 아이콘
    public Image cooldownOverlay;    // 쿨타임 시 채워지는 이미지 (Radial Fill)
    public TextMeshProUGUI cooldownText; // 남은 쿨타임 숫자
    public GameObject activeEffect;  // 스킬 활성화 중 효과 (선택 사항)

    [Header("Settings")]
    public bool hideTextWhenReady = true;

    /// <summary>
    /// 스킬 UI 상태를 업데이트합니다.
    /// </summary>
    /// <param name="currentCooldown">현재 남은 쿨타임</param>
    /// <param name="maxCooldown">최대 쿨타임</param>
    /// <param name="isActive">스킬이 현재 활성화(지속) 중인지 여부</param>
    public void UpdateUI(float currentCooldown, float maxCooldown, bool isActive = false)
    {
        if (cooldownOverlay != null)
        {
            // 쿨타임 비율 계산 (0~1)
            float fillAmount = maxCooldown > 0 ? Mathf.Clamp01(currentCooldown / maxCooldown) : 0;
            cooldownOverlay.fillAmount = fillAmount;
        }

        if (cooldownText != null)
        {
            if (currentCooldown > 0)
            {
                cooldownText.gameObject.SetActive(true);
                // 1초 이상이면 정수로, 1초 미만이면 소수점 첫째자리까지 표시
                cooldownText.text = currentCooldown >= 1f ? 
                    Mathf.CeilToInt(currentCooldown).ToString() : 
                    currentCooldown.ToString("F1");
            }
            else
            {
                if (hideTextWhenReady) cooldownText.gameObject.SetActive(false);
                else cooldownText.text = "";
            }
        }

        if (activeEffect != null)
        {
            activeEffect.SetActive(isActive);
        }

        // 스킬이 사용 가능할 때 아이콘을 밝게, 아닐 때 약간 어둡게 처리할 수 있습니다.
        if (skillIcon != null)
        {
            skillIcon.color = (currentCooldown <= 0) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
}
