using System;
using TMPro;
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
    Name = UnityCliToolNames.SetTmpTextStyle,
    Description = "Set TMP text style fields such as font, size, color, alignment, wrapping, and overflow.",
    Group = UnityCliToolGroups.Visual)]
public static class SetTmpTextStyleTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the TMP object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target TMP GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("TMP font asset path.")]
        public string FontAssetPath { get; set; }

        [ToolParameter("TMP font asset GUID.")]
        public string FontAssetGuid { get; set; }

        [ToolParameter("Font size.")]
        public float FontSize { get; set; }

        [ToolParameter("Color as r,g,b,a.")]
        public string Color { get; set; }

        [ToolParameter("Alignment enum, e.g. Center, Left, TopLeft.")]
        public string Alignment { get; set; }

        [ToolParameter("Font style enum flags, e.g. Bold, Italic.")]
        public string FontStyle { get; set; }

        [ToolParameter("Enable word wrapping.")]
        public string EnableWordWrapping { get; set; }

        [ToolParameter("Overflow mode enum, e.g. Overflow, Ellipsis, Truncate.")]
        public string OverflowMode { get; set; }

        [ToolParameter("Raycast target value.")]
        public string RaycastTarget { get; set; }

        [ToolParameter("Preview only without modifying the TMP component.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = p.Get("scenePath", string.Empty);
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var dryRun = p.GetBool("dryRun", true);
            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, hierarchyPath = hierarchyPath ?? string.Empty });
            }

            var tmp = target.GetComponent<TMP_Text>();
            if (tmp == null)
            {
                return ToolResult.Error("Target has no TMP_Text component.", UiToolUtility.ElementSnapshot(target));
            }

            if (!TryResolveFont(p, out var font, out error))
            {
                return ToolResult.Error(error);
            }

            var before = Snapshot(tmp);
            if (!dryRun)
            {
                Undo.RecordObject(tmp, "Set TMP Text Style");
                Apply(parameters, p, tmp, font);
                EditorUtility.SetDirty(tmp);
                UiToolUtility.MarkUiObjectDirty(target, "Set TMP Text Style");
            }

            return ToolResult.Success(
                dryRun ? "TMP text style change previewed." : "TMP text style updated.",
                new
                {
                    dryRun,
                    target = UiToolUtility.ElementSnapshot(target),
                    before,
                    after = dryRun ? before : Snapshot(tmp)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set TMP text style.");
        }
    }

    private static void Apply(JObject parameters, ToolParams p, TMP_Text tmp, TMP_FontAsset font)
    {
        if (font != null)
        {
            tmp.font = font;
        }

        var fontSize = p.GetFloat("fontSize");
        if (fontSize.HasValue)
        {
            tmp.fontSize = Mathf.Max(0f, fontSize.Value);
        }

        if (ParamUtility.TryReadColor(parameters, "color", out var color))
        {
            tmp.color = color;
        }

        if (Enum.TryParse<TextAlignmentOptions>(p.Get("alignment", string.Empty), true, out var alignment))
        {
            tmp.alignment = alignment;
        }

        if (Enum.TryParse<FontStyles>(p.Get("fontStyle", string.Empty), true, out var fontStyle))
        {
            tmp.fontStyle = fontStyle;
        }

        var wrap = ParseOptionalBool(p.Get("enableWordWrapping", string.Empty));
        if (wrap.HasValue)
        {
            tmp.textWrappingMode = wrap.Value ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        }

        if (Enum.TryParse<TextOverflowModes>(p.Get("overflowMode", string.Empty), true, out var overflow))
        {
            tmp.overflowMode = overflow;
        }

        var raycast = ParseOptionalBool(p.Get("raycastTarget", string.Empty));
        if (raycast.HasValue)
        {
            tmp.raycastTarget = raycast.Value;
        }
    }

    private static bool TryResolveFont(ToolParams p, out TMP_FontAsset font, out string error)
    {
        font = null;
        error = string.Empty;

        var guid = p.Get("fontAssetGuid", string.Empty)?.Trim();
        var path = UnityCliToolShared.TryConvertToAssetPath(p.Get("fontAssetPath", string.Empty));
        if (!string.IsNullOrWhiteSpace(guid))
        {
            path = AssetDatabase.GUIDToAssetPath(guid);
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return true;
        }

        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (font == null)
        {
            error = "TMP font asset could not be resolved.";
            return false;
        }

        return true;
    }

    private static bool? ParseOptionalBool(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return bool.TryParse(value.Trim(), out var parsed) ? parsed : (bool?)null;
    }

    private static object Snapshot(TMP_Text tmp)
    {
        return new
        {
            text = tmp.text,
            font = tmp.font != null ? tmp.font.name : string.Empty,
            fontSize = tmp.fontSize,
            color = UiToolUtility.ColorSnapshot(tmp.color),
            alignment = tmp.alignment.ToString(),
            fontStyle = tmp.fontStyle.ToString(),
            enableWordWrapping = IsWordWrappingEnabled(tmp.textWrappingMode),
            textWrappingMode = tmp.textWrappingMode.ToString(),
            overflowMode = tmp.overflowMode.ToString(),
            raycastTarget = tmp.raycastTarget
        };
    }

    private static bool IsWordWrappingEnabled(TextWrappingModes mode)
    {
        return mode == TextWrappingModes.Normal || mode == TextWrappingModes.PreserveWhitespace;
    }
}
}
