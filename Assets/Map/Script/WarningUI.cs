using UnityEngine;
using TMPro;
using System.Collections;

public class WarningUI : MonoBehaviour
{
    public static WarningUI Instance;

    public TextMeshProUGUI warningText;
    public float displayDuration = 2.0f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작 시 텍스트 숨기기
        if (warningText != null) warningText.alpha = 0f;
    }

    public void ShowWarning(string message = "")
    {
        Debug.Log("WarningUI: ShowWarning 호출됨");
        if (warningText == null) return;

        // 메시지가 입력되었을 때만 텍스트 변경 (빈 값이면 기존 Inspector 텍스트 유지)
        if (!string.IsNullOrEmpty(message))
        {
            warningText.text = message;
        }
        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        // 1. 나타나기 (0.2초)
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            warningText.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.2f);
            yield return null;
        }
        warningText.alpha = 1f;

        // 2. 유지하기
        yield return new WaitForSeconds(displayDuration);

        // 3. 사라지기 (0.5초)
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            warningText.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.5f);
            yield return null;
        }
        warningText.alpha = 0f;
    }
}
