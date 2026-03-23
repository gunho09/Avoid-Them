using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // 코루틴을 위해 추가

using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class EscMenuManager : MonoBehaviour
{
    public static EscMenuManager Instance;

    [Header("연결할 설정창 패널")]
    public GameObject settingsPanel;

    [Header("사운드 슬라이더 연결")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    private bool isOpened = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            UnitySceneManager.sceneLoaded += OnSceneLoaded;

            // [수정] 즉시 찾지 않고, 0.1초 뒤에 찾도록 코루틴 실행
            if (UnitySceneManager.GetActiveScene().buildIndex == 0)
            {
                StartCoroutine(WaitAndConnectButton());
            }
        }
        else
        {
            // [중요] 메인메뉴에서 넘어온 기존 Instance가 있다면, 
            // 현재 씬(Game 등)에 새로 생성된 EscMenuManager의 UI 연결 정보들을 기존 Instance에 넘겨줍니다.
            // 안 그러면 기존 Instance의 settingsPanel이 파괴된 객체를 가리켜 작동하지 않습니다.
            if (this.settingsPanel != null) Instance.settingsPanel = this.settingsPanel;
            if (this.bgmSlider != null) Instance.bgmSlider = this.bgmSlider;
            if (this.sfxSlider != null) Instance.sfxSlider = this.sfxSlider;
            
            Instance.isOpened = false;
            // 게임 씬 시작 시 패널이 떠있지 않도록 확실히 닫음 처리
            if (Instance.settingsPanel != null) Instance.settingsPanel.SetActive(false);

            Destroy(gameObject);
            return;
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    void Update()
    {
        // 게임 도중(buildIndex != 0)일 때는 PlayerControler가 ESC를 대신 감지해주므로
        // 메인 로비(buildIndex == 0)에서 설정창이 열려있을 때 닫는 용도로만 사용합니다.
        if (UnitySceneManager.GetActiveScene().buildIndex == 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenu();
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            // 씬이 바뀔 때도 안전하게 코루틴으로 연결
            StartCoroutine(WaitAndConnectButton());
        }
    }

    // 0.1초 기다린 후 버튼을 찾는 함수
    IEnumerator WaitAndConnectButton()
    {
        // 시간을 멈춘 상태에서도 작동하도록 Realtime 사용
        yield return new WaitForSecondsRealtime(0.1f);

        GameObject settingBtnObj = GameObject.Find("Setting");
        if (settingBtnObj != null)
        {
            Button btn = settingBtnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ToggleMenu);
            }
        }
        else
        {
            Debug.LogWarning("Setting 버튼을 찾지 못했습니다. 오브젝트 이름을 확인하세요.");
        }
    }

    public void ToggleMenu()
    {
        if (settingsPanel == null) return;

        isOpened = !isOpened;
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-12"); // 버튼 누르는 소리
        settingsPanel.SetActive(isOpened);

        if (isOpened)
        {
            UpdateSliderValues(); // 메뉴 열 때 슬라이더 값 동기화
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
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-12"); // 버튼 누르는 소리
        isOpened = false;
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        UnitySceneManager.LoadScene("MainUI");
    }

    private void UpdateSliderValues()
    {
        if (SoundManager.Instance == null) return;

        if (bgmSlider != null)
        {
            bgmSlider.value = SoundManager.Instance.BGMVolume;
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = SoundManager.Instance.SFXVolume;
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.BGMVolume = volume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SFXVolume = volume;
        }
    }

    public void ForceCloseMenu()
    {
        isOpened = false;
        if (settingsPanel != null) settingsPanel.SetActive(false);

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