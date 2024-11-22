using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    public Image cooltimeFillImage;
    public TMP_Text countText;
    public TMP_Text cooltimeText;

    public void SetFillAmount(float fillAmount)
    {
        cooltimeFillImage.fillAmount = fillAmount;
    }
    public void SetCountText(int count)
    {
        if (count != 0) countText.text = count.ToString();
        else countText.text = string.Empty;
    }
}