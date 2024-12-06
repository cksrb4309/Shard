using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    public GameObject optionPanel;

    bool isSceneLoading = false;
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
        if (isSceneLoading) return;

        ScreenTransition.Play("Cube_FadeOut", "Cube_FadeIn", Color.black, Color.white, "Game", 1.5f, 0);

        isSceneLoading = true;
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