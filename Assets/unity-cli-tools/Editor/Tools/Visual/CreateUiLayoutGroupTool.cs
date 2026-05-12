using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.CreateUiLayoutGroup,
    Description = "Add or configure common UI layout components.",
    Group = UnityCliToolGroups.Visual)]
public static class CreateUiLayoutGroupTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Layout type: vertical, horizontal, grid, content_size_fitter, layout_element, aspect_ratio_fitter.", Required = true)]
        public string LayoutType { get; set; }

        [ToolParameter("Add component if missing.", DefaultValue = "true")]
        public bool AddIfMissing { get; set; }

        [ToolParameter("Spacing for horizontal/vertical groups.")]
        public float Spacing { get; set; }

        [ToolParameter("Padding as left,top,right,bottom.")]
        public string Padding { get; set; }

        [ToolParameter("Child alignment TextAnchor enum.")]
        public string ChildAlignment { get; set; }

        [ToolParameter("Horizontal/Vertical group childControlWidth value.")]
        public string ChildControlWidth { get; set; }

        [ToolParameter("Horizontal/Vertical group childControlHeight value.")]
        public string ChildControlHeight { get; set; }

        [ToolParameter("Horizontal/Vertical group childForceExpandWidth value.")]
        public string ChildForceExpandWidth { get; set; }

        [ToolParameter("Horizontal/Vertical group childForceExpandHeight value.")]
        public string ChildForceExpandHeight { get; set; }

        [ToolParameter("Grid cell size as x,y.")]
        public string CellSize { get; set; }

        [ToolParameter("Grid spacing as x,y.")]
        public string GridSpacing { get; set; }

        [ToolParameter("Grid constraint enum.")]
        public string Constraint { get; set; }

        [ToolParameter("Grid constraint count.")]
        public int ConstraintCount { get; set; }

        [ToolParameter("ContentSizeFitter horizontal fit.")]
        public string HorizontalFit { get; set; }

        [ToolParameter("ContentSizeFitter vertical fit.")]
        public string VerticalFit { get; set; }

        [ToolParameter("LayoutElement preferred width.")]
        public float PreferredWidth { get; set; }

        [ToolParameter("LayoutElement preferred height.")]
        public float PreferredHeight { get; set; }

        [ToolParameter("LayoutElement min width.")]
        public float MinWidth { get; set; }

        [ToolParameter("LayoutElement min height.")]
        public float MinHeight { get; set; }

        [ToolParameter("LayoutElement flexible width.")]
        public float FlexibleWidth { get; set; }

        [ToolParameter("LayoutElement flexible height.")]
        public float FlexibleHeight { get; set; }

        [ToolParameter("LayoutElement ignoreLayout value.")]
        public string IgnoreLayout { get; set; }

        [ToolParameter("AspectRatioFitter mode enum.")]
        public string AspectMode { get; set; }

        [ToolParameter("AspectRatioFitter aspect ratio, width / height.")]
        public float AspectRatio { get; set; }

        [ToolParameter("Preview only without modifying the target.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = p.Get("scenePath", string.Empty);
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var layoutType = Normalize(p.Get("layoutType", string.Empty));
            var addIfMissing = p.GetBool("addIfMissing", true);
            var dryRun = p.GetBool("dryRun", true);
            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, hierarchyPath = hierarchyPath ?? string.Empty });
            }

            if (!IsSupported(layoutType))
            {
                return ToolResult.Error("Unsupported layoutType.", new { layoutType });
            }

            var before = UiToolUtility.ElementSnapshot(target);
            if (!dryRun)
            {
                Apply(parameters, p, target, layoutType, addIfMissing);
                UiToolUtility.MarkUiObjectDirty(target, "Create UI Layout Group");
            }

            return ToolResult.Success(
                dryRun ? "UI layout configuration previewed." : "UI layout configured.",
                new
                {
                    dryRun,
                    layoutType,
                    before,
                    after = UiToolUtility.ElementSnapshot(target)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to configure UI layout group.");
        }
    }

    private static void Apply(JObject parameters, ToolParams p, GameObject target, string layoutType, bool addIfMissing)
    {
        if (layoutType == "vertical" || layoutType == "horizontal")
        {
            var group = layoutType == "vertical"
                ? GetOrAdd<VerticalLayoutGroup>(target, addIfMissing) as HorizontalOrVerticalLayoutGroup
                : GetOrAdd<HorizontalLayoutGroup>(target, addIfMissing) as HorizontalOrVerticalLayoutGroup;
            if (group == null) return;
            Undo.RecordObject(group, "Configure Layout Group");
            var spacing = p.GetFloat("spacing");
            if (spacing.HasValue) group.spacing = spacing.Value;
            if (TryReadPadding(p.Get("padding", string.Empty), out var padding)) group.padding = padding;
            if (Enum.TryParse<TextAnchor>(p.Get("childAlignment", string.Empty), true, out var alignment)) group.childAlignment = alignment;
            ApplyOptionalBool(p, "childControlWidth", v => group.childControlWidth = v);
            ApplyOptionalBool(p, "childControlHeight", v => group.childControlHeight = v);
            ApplyOptionalBool(p, "childForceExpandWidth", v => group.childForceExpandWidth = v);
            ApplyOptionalBool(p, "childForceExpandHeight", v => group.childForceExpandHeight = v);
            EditorUtility.SetDirty(group);
            return;
        }

        if (layoutType == "grid")
        {
            var grid = GetOrAdd<GridLayoutGroup>(target, addIfMissing);
            if (grid == null) return;
            Undo.RecordObject(grid, "Configure Grid Layout Group");
            if (ParamUtility.TryReadVector2(parameters, "cellSize", out var cellSize)) grid.cellSize = cellSize;
            if (ParamUtility.TryReadVector2(parameters, "gridSpacing", out var gridSpacing)) grid.spacing = gridSpacing;
            if (TryReadPadding(p.Get("padding", string.Empty), out var padding)) grid.padding = padding;
            if (Enum.TryParse<TextAnchor>(p.Get("childAlignment", string.Empty), true, out var alignment)) grid.childAlignment = alignment;
            if (Enum.TryParse<GridLayoutGroup.Constraint>(p.Get("constraint", string.Empty), true, out var constraint)) grid.constraint = constraint;
            var count = p.GetInt("constraintCount");
            if (count.HasValue) grid.constraintCount = Math.Max(1, count.Value);
            EditorUtility.SetDirty(grid);
            return;
        }

        if (layoutType == "content_size_fitter")
        {
            var fitter = GetOrAdd<ContentSizeFitter>(target, addIfMissing);
            if (fitter == null) return;
            Undo.RecordObject(fitter, "Configure Content Size Fitter");
            if (Enum.TryParse<ContentSizeFitter.FitMode>(p.Get("horizontalFit", string.Empty), true, out var hFit)) fitter.horizontalFit = hFit;
            if (Enum.TryParse<ContentSizeFitter.FitMode>(p.Get("verticalFit", string.Empty), true, out var vFit)) fitter.verticalFit = vFit;
            EditorUtility.SetDirty(fitter);
            return;
        }

        if (layoutType == "aspect_ratio_fitter")
        {
            var fitter = GetOrAdd<AspectRatioFitter>(target, addIfMissing);
            if (fitter == null) return;
            Undo.RecordObject(fitter, "Configure Aspect Ratio Fitter");
            if (Enum.TryParse<AspectRatioFitter.AspectMode>(p.Get("aspectMode", string.Empty), true, out var aspectMode)) fitter.aspectMode = aspectMode;
            var aspectRatio = p.GetFloat("aspectRatio");
            if (aspectRatio.HasValue) fitter.aspectRatio = Mathf.Max(0.01f, aspectRatio.Value);
            EditorUtility.SetDirty(fitter);
            return;
        }

        var element = GetOrAdd<LayoutElement>(target, addIfMissing);
        if (element == null) return;
        Undo.RecordObject(element, "Configure Layout Element");
        ApplyOptionalFloat(p, "preferredWidth", v => element.preferredWidth = v);
        ApplyOptionalFloat(p, "preferredHeight", v => element.preferredHeight = v);
        ApplyOptionalFloat(p, "minWidth", v => element.minWidth = v);
        ApplyOptionalFloat(p, "minHeight", v => element.minHeight = v);
        ApplyOptionalFloat(p, "flexibleWidth", v => element.flexibleWidth = v);
        ApplyOptionalFloat(p, "flexibleHeight", v => element.flexibleHeight = v);
        ApplyOptionalBool(p, "ignoreLayout", v => element.ignoreLayout = v);
        EditorUtility.SetDirty(element);
    }

    private static T GetOrAdd<T>(GameObject target, bool addIfMissing) where T : UnityComponent
    {
        var component = target.GetComponent<T>();
        if (component == null && addIfMissing)
        {
            component = Undo.AddComponent<T>(target);
        }

        return component;
    }

    private static void ApplyOptionalFloat(ToolParams p, string key, Action<float> apply)
    {
        var value = p.GetFloat(key);
        if (value.HasValue) apply(value.Value);
    }

    private static void ApplyOptionalBool(ToolParams p, string key, Action<bool> apply)
    {
        var raw = p.Get(key, string.Empty);
        if (bool.TryParse(raw, out var value)) apply(value);
    }

    private static bool TryReadPadding(string raw, out RectOffset padding)
    {
        padding = null;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var parts = raw.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4) return false;
        if (!int.TryParse(parts[0], out var left) || !int.TryParse(parts[1], out var top)
            || !int.TryParse(parts[2], out var right) || !int.TryParse(parts[3], out var bottom))
        {
            return false;
        }

        padding = new RectOffset(left, right, top, bottom);
        return true;
    }

    private static string Normalize(string value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant().Replace("-", "_");
    }

    private static bool IsSupported(string value)
    {
        return value == "vertical" || value == "horizontal" || value == "grid"
            || value == "content_size_fitter" || value == "layout_element"
            || value == "aspect_ratio_fitter";
    }
}
}
