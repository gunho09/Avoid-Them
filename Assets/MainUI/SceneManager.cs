using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneManager : MonoBehaviour
{

    public GameObject SettingMenu;
    
    public void Setting()
    {
        SettingMenu.SetActive(true);
    }

    public void SettingClose()
    {
        SettingMenu.SetActive(false);
    }

    public void GameStart()
    {
        // 씬 전환 시 시간 정지가 유지될 수 있으므로 초기화
        Time.timeScale = 1f;

        // "Game" 씬이 Build Settings에 있는지 확인하는 것이 좋지만, 
        // 런타임에는 이름으로 로드합니다.
        try
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene 'Game': {e.Message}. Make sure it is added to Build Settings.");
        }
    }
    
    public void GameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PracticeRoom()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("practiceRoom");
    }



    private void Start()
    {
        // 메인 메뉴 진입 시 BGM 재생 (1-4: 메인 테마)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM("1-4");
        }
        else
        {
            Debug.LogError("SceneManager: SoundManager.Instance가 NULL입니다! MainUI 씬에 SoundManager 프리팹이 있는지 확인하세요.");
        }
    }
}
