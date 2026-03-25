using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class EscMenuManager : MonoBehaviour
{
    public static EscMenuManager Instance;

    [Header("연결할 설정창 패널")]
    public GameObject settingsPanel;

    [Header("사운드 슬라이더 연결")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    // isOpened는 항상 SetPanelState()를 통해서만 변경
    private bool isOpened = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            UnitySceneManager.sceneLoaded += OnSceneLoaded;

            if (UnitySceneManager.GetActiveScene().buildIndex == 0)
            {
                StartCoroutine(WaitAndConnectButton());
            }
        }
        else
        {
            // [수정 3] settingsPanel이 this의 자식이면 Destroy 시 참조가 끊기므로
            // DontDestroyOnLoad된 Instance 쪽으로 부모를 옮겨서 살려둠
            if (this.settingsPanel != null)
            {
                this.settingsPanel.transform.SetParent(Instance.transform);
                Instance.settingsPanel = this.settingsPanel;
            }
            if (this.bgmSlider != null)
            {
                this.bgmSlider.transform.SetParent(Instance.transform);
                Instance.bgmSlider = this.bgmSlider;
            }
            if (this.sfxSlider != null)
            {
                this.sfxSlider.transform.SetParent(Instance.transform);
                Instance.sfxSlider = this.sfxSlider;
            }

            // [수정 5] 상태를 SetPanelState로 통일해서 닫기
            Instance.SetPanelState(false);

            Destroy(gameObject);
            return;
        }

        // 첫 생성 시에도 SetPanelState로 초기화
        SetPanelState(false);
    }

    void OnDestroy()
    {
        // [수정 2] 이벤트 구독 해제
        UnitySceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (UnitySceneManager.GetActiveScene().buildIndex == 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenu();
            }
        }
    }

    // [수정 1] OnSceneLoaded는 Instance에서만 실행되도록 보장
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (this != Instance) return; // Duplicate가 호출하는 경우 차단

        if (scene.buildIndex == 0)
        {
            StartCoroutine(WaitAndConnectButton());
        }
    }

    // [수정 4] 재시도 루프 + 타임아웃 적용
    IEnumerator WaitAndConnectButton()
    {
        GameObject settingBtnObj = null;
        float timeout = 3f;
        float elapsed = 0f;

        while (settingBtnObj == null && elapsed < timeout)
        {
            settingBtnObj = GameObject.Find("Setting");
            if (settingBtnObj == null)
            {
                elapsed += 0.1f;
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        if (settingBtnObj == null)
        {
            Debug.LogWarning("Setting 버튼을 찾지 못했습니다. 오브젝트 이름을 확인하세요.");
            yield break;
        }

        Button btn = settingBtnObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(ToggleMenu);
        }
    }

    // [수정 5] 패널 상태를 isOpened와 항상 동기화하는 단일 메서드
    private void SetPanelState(bool open)
    {
        isOpened = open;
        if (settingsPanel != null)
            settingsPanel.SetActive(isOpened);
    }

    public void ToggleMenu()
    {
        if (settingsPanel == null) return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX("2-12");

        SetPanelState(!isOpened); // [수정 5] SetPanelState로 통일

        if (isOpened)
        {
            UpdateSliderValues();
        }

        if (UnitySceneManager.GetActiveScene().buildIndex != 0)
        {
            Time.timeScale = isOpened ? 0f : 1f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        if (isOpened)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            if (UnitySceneManager.GetActiveScene().buildIndex == 0)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void GoToMainMenu()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX("2-12");

        SetPanelState(false); // [수정 5]
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        UnitySceneManager.LoadScene("MainUI");
    }

    private void UpdateSliderValues()
    {
        // [수정 6] SoundManager가 없으면 PlayerPrefs로 fallback
        float bgmVol = SoundManager.Instance != null
            ? SoundManager.Instance.BGMVolume
            : PlayerPrefs.GetFloat("BGM_Save", 0.75f);

        float sfxVol = SoundManager.Instance != null
            ? SoundManager.Instance.SFXVolume
            : PlayerPrefs.GetFloat("SFX_Save", 0.75f);

        if (bgmSlider != null)
        {
            bgmSlider.value = bgmVol;
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVol;
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.BGMVolume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SFXVolume = volume;
    }

    public void ForceCloseMenu()
    {
        SetPanelState(false); // [수정 5]

        if (UnitySceneManager.GetActiveScene().buildIndex != 0)
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}