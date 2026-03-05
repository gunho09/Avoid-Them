using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    public Image fadeImage;
    public float defaultFadeDuration = 0.2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = Color.clear;
            fadeImage.raycastTarget = false;
        }
    }

    public void FadeOut(System.Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(1f, onComplete));
    }

    public void FadeIn(System.Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(0f, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, System.Action onComplete)
    {
        if (fadeImage == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        fadeImage.raycastTarget = true;
        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < defaultFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / defaultFadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
        if (targetAlpha <= 0) fadeImage.raycastTarget = false;

        onComplete?.Invoke();
    }
}
