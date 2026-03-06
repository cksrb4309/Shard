using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelectMenuUI : MonoBehaviour
{
    public PlayModeSelectUI playModeSelectUI;

    public ShipSelectOption[] shipSelectOptions;

    public Image shipImage;
    public TMP_Text shipNameText;
    public TMP_Text shipDescriptionText;

    public int beforeShowShipId = 0;

    bool isSceneLoading = false;

    public Image colorImage;

    public Slider color_R_Slider;
    public Slider color_G_Slider;
    public Slider color_B_Slider;

    public TMP_Text colorName_Text;

    public TMP_Text[] colorValues;

    public GameObject shipSelectPanelGroup;
    public GameObject colorSelectPanelGroup;

    public TitleCursor titleCursor = null;

    string colorName = "Jet";

    private void Start()
    {
        shipImage.sprite = shipSelectOptions[beforeShowShipId].shipImage;
        shipNameText.text = shipSelectOptions[beforeShowShipId].shipName;
        shipDescriptionText.text = shipSelectOptions[beforeShowShipId].shipDescriptionText;

        OnColorPanel(colorName);
    }

    public void SelectShip(int shipID)
    {
        if (shipID == beforeShowShipId) return;

        beforeShowShipId = shipID;

        shipImage.sprite = shipSelectOptions[shipID].shipImage;
        shipNameText.text = shipSelectOptions[shipID].shipName;
        shipDescriptionText.text = shipSelectOptions[shipID].shipDescriptionText;
    }
    public void OnShipSelectPanel()
    {
        shipSelectPanelGroup.SetActive(true);
        colorSelectPanelGroup.SetActive(false);
    }
    public void OnColorSelectPanel()
    {
        colorSelectPanelGroup.SetActive(true);
        shipSelectPanelGroup.SetActive(false);
    }
    public void SelectColor()
    {
        PlayerPrefs.SetFloat(colorName + 'r', colorImage.color.r);
        PlayerPrefs.SetFloat(colorName + 'g', colorImage.color.g);
        PlayerPrefs.SetFloat(colorName + 'b', colorImage.color.b);
    }
    public void OnColorPanel(string colorName)
    {
        this.colorName = colorName;

        Color color = Color.white;

        color.r = PlayerPrefs.GetFloat(colorName + 'r', 1f);
        color.g = PlayerPrefs.GetFloat(colorName + 'g', 1f);
        color.b = PlayerPrefs.GetFloat(colorName + 'b', 1f);

        colorImage.color = color;

        color_R_Slider.value = (int)(color.r * 255);
        color_G_Slider.value = (int)(color.g * 255);
        color_B_Slider.value = (int)(color.b * 255);

        colorValues[0].text = color_R_Slider.value.ToString();
        colorValues[1].text = color_G_Slider.value.ToString();
        colorValues[2].text = color_B_Slider.value.ToString();

        switch (colorName)
        {
            case "Cursor":
                colorName_Text.text = "커서 색상"; break;
            case "Jet":
                colorName_Text.text = "전투기 외곽선 색상"; break;
            case "AttackCursor":
                colorName_Text.text = "공격 커서 색상"; break;
            case "AttackBackgroundCursor":
                colorName_Text.text = "공격 배경 커서 색상"; break;
        }
    }
    public void ColorChange(string c)
    {
        Color color = Color.white;

        color.r = color_R_Slider.value / 255f;
        color.g = color_G_Slider.value / 255f;
        color.b = color_B_Slider.value / 255f;

        colorImage.color = color;

        if (c == "r") colorValues[0].text = color_R_Slider.value.ToString();
        else if (c == "g") colorValues[1].text = color_G_Slider.value.ToString();
        else if (c == "b") colorValues[2].text = color_B_Slider.value.ToString();

        if (colorName == "Cursor")
        {
            titleCursor?.SetCursorColor(color);
        }
    }
    public void Play()
    {
        if (isSceneLoading) return;

        isSceneLoading = true;

        playModeSelectUI.SetShipID(shipSelectOptions[beforeShowShipId].shipID);
    }
    public void Cancel()
    {
        if (isSceneLoading) return;

        playModeSelectUI.Cancel();

        transform.gameObject.SetActive(false);
    }
}
