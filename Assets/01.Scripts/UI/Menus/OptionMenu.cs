using UnityEngine;

public class OptionMenu : MonoBehaviour
{
    public GameObject soundMenuPanel;
    public GameObject inputKeySettingMenu;
    public void OnSoundMenuPanel()
    {
        inputKeySettingMenu.SetActive(false);
        soundMenuPanel.SetActive(true);
    }
    public void OnInputKeySettingMenu()
    {
        soundMenuPanel.SetActive(false);
        inputKeySettingMenu.SetActive(true);
    }
}
