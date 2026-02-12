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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
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
                Debug.Log("톱니바퀴 버튼 연결 성공!");
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
        settingsPanel.SetActive(isOpened);

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
        isOpened = false;
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        UnitySceneManager.LoadScene("MainUI");
    }

    private void UpdateSliderValues() { }
    public void SetBGMVolume(float volume) { }
    public void SetSFXVolume(float volume) { }
}