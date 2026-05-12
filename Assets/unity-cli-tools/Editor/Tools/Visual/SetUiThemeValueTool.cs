using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.SetUiThemeValue,
    Description = "Set one serialized token value on a UiTheme asset.",
    Group = UnityCliToolGroups.Visual)]
public static class SetUiThemeValueTool
{
    public sealed class Parameters
    {
        [ToolParameter("UiTheme asset path.")]
        public string ThemePath { get; set; }

        [ToolParameter("UiTheme asset GUID.")]
        public string ThemeGuid { get; set; }

        [ToolParameter("Theme token or serialized field path.", Required = true)]
        public string Token { get; set; }

        [ToolParameter("Scalar/string value for float, int, bool, enum, or string fields.")]
        public string Value { get; set; }

        [ToolParameter("Color value as r,g,b,a.")]
        public string Color { get; set; }

        [ToolParameter("Object reference asset path for font/sprite fields.")]
        public string ReferenceAssetPath { get; set; }

        [ToolParameter("Object reference asset GUID for font/sprite fields.")]
        public string ReferenceAssetGuid { get; set; }

        [ToolParameter("Preview only without modifying the theme.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var token = p.Get("token", string.Empty)?.Trim();
            var dryRun = p.GetBool("dryRun", true);
            UnityCliToolShared.GuardRequired(token, nameof(token));

            if (!UiThemeToolUtility.TryResolveTheme(parameters, out var theme, out var themePath, out var error))
            {
                return ToolResult.Error(error);
            }

            var propertyPath = UiThemeToolUtility.ResolveToken(token);
            var serialized = new SerializedObject(theme);
            var property = serialized.FindProperty(propertyPath);
            if (property == null)
            {
                return ToolResult.Error("Theme token was not found.", new { token, propertyPath });
            }

            var before = Snapshot(property);
            var preview = SnapshotPlannedValue(parameters, p, property);
            if (!dryRun)
            {
                Undo.RecordObject(theme, "Set UI Theme Value");
                if (!TryAssign(parameters, p, property, out error))
                {
                    return ToolResult.Error(error, new { token, propertyPath, propertyType = property.propertyType.ToString() });
                }

                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(theme);
                AssetDatabase.SaveAssets();
            }

            return ToolResult.Success(
                dryRun ? "UI theme value change previewed." : "UI theme value updated.",
                new
                {
                    dryRun,
                    themePath,
                    token,
                    propertyPath,
                    propertyType = property.propertyType.ToString(),
                    before,
                    planned = preview,
                    after = dryRun ? before : Snapshot(property)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set UI theme value.");
        }
    }

    private static bool TryAssign(JObject parameters, ToolParams p, SerializedProperty property, out string error)
    {
        error = string.Empty;
        if (property.propertyType == SerializedPropertyType.Color)
        {
            if (!ParamUtility.TryReadColor(parameters, "color", out var color)
                && !ParamUtility.TryReadColor(parameters, "value", out color))
            {
                error = "Color property requires color or value as r,g,b,a.";
                return false;
            }

            property.colorValue = color;
            return true;
        }

        if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            var path = UnityCliToolShared.TryConvertToAssetPath(p.Get("referenceAssetPath", string.Empty));
            var guid = p.Get("referenceAssetGuid", string.Empty)?.Trim();
            if (!string.IsNullOrWhiteSpace(guid))
            {
                path = AssetDatabase.GUIDToAssetPath(guid);
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                error = "Object reference property requires referenceAssetPath or referenceAssetGuid.";
                return false;
            }

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset == null)
            {
                error = "Reference asset could not be loaded.";
                return false;
            }

            property.objectReferenceValue = asset;
            return true;
        }

        var raw = p.Get("value", string.Empty);
        if (property.propertyType == SerializedPropertyType.Float)
        {
            if (!float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value))
            {
                error = "Float property requires numeric value.";
                return false;
            }

            property.floatValue = value;
            return true;
        }

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            if (!int.TryParse(raw, out var value))
            {
                error = "Integer property requires numeric value.";
                return false;
            }

            property.intValue = value;
            return true;
        }

        if (property.propertyType == SerializedPropertyType.Boolean)
        {
            if (!bool.TryParse(raw, out var value))
            {
                error = "Boolean property requires true or false.";
                return false;
            }

            property.boolValue = value;
            return true;
        }

        if (property.propertyType == SerializedPropertyType.Enum)
        {
            for (var i = 0; i < property.enumNames.Length; i++)
            {
                if (string.Equals(property.enumNames[i], raw, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(property.enumDisplayNames[i], raw, StringComparison.OrdinalIgnoreCase))
                {
                    property.enumValueIndex = i;
                    return true;
                }
            }

            error = "Enum value was not found.";
            return false;
        }

        if (property.propertyType == SerializedPropertyType.String)
        {
            property.stringValue = raw ?? string.Empty;
            return true;
        }

        error = "Unsupported theme property type.";
        return false;
    }

    private static object SnapshotPlannedValue(JObject parameters, ToolParams p, SerializedProperty property)
    {
        if (property.propertyType == SerializedPropertyType.Color)
        {
            if (ParamUtility.TryReadColor(parameters, "color", out var color)
                || ParamUtility.TryReadColor(parameters, "value", out color))
            {
                return UiToolUtility.ColorSnapshot(color);
            }
        }

        if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            var path = UnityCliToolShared.TryConvertToAssetPath(p.Get("referenceAssetPath", string.Empty));
            var guid = p.Get("referenceAssetGuid", string.Empty)?.Trim();
            return new { referenceAssetPath = path ?? string.Empty, referenceAssetGuid = guid ?? string.Empty };
        }

        return p.Get("value", string.Empty);
    }

    private static object Snapshot(SerializedProperty property)
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
}
}
