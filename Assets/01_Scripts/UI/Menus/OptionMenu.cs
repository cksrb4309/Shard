using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class OptionMenu : MonoBehaviour
{
    public GameObject soundMenuPanel;
    public GameObject inputKeySettingMenu;

    public Slider allVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    public InputActionReference cancelActionReference = null;

    public GameObject optionPanel;

    private void Start()
    {
        allVolumeSlider.value = SoundManager.GetALLVolume();
        bgmVolumeSlider.value = SoundManager.GetBGMVolume();
        sfxVolumeSlider.value = SoundManager.GetSFXVolume();

        if (SceneManager.GetActiveScene().name == "Game") 
        {
            cancelActionReference.action.Enable();
        }
    }
    public void GameExit()
    {
        GameManager.PlayTime();

        ScreenTransition.Play("Default_FadeOut", "Default_FadeIn", Color.black, Color.black, "Title", 0, 1);

        optionPanel.SetActive(false);

        enabled = false;

        gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            cancelActionReference.action.Disable();
        }
    }
    private void Update()
    {
        if (cancelActionReference == null) return;

        if (cancelActionReference.action.WasPressedThisFrame())
        {
            optionPanel.SetActive(!optionPanel.activeSelf);

            if (optionPanel.activeSelf == true)
            {
                GameManager.StopTime();
            }
            else
            {
                GameManager.PlayTime();
            }
        }

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
