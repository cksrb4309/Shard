using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;
using UnityCliTools.Runtime.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.SetUiThemeValues,
    Description = "Set multiple UiTheme color, scalar, and image/font reference tokens in one call.",
    Group = UnityCliToolGroups.Visual)]
public static class SetUiThemeValuesTool
{
    public sealed class Parameters
    {
        [ToolParameter("UiTheme asset path.")]
        public string ThemePath { get; set; }

        [ToolParameter("UiTheme asset GUID.")]
        public string ThemeGuid { get; set; }

        [ToolParameter("Color token map, e.g. { panel: '0,0,0,1', primary: '1,0.7,0.2,1' }.")]
        public string Colors { get; set; }

        [ToolParameter("Scalar token map, e.g. { title_size: 52, body_size: 30 }.")]
        public string Values { get; set; }

        [ToolParameter("Reference token map for font/sprite asset paths, e.g. { button_sprite: 'Assets/UI/button.png' }.")]
        public string References { get; set; }

        [ToolParameter("Scene path containing UI root to apply after update.")]
        public string ScenePath { get; set; }

        [ToolParameter("UI root hierarchy path to apply after update.")]
        public string RootHierarchyPath { get; set; }

        [ToolParameter("Include inactive bindings when applying after update.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Apply changed theme to root immediately if rootHierarchyPath is provided.", DefaultValue = "true")]
        public bool ApplyAfterUpdate { get; set; }

        [ToolParameter("Preview only without modifying the theme.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var dryRun = p.GetBool("dryRun", true);
            var includeInactive = p.GetBool("includeInactive", true);
            var applyAfterUpdate = p.GetBool("applyAfterUpdate", true);
            if (!UiThemeToolUtility.TryResolveTheme(parameters, out var theme, out var themePath, out var error))
            {
                return ToolResult.Error(error);
            }

            var serialized = new SerializedObject(theme);
            var changes = new List<object>();
            CollectColorChanges(parameters["colors"], serialized, changes);
            CollectValueChanges(parameters["values"], serialized, changes);
            CollectReferenceChanges(parameters["references"], serialized, changes);
            if (changes.Count == 0)
            {
                return ToolResult.Error("At least one colors, values, or references entry is required.");
            }

            var appliedToRoot = false;
            var appliedBindingCount = 0;
            if (!dryRun)
            {
                Undo.RecordObject(theme, "Set UI Theme Values");
                foreach (var change in changes)
                {
                    var item = (Change)change;
                    if (!item.Apply(out error))
                    {
                        return ToolResult.Error(error, new { token = item.Token, propertyPath = item.PropertyPath });
                    }
                }

                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(theme);
                AssetDatabase.SaveAssets();

                var rootPath = p.Get("rootHierarchyPath", string.Empty)?.Trim();
                if (applyAfterUpdate && !string.IsNullOrWhiteSpace(rootPath))
                {
                    var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
                    if (!UiToolUtility.TryResolveGameObject(scenePath, rootPath, out var root, out error))
                    {
                        return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, rootHierarchyPath = rootPath });
                    }

                    var applier = root.GetComponent<UiThemeApplier>();
                    if (applier == null)
                    {
                        applier = Undo.AddComponent<UiThemeApplier>(root);
                    }
                    else
                    {
                        Undo.RecordObject(applier, "Apply UI Theme Values");
                    }

                    applier.Theme = theme;
                    applier.IncludeInactive = includeInactive;
                    appliedBindingCount = applier.ApplyTheme();
                    EditorUtility.SetDirty(applier);
                    UiToolUtility.MarkUiObjectDirty(root, "Apply UI Theme Values");
                    appliedToRoot = true;
                }
            }

            return ToolResult.Success(
                dryRun ? "UI theme values change previewed." : "UI theme values updated.",
                new
                {
                    dryRun,
                    themePath,
                    changeCount = changes.Count,
                    changes,
                    appliedToRoot,
                    appliedBindingCount
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set UI theme values.");
        }
    }

    private static void CollectColorChanges(JToken token, SerializedObject serialized, List<object> changes)
    {
        foreach (var entry in EnumerateObject(token))
        {
            var propertyPath = UiThemeToolUtility.ResolveToken(entry.Key);
            var property = serialized.FindProperty(propertyPath);
            if (property == null)
            {
                changes.Add(Change.Error(entry.Key, propertyPath, "Theme token was not found."));
                continue;
            }

            changes.Add(Change.Color(entry.Key, propertyPath, property, entry.Value));
        }
    }

