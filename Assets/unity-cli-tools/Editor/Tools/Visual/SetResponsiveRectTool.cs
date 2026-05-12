using System;
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
    Name = UnityCliToolNames.SetResponsiveRect,
    Description = "Add or configure ResponsiveRectTransform for safe-area and extreme aspect-ratio UI fitting.",
    Group = UnityCliToolGroups.Visual)]
public static class SetResponsiveRectTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target UI GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Mode: stretch_parent, safe_area, centered_max_size, fit_aspect_inside_parent.")]
        public string Mode { get; set; }

        [ToolParameter("Margins as left,top,right,bottom.")]
        public string Margins { get; set; }

        [ToolParameter("Minimum size as x,y.")]
        public string MinSize { get; set; }

        [ToolParameter("Maximum size as x,y. Use 0 or omit a value to leave that axis unconstrained.")]
        public string MaxSize { get; set; }

        [ToolParameter("Aspect ratio for fit_aspect_inside_parent, width / height.")]
        public float AspectRatio { get; set; }

        [ToolParameter("Add ResponsiveRectTransform if missing.", DefaultValue = "true")]
        public bool AddIfMissing { get; set; }

        [ToolParameter("Apply the fitter immediately after updating fields.", DefaultValue = "true")]
        public bool ApplyNow { get; set; }

        [ToolParameter("Preview only without modifying the target.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var addIfMissing = p.GetBool("addIfMissing", true);
            var applyNow = p.GetBool("applyNow", true);
            var dryRun = p.GetBool("dryRun", true);

            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, hierarchyPath = hierarchyPath ?? string.Empty });
            }

            if (!(target.transform is RectTransform))
            {
                return ToolResult.Error("Target does not have a RectTransform.", UiToolUtility.ElementSnapshot(target));
            }

            var responsive = target.GetComponent<ResponsiveRectTransform>();
            if (responsive == null && !addIfMissing)
            {
                return ToolResult.Error("ResponsiveRectTransform is missing and addIfMissing is false.", UiToolUtility.ElementSnapshot(target));
            }

            if (!TryReadMode(p.Get("mode", string.Empty), out var mode, out error))
            {
                return ToolResult.Error(error);
            }

            var before = responsive != null ? Snapshot(responsive) : null;
            var plan = BuildPlan(parameters, p, mode);
            if (!dryRun)
            {
                if (responsive == null)
                {
                    responsive = Undo.AddComponent<ResponsiveRectTransform>(target);
                }

                Undo.RecordObject(responsive, "Set Responsive Rect");
                Apply(parameters, p, responsive, mode);
                if (applyNow)
                {
                    responsive.Apply();
                }

                EditorUtility.SetDirty(responsive);
                UiToolUtility.MarkUiObjectDirty(target, "Set Responsive Rect");
            }

            return ToolResult.Success(
                dryRun ? "Responsive Rect change previewed." : "Responsive Rect updated.",
                new
                {
                    dryRun,
                    applyNow,
                    target = UiToolUtility.ElementSnapshot(target),
                    before,
                    plan,
                    after = dryRun ? before : Snapshot(responsive)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set Responsive Rect.");
        }
    }

    private static void Apply(JObject parameters, ToolParams p, ResponsiveRectTransform responsive, ResponsiveRectMode? mode)
    {
        if (mode.HasValue)
        {
            responsive.Mode = mode.Value;
        }

        if (ParamUtility.TryReadVector4(parameters, "margins", out var margins))
        {
            responsive.Margins = margins;
        }

        if (ParamUtility.TryReadVector2(parameters, "minSize", out var minSize))
        {
            responsive.MinSize = minSize;
        }

        if (ParamUtility.TryReadVector2(parameters, "maxSize", out var maxSize))
        {
            responsive.MaxSize = maxSize;
        }

        var aspectRatio = p.GetFloat("aspectRatio");
        if (aspectRatio.HasValue)
        {
            responsive.AspectRatio = aspectRatio.Value;
        }
    }

    private static object BuildPlan(JObject parameters, ToolParams p, ResponsiveRectMode? mode)
    {
        return new
        {
            mode = mode.HasValue ? mode.Value.ToString() : string.Empty,
            margins = ParamUtility.TryReadVector4(parameters, "margins", out var margins) ? UiToolUtility.Vector4Snapshot(margins) : null,
            minSize = ParamUtility.TryReadVector2(parameters, "minSize", out var minSize) ? UiToolUtility.Vector2Snapshot(minSize) : null,
            maxSize = ParamUtility.TryReadVector2(parameters, "maxSize", out var maxSize) ? UiToolUtility.Vector2Snapshot(maxSize) : null,
            aspectRatio = p.GetFloat("aspectRatio")
        };
    }

    private static bool TryReadMode(string raw, out ResponsiveRectMode? mode, out string error)
    {
        mode = null;
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        var normalized = raw.Trim().ToLowerInvariant().Replace("-", "_");
        if (normalized == "stretch_parent" || normalized == "stretch_to_parent")
        {
            mode = ResponsiveRectMode.StretchToParent;
            return true;
        }

        if (normalized == "safe_area" || normalized == "stretch_safe_area" || normalized == "stretch_to_safe_area")
        {
            mode = ResponsiveRectMode.StretchToSafeArea;
            return true;
        }

        if (normalized == "centered" || normalized == "centered_max_size")
        {
            mode = ResponsiveRectMode.CenteredMaxSize;
            return true;
        }

        if (normalized == "fit_aspect" || normalized == "fit_aspect_inside_parent")
        {
            mode = ResponsiveRectMode.FitAspectInsideParent;
            return true;
        }

        error = "Unsupported mode. Use stretch_parent, safe_area, centered_max_size, or fit_aspect_inside_parent.";
        return false;
    }

    private static object Snapshot(ResponsiveRectTransform responsive)
    {
        return new
        {
            mode = responsive.Mode.ToString(),
            margins = UiToolUtility.Vector4Snapshot(responsive.Margins),
            minSize = UiToolUtility.Vector2Snapshot(responsive.MinSize),
            maxSize = UiToolUtility.Vector2Snapshot(responsive.MaxSize),
            aspectRatio = responsive.AspectRatio
        };
    }
}
}
