using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCliTools.Runtime.Ui
{

[CreateAssetMenu(menuName = "Unity CLI Tools/UI Theme", fileName = "UiTheme")]
public sealed class UiTheme : ScriptableObject
{
    public Color backgroundColor = new Color(0.04f, 0.05f, 0.06f, 1f);
    public Color panelColor = new Color(0.10f, 0.12f, 0.15f, 0.92f);
    public Color primaryColor = new Color(0.94f, 0.74f, 0.28f, 1f);
    public Color secondaryColor = new Color(0.28f, 0.45f, 0.62f, 1f);
    public Color dangerColor = new Color(0.82f, 0.22f, 0.18f, 1f);
    public Color textColor = new Color(0.96f, 0.96f, 0.92f, 1f);
    public Color mutedTextColor = new Color(0.70f, 0.74f, 0.78f, 1f);
    public Color buttonTextColor = new Color(0.04f, 0.05f, 0.06f, 1f);
    public Color buttonHighlightedColor = new Color(1.00f, 0.86f, 0.42f, 1f);
    public Color buttonPressedColor = new Color(0.78f, 0.58f, 0.18f, 1f);
    public Color buttonDisabledColor = new Color(0.42f, 0.42f, 0.42f, 0.55f);

    public TMP_FontAsset titleFont;
    public TMP_FontAsset bodyFont;
    public TMP_FontAsset buttonFont;
    public float titleFontSize = 48f;
    public float bodyFontSize = 32f;
    public float buttonFontSize = 34f;

    public Sprite panelSprite;
    public Sprite buttonSprite;
    public Sprite iconSprite;

    public Color GetGraphicColor(UiThemeRole role)
    {
        switch (role)
        {
            case UiThemeRole.Background:
                return backgroundColor;
            case UiThemeRole.Panel:
                return panelColor;
            case UiThemeRole.PrimaryButton:
                return primaryColor;
            case UiThemeRole.SecondaryButton:
                return secondaryColor;
            case UiThemeRole.DangerButton:
                return dangerColor;
            case UiThemeRole.MutedText:
                return mutedTextColor;
            case UiThemeRole.TitleText:
            case UiThemeRole.BodyText:
                return textColor;
            default:
                return Color.white;
        }
    }

    public TMP_FontAsset GetFont(UiThemeRole role)
    {
        if (role == UiThemeRole.TitleText && titleFont != null)
        {
            return titleFont;
        }

        if ((role == UiThemeRole.PrimaryButton || role == UiThemeRole.SecondaryButton || role == UiThemeRole.DangerButton) && buttonFont != null)
        {
            return buttonFont;
        }

        return bodyFont;
    }

    public float GetFontSize(UiThemeRole role)
    {
        if (role == UiThemeRole.TitleText)
        {
            return titleFontSize;
        }

        if (role == UiThemeRole.PrimaryButton || role == UiThemeRole.SecondaryButton || role == UiThemeRole.DangerButton)
        {
            return buttonFontSize;
        }

        return bodyFontSize;
    }

    public Sprite GetSprite(UiThemeRole role)
    {
        if (role == UiThemeRole.Panel)
        {
            return panelSprite;
        }

        if (role == UiThemeRole.PrimaryButton || role == UiThemeRole.SecondaryButton || role == UiThemeRole.DangerButton)
        {
            return buttonSprite;
        }

        if (role == UiThemeRole.Icon)
        {
            return iconSprite;
        }

        return null;
    }

    public ColorBlock BuildButtonColors(UiThemeRole role, ColorBlock baseColors)
    {
        var colors = baseColors;
        colors.normalColor = GetGraphicColor(role);
        colors.highlightedColor = buttonHighlightedColor;
        colors.pressedColor = buttonPressedColor;
        colors.selectedColor = buttonHighlightedColor;
        colors.disabledColor = buttonDisabledColor;
        return colors;
    }
}
}
