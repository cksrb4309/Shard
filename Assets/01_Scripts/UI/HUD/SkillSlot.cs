using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    public Slider slider;

    public Image cooltimeFillImage;

    public TMP_Text countText;
    public TMP_Text cooltimeText;

    public Image handleImage;

    public Image skillIcon;
    public void SetFillAmount(float fillAmount)
    {
        slider.value = 1 - fillAmount;
    }
    public void SetCountText(int count)
    {
        if (count != 0) countText.text = count.ToString();

        else countText.text = string.Empty;
    }
    public void SetCooltimeText(int cooltime)
    {
        cooltimeText.text = cooltime.ToString();
    }
    public void ClearCooltimeText()
    {
        cooltimeText.text = string.Empty;
    }
    public void ClearCountText()
    {
        countText.text = string.Empty;
    }
    public void EnableFillAmount()
    {
        cooltimeFillImage.enabled = true;
    }
    public void DisableFillAmount()
    {
        cooltimeFillImage.enabled = false;
    }
    public void EnableHandle()
    {
        handleImage.enabled = true;
    }
    public void DisableHandle()
    {
        handleImage.enabled = false;
    }
    public void SetSkillIcon(Sprite icon)
    {
        skillIcon.sprite = icon;
    }
}