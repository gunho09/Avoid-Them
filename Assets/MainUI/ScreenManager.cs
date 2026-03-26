using UnityEngine;
using UnityEngine.UI;

public class ScreenModeManager : MonoBehaviour
{
    public static ScreenModeManager Instance;

    [Header("전체화면 토글 버튼")]
    public Button screenToggleButton;

    [Header("버튼 텍스트 (선택)")]
    public Text buttonText; // 없으면 무시됨

    // 창모드 기본 해상도
    private const int WINDOW_WIDTH = 1920;
    private const int WINDOW_HEIGHT = 1080;

    private const string PREF_KEY = "ScreenMode_Save"; // 0 = 창모드, 1 = 전체화면

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // EscMenuManager와 동일하게 Duplicate 처리
            if (this.screenToggleButton != null) Instance.screenToggleButton = this.screenToggleButton;
            if (this.buttonText != null) Instance.buttonText = this.buttonText;
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 버튼 리스너 등록
        if (screenToggleButton != null)
        {
            screenToggleButton.onClick.RemoveAllListeners();
            screenToggleButton.onClick.AddListener(ToggleScreenMode);
        }

        // 저장된 설정 불러와서 적용
        bool savedFullscreen = PlayerPrefs.GetInt(PREF_KEY, 1) == 1; // 기본값: 전체화면
        ApplyScreenMode(savedFullscreen);
    }

    void OnDestroy()
    {
        if (screenToggleButton != null)
            screenToggleButton.onClick.RemoveListener(ToggleScreenMode);
    }

    public void ToggleScreenMode()
    {
        // 현재 상태 반전
        ApplyScreenMode(!Screen.fullScreen);
    }

    private void ApplyScreenMode(bool fullscreen)
    {
        if (fullscreen)
        {
            // 전체화면: 현재 모니터의 최대 해상도로 설정
            Screen.SetResolution(
                Display.main.systemWidth,
                Display.main.systemHeight,
                FullScreenMode.FullScreenWindow
            );
        }
        else
        {
            // 창모드: 지정된 해상도로 설정
            Screen.SetResolution(WINDOW_WIDTH, WINDOW_HEIGHT, FullScreenMode.Windowed);
        }

        // 저장
        PlayerPrefs.SetInt(PREF_KEY, fullscreen ? 1 : 0);
        PlayerPrefs.Save();

        // 버튼 텍스트 갱신
        UpdateButtonText(fullscreen);
    }

    private void UpdateButtonText(bool isFullscreen)
    {
        if (buttonText == null) return;
        buttonText.text = isFullscreen ? "창모드로 전환" : "전체화면으로 전환";
    }
}