using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public void GoToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI");
    }
}
