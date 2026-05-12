using System;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Runtime.Ui;

namespace UnityCliTools.Infrastructure.Ui
{

public static class UiThemeToolUtility
{
    public static bool TryResolveTheme(JObject parameters, out UiTheme theme, out string path, out string error)
    {
        theme = null;
        path = string.Empty;
        error = string.Empty;
        var p = new ToolParams(parameters);
        var guid = p.Get("themeGuid", string.Empty)?.Trim();
        path = UnityCliToolShared.TryConvertToAssetPath(p.Get("themePath", string.Empty));

        if (!string.IsNullOrWhiteSpace(guid))
        {
            path = AssetDatabase.GUIDToAssetPath(guid);
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            error = "themePath or themeGuid is required.";
            return false;
        }

        theme = AssetDatabase.LoadAssetAtPath<UiTheme>(path);
        if (theme == null)
        {
            error = "UiTheme asset could not be resolved.";
            return false;
        }

        return true;
    }

    public static bool TryReadRole(string raw, out UiThemeRole role, out string error)
    {
        role = UiThemeRole.None;
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        var normalized = raw.Trim().Replace("-", "_");
        foreach (UiThemeRole candidate in Enum.GetValues(typeof(UiThemeRole)))
        {
            if (string.Equals(candidate.ToString(), normalized, StringComparison.OrdinalIgnoreCase)
                || string.Equals(ToSnakeCase(candidate.ToString()), normalized, StringComparison.OrdinalIgnoreCase))
            {
                role = candidate;
                return true;
            }
        }

        error = "Unsupported theme role.";
        return false;
    }

    public static UiThemeBinding AddOrUpdateBinding(
        GameObject target,
        UiThemeRole role,
        bool applyGraphic,
        bool applyText,
        bool applyButton,
        bool applySprite,
        bool applyFont,
        bool applyFontSize)
    {
        var binding = target.GetComponent<UiThemeBinding>();
        if (binding == null)
        {
            binding = Undo.AddComponent<UiThemeBinding>(target);
        }
        else
        {
            Undo.RecordObject(binding, "Bind UI Theme Element");
        }

        binding.Configure(role, applyGraphic, applyText, applyButton, applySprite, applyFont, applyFontSize);
        EditorUtility.SetDirty(binding);
        return binding;
    }

    public static bool HasApplicableTarget(GameObject target)
    {
        return target.GetComponent<TMP_Text>() != null
            || target.GetComponent<Graphic>() != null
            || target.GetComponent<Button>() != null
            || target.GetComponent<Selectable>() != null
            || target.GetComponent<Image>() != null;
    }

    public static object BindingSnapshot(UiThemeBinding binding)
    {
        if (binding == null)
        {
            return null;
        }

        return new
        {
            role = binding.Role.ToString(),
            gameObject = UiToolUtility.ElementSnapshot(binding.gameObject)
        };
    }

    public static string ResolveToken(string token)
    {
        var normalized = (token ?? string.Empty).Trim().Replace("-", "_");
        switch (normalized.ToLowerInvariant())
        {
            case "background":
            case "background_color":
                return "backgroundColor";
            case "panel":
            case "panel_color":
                return "panelColor";
            case "primary":
            case "primary_color":
                return "primaryColor";
            case "secondary":
            case "secondary_color":
                return "secondaryColor";
            case "danger":
            case "danger_color":
                return "dangerColor";
            case "text":
            case "text_color":
                return "textColor";
            case "muted_text":
            case "muted_text_color":
                return "mutedTextColor";
            case "button_text":
            case "button_text_color":
                return "buttonTextColor";
            case "button_highlighted":
            case "button_highlighted_color":
                return "buttonHighlightedColor";
            case "button_pressed":
            case "button_pressed_color":
                return "buttonPressedColor";
            case "button_disabled":
            case "button_disabled_color":
                return "buttonDisabledColor";
            case "title_font":
                return "titleFont";
            case "body_font":
                return "bodyFont";
            case "button_font":
                return "buttonFont";
            case "title_size":
            case "title_font_size":
                return "titleFontSize";
            case "body_size":
            case "body_font_size":
                return "bodyFontSize";
            case "button_size":
            case "button_font_size":
                return "buttonFontSize";
            case "panel_sprite":
                return "panelSprite";
            case "button_sprite":
                return "buttonSprite";
            case "icon_sprite":
                return "iconSprite";
            default:
                return token;
        }
    }

    public static object SerializedPropertySnapshot(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Color:
                return UiToolUtility.ColorSnapshot(property.colorValue);
            case SerializedPropertyType.Float:
                return property.floatValue;
            case SerializedPropertyType.Integer:
                return property.intValue;
            case SerializedPropertyType.Boolean:
                return property.boolValue;
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.Enum:
                return property.enumDisplayNames[property.enumValueIndex];
            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue == null
                    ? null
                    : new
                    {
                        name = property.objectReferenceValue.name,
                        type = property.objectReferenceValue.GetType().FullName,
                        assetPath = AssetDatabase.GetAssetPath(property.objectReferenceValue)
                    };
            default:
                return property.propertyType.ToString();
        }
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var result = string.Empty;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsUpper(c) && i > 0)
            {
                result += "_";
            }

            result += char.ToLowerInvariant(c);
        }

        return result;
    }
}
}
