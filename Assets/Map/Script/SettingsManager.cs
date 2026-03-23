using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public Toggle fullscreenToggle; // 에디터에서 Toggle UI 연결

    void Start()
    {
        Time.timeScale = 1f;

        // 게임 시작 시 현재 전체화면 상태를 Toggle에 반영
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;

            // 리스너 등록
            fullscreenToggle.onValueChanged.AddListener(SetFullscreenMode);
        }
    }

    // Toggle이 바뀔 때 자동 호출
    public void SetFullscreenMode(bool isFullscreen)
    {
        if (isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
        }
    }

    public void ToggleSettings()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-12");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void SettingFalse()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-12");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        if (EscMenuManager.Instance != null)
        {
            EscMenuManager.Instance.ForceCloseMenu();
        }
    }

    public void RestartAndClose()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-12");
        Time.timeScale = 1f;
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-12");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        // 리스너 메모리 누수 방지
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(SetFullscreenMode);
        }
    }
}