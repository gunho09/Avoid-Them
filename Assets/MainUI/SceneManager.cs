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
        UnityEngine.SceneManagement.SceneManager.LoadScene("HallWay");
    }
    
    public void GameExit()
    {
        Application.Quit();
    }

}
