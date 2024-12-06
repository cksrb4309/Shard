using UnityEngine;
using UnityEngine.UI;
public class OptionMenu : MonoBehaviour
{
    public GameObject soundMenuPanel;
    public GameObject inputKeySettingMenu;

    public Slider allVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    private void Start()
    {
        allVolumeSlider.value = SoundManager.GetALLVolume();
        bgmVolumeSlider.value = SoundManager.GetBGMVolume();
        sfxVolumeSlider.value = SoundManager.GetSFXVolume();
    }
    public void ALL_Volume_Change()
    {
        SoundManager.SetAllVolume(allVolumeSlider.value);
    }
    public void BGM_Volume_Change()
    {
        SoundManager.SetBGMVolume(bgmVolumeSlider.value);
    }
    public void SFX_Volume_Change()
    {
        SoundManager.SetSFXVolume(sfxVolumeSlider.value);
    }
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
