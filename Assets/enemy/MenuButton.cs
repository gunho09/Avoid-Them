using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public void GoToMenu()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-12");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI");
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
}
