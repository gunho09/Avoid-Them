using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveScene : MonoBehaviour
{
    
    public void GoMain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI");
    }

    public void Quit()
    {
        Application.Quit();
    }

}
