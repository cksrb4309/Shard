using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    public GameObject optionPanel;
    public GameObject modeSelectPanel;
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
        modeSelectPanel.SetActive(true);

        // ScreenTransition.Play("Cube_FadeOut", "Cube_FadeIn", Color.black, Color.white, "Game", 1.5f, 0);
    }
    public void LoadGameScene()
    {
        SceneManager.LoadScene("Game");
    }
    public void OnQuitGame()
    {
        Application.Quit();
    }
}