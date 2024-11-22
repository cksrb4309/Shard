using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    public GameObject optionPanel;

    public void OnShowOptionPanel()
    {
        optionPanel.SetActive(true);
    }
    public void OnHideOptionPanel()
    {
        optionPanel.SetActive(false);
    }
    public void OnGameStart()
    {
        SceneManager.LoadScene("Game");
    }
    public void OnQuitGame()
    {
        Application.Quit();
    }
}