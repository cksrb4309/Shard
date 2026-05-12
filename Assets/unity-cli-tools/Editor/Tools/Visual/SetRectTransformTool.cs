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
    Name = UnityCliToolNames.SetRectTransform,
    Description = "Set RectTransform anchors, pivot, offsets, size, position, or scale.",
    Group = UnityCliToolGroups.Visual)]
public static class SetRectTransformTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target UI GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Anchored position as x,y.")]
        public string AnchoredPosition { get; set; }

        [ToolParameter("Size delta as x,y.")]
        public string SizeDelta { get; set; }

        [ToolParameter("Anchor min as x,y.")]
        public string AnchorMin { get; set; }

        [ToolParameter("Anchor max as x,y.")]
        public string AnchorMax { get; set; }

        [ToolParameter("Pivot as x,y.")]
        public string Pivot { get; set; }

        [ToolParameter("Offset min as x,y.")]
        public string OffsetMin { get; set; }

        [ToolParameter("Offset max as x,y.")]
        public string OffsetMax { get; set; }

        [ToolParameter("Local scale as x,y,z.")]
        public string LocalScale { get; set; }

        [ToolParameter("Preview only without modifying the object.", DefaultValue = "true")]
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

            var rectTransform = target.transform as RectTransform;
            if (rectTransform == null)
            {
                return ToolResult.Error("Target does not have a RectTransform.", UiToolUtility.ElementSnapshot(target));
            }

            var hasAnchoredPosition = ParamUtility.TryReadVector2(parameters, "anchoredPosition", out var anchoredPosition);
            var hasSizeDelta = ParamUtility.TryReadVector2(parameters, "sizeDelta", out var sizeDelta);
            var hasAnchorMin = ParamUtility.TryReadVector2(parameters, "anchorMin", out var anchorMin);
            var hasAnchorMax = ParamUtility.TryReadVector2(parameters, "anchorMax", out var anchorMax);
            var hasPivot = ParamUtility.TryReadVector2(parameters, "pivot", out var pivot);
            var hasOffsetMin = ParamUtility.TryReadVector2(parameters, "offsetMin", out var offsetMin);
            var hasOffsetMax = ParamUtility.TryReadVector2(parameters, "offsetMax", out var offsetMax);
            var hasLocalScale = ParamUtility.TryReadVector3(parameters, "localScale", out var localScale);
            if (!hasAnchoredPosition && !hasSizeDelta && !hasAnchorMin && !hasAnchorMax && !hasPivot && !hasOffsetMin && !hasOffsetMax && !hasLocalScale)
            {
                return ToolResult.Error("At least one RectTransform field is required.");
            }

            var before = UiToolUtility.RectSnapshot(rectTransform);
            if (!dryRun)
            {
                Undo.RecordObject(rectTransform, "Set RectTransform");
                if (hasAnchorMin) rectTransform.anchorMin = anchorMin;
                if (hasAnchorMax) rectTransform.anchorMax = anchorMax;
                if (hasPivot) rectTransform.pivot = pivot;
                if (hasAnchoredPosition) rectTransform.anchoredPosition = anchoredPosition;
                if (hasSizeDelta) rectTransform.sizeDelta = sizeDelta;
                if (hasOffsetMin) rectTransform.offsetMin = offsetMin;
                if (hasOffsetMax) rectTransform.offsetMax = offsetMax;
                if (hasLocalScale) rectTransform.localScale = localScale;
                UiToolUtility.MarkUiObjectDirty(target, "Set RectTransform");
            }

            return ToolResult.Success(
                dryRun ? "RectTransform change previewed." : "RectTransform updated.",
                new
                {
                    dryRun,
                    target = UiToolUtility.ElementSnapshot(target),
                    before,
                    after = UiToolUtility.RectSnapshot(rectTransform)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set RectTransform.");
        }
    }
}
}