    private static void CollectValueChanges(JToken token, SerializedObject serialized, List<object> changes)
    {
        foreach (var entry in EnumerateObject(token))
        {
            var propertyPath = UiThemeToolUtility.ResolveToken(entry.Key);
            var property = serialized.FindProperty(propertyPath);
            if (property == null)
            {
                changes.Add(Change.Error(entry.Key, propertyPath, "Theme token was not found."));
                continue;
            }

            changes.Add(Change.Value(entry.Key, propertyPath, property, entry.Value));
        }
    }

    private static void CollectReferenceChanges(JToken token, SerializedObject serialized, List<object> changes)
    {
        foreach (var entry in EnumerateObject(token))
        {
            var propertyPath = UiThemeToolUtility.ResolveToken(entry.Key);
            var property = serialized.FindProperty(propertyPath);
            if (property == null)
            {
                changes.Add(Change.Error(entry.Key, propertyPath, "Theme token was not found."));
                continue;
            }

            changes.Add(Change.Reference(entry.Key, propertyPath, property, entry.Value));
        }
    }

    private static IEnumerable<KeyValuePair<string, JToken>> EnumerateObject(JToken token)
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                yield return new KeyValuePair<string, JToken>(property.Name, property.Value);
            }
        }
    }

    private sealed class Change
    {
        private readonly SerializedProperty property;
        private readonly JToken value;
        private readonly string kind;
        private readonly string error;

        public string Token { get; private set; }
        public string PropertyPath { get; private set; }
        public string Kind { get { return kind; } }
        public object Before { get; private set; }
        public object Planned { get; private set; }

        private Change(string token, string propertyPath, SerializedProperty property, JToken value, string kind, object planned, string error)
        {
            Token = token;
            PropertyPath = propertyPath;
            this.property = property;
            this.value = value;
            this.kind = kind;
            this.error = error;
            Before = property != null ? UiThemeToolUtility.SerializedPropertySnapshot(property) : null;
            Planned = planned;
        }

        public static Change Color(string token, string propertyPath, SerializedProperty property, JToken value)
        {
            return new Change(token, propertyPath, property, value, "color", value != null ? value.ToString() : string.Empty, string.Empty);
        }

        public static Change Value(string token, string propertyPath, SerializedProperty property, JToken value)
        {
            return new Change(token, propertyPath, property, value, "value", value != null ? value.ToString() : string.Empty, string.Empty);
        }

        public static Change Reference(string token, string propertyPath, SerializedProperty property, JToken value)
        {
            return new Change(token, propertyPath, property, value, "reference", value != null ? value.ToString() : string.Empty, string.Empty);
        }

        public static Change Error(string token, string propertyPath, string error)
        {
            return new Change(token, propertyPath, null, null, "error", null, error);
        }

        public bool Apply(out string applyError)
        {
            applyError = string.Empty;
            if (!string.IsNullOrWhiteSpace(error))
            {
                applyError = error;
                return false;
            }

            if (kind == "color")
            {
                if (property.propertyType != SerializedPropertyType.Color)
                {
                    applyError = "Color entry targets a non-color property.";
                    return false;
                }

                if (!TryReadColor(value, out var color))
                {
                    applyError = "Color value must be r,g,b,a or JSON array [r,g,b,a].";
                    return false;
                }

                property.colorValue = color;
                return true;
            }

            if (kind == "reference")
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    applyError = "Reference entry targets a non-object-reference property.";
                    return false;
                }

                var path = UnityCliToolShared.TryConvertToAssetPath(value != null ? value.ToString() : string.Empty);
                if (string.IsNullOrWhiteSpace(path))
                {
                    applyError = "Reference value must be an asset path.";
                    return false;
                }

                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (asset == null)
                {
                    applyError = "Reference asset could not be loaded.";
                    return false;
                }

                property.objectReferenceValue = asset;
                return true;
            }

            return ApplyScalar(out applyError);
        }

        private bool ApplyScalar(out string applyError)
        {
            applyError = string.Empty;
            var raw = value != null ? value.ToString() : string.Empty;
            if (property.propertyType == SerializedPropertyType.Float)
            {
                if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    applyError = "Float value is invalid.";
                    return false;
                }

                property.floatValue = parsed;
                return true;
            }

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    applyError = "Integer value is invalid.";
                    return false;
                }

                property.intValue = parsed;
                return true;
            }

            if (property.propertyType == SerializedPropertyType.Boolean)
            {
                if (!bool.TryParse(raw, out var parsed))
                {
                    applyError = "Boolean value is invalid.";
                    return false;
                }

                property.boolValue = parsed;
                return true;
            }

            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = raw;
                return true;
            }

            applyError = "Unsupported scalar property type.";
            return false;
        }

        private static bool TryReadColor(JToken token, out Color color)
        {
            color = default;
            var values = ParamUtility.ReadFloatList(token);
            if (values.Count < 4)
            {
                return false;
            }

            color = new Color(values[0], values[1], values[2], values[3]);
            return true;
        }
    }
}
}
