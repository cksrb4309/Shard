using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCliTools.Runtime.Ui
{

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class UiThemeBinding : MonoBehaviour
{
    [SerializeField] private UiThemeRole role = UiThemeRole.None;
    [SerializeField] private bool applyGraphic = true;
    [SerializeField] private bool applyText = true;
    [SerializeField] private bool applyButton = true;
    [SerializeField] private bool applySprite = true;
    [SerializeField] private bool applyFont = true;
    [SerializeField] private bool applyFontSize = true;

    public UiThemeRole Role
    {
        get { return role; }
        set { role = value; }
    }

    public void Configure(UiThemeRole nextRole, bool graphic, bool text, bool button, bool sprite, bool font, bool fontSize)
    {
        role = nextRole;
        applyGraphic = graphic;
        applyText = text;
        applyButton = button;
        applySprite = sprite;
        applyFont = font;
        applyFontSize = fontSize;
    }

    public void Apply(UiTheme theme)
    {
        if (theme == null || role == UiThemeRole.None)
        {
            return;
        }

        var tmp = GetComponent<TMP_Text>();
        if (tmp != null)
        {
            if (applyText)
            {
                tmp.color = theme.GetGraphicColor(role);
            }

            var font = theme.GetFont(role);
            if (applyFont && font != null)
            {
                tmp.font = font;
            }

            if (applyFontSize)
            {
                tmp.fontSize = theme.GetFontSize(role);
            }
        }

        var graphic = GetComponent<Graphic>();
        if (graphic != null && applyGraphic && tmp == null)
        {
            graphic.color = theme.GetGraphicColor(role);
        }

        var image = GetComponent<Image>();
        if (image != null && applySprite)
        {
            var sprite = theme.GetSprite(role);
            if (sprite != null)
            {
                image.sprite = sprite;
            }
        }

        var button = GetComponent<Button>();
        if (button != null && applyButton)
        {
            button.colors = theme.BuildButtonColors(role, button.colors);
            ApplyButtonText(theme, transform);
        }
    }

    private void ApplyButtonText(UiTheme theme, Transform root)
    {
        var texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (var i = 0; i < texts.Length; i++)
        {
            var text = texts[i];
            if (text == null)
            {
                continue;
            }

            text.color = theme.buttonTextColor;
            var font = theme.GetFont(role);
            if (font != null)
            {
                text.font = font;
            }

            text.fontSize = theme.GetFontSize(role);
        }
    }
}
}
