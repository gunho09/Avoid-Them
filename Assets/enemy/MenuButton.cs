using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public void GoToMenu()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("2-12");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI");
    }
}
