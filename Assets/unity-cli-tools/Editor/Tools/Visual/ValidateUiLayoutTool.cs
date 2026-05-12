using System;
using System.Collections.Generic;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityCliTools.Infrastructure.Ui;
using UnityCliTools.Runtime.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.ValidateUiLayout,
    Description = "Validate common Canvas, RectTransform, raycast, EventSystem, and TMP layout issues.",
    Group = UnityCliToolGroups.Visual)]
public static class ValidateUiLayoutTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path to validate.")]
        public string ScenePath { get; set; }

        [ToolParameter("Optional Canvas or UI root hierarchy path.")]
        public string CanvasHierarchyPath { get; set; }

        [ToolParameter("Include inactive UI objects.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Max number of returned issues.", DefaultValue = "500")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var rootPath = p.Get("canvasHierarchyPath", string.Empty)?.Trim();
            var includeInactive = p.GetBool("includeInactive", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 500, 1, 5000);
            var issues = new List<object>();
            var canvasCount = 0;
            var uiObjectCount = 0;
            var eventSystemCount = 0;
            Canvas.ForceUpdateCanvases();

            if (!TryBuildValidationSet(scenePath, rootPath, includeInactive, out var validationObjects, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, canvasHierarchyPath = rootPath ?? string.Empty });
            }

            foreach (var go in UiToolUtility.EnumerateLoadedSceneObjects(scenePath, includeInactive))
            {
                if (go.GetComponent<EventSystem>() != null)
                {
                    eventSystemCount++;
                }
            }

            foreach (var go in validationObjects)
            {
                var rect = go.transform as RectTransform;
                var hasUiComponent = rect != null || go.GetComponent<Graphic>() != null || go.GetComponent<Selectable>() != null || go.GetComponent<TMP_Text>() != null;
                if (!hasUiComponent)
                {
                    continue;
                }

                uiObjectCount++;
                var canvas = go.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvasCount++;
                    ValidateCanvas(go, canvas, issues, limit);
                }

                if (rect != null)
                {
                    ValidateRect(go, rect, issues, limit);
                }

                ValidateGraphic(go, issues, limit);
                if (issues.Count >= limit)
                {
                    break;
                }
            }

            if (canvasCount > 0 && eventSystemCount == 0)
            {
                AddIssue(issues, limit, "warning", "missing_event_system", "Loaded UI has Canvas objects but no EventSystem.", null);
            }

            return ToolResult.Success(
                "UI layout validated.",
                new
                {
                    query = new { scenePath = scenePath ?? string.Empty, canvasHierarchyPath = rootPath ?? string.Empty, includeInactive, limit },
                    canvasCount,
                    uiObjectCount,
                    eventSystemCount,
                    issueCount = issues.Count,
                    issues
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to validate UI layout.");
        }
    }

    private static void ValidateCanvas(GameObject go, Canvas canvas, List<object> issues, int limit)
    {
        if (canvas.renderMode != RenderMode.WorldSpace && go.GetComponent<CanvasScaler>() == null)
        {
            AddIssue(issues, limit, "warning", "missing_canvas_scaler", "Screen-space Canvas has no CanvasScaler.", go);
        }

        if (go.GetComponent<GraphicRaycaster>() == null)
        {
            AddIssue(issues, limit, "warning", "missing_graphic_raycaster", "Canvas has no GraphicRaycaster.", go);
        }
    }

    private static void ValidateRect(GameObject go, RectTransform rect, List<object> issues, int limit)
    {
        ValidateResponsiveRect(go, issues, limit);

        if (!IsExpectedDynamicZeroSize(go, rect)
            && (Mathf.Approximately(rect.rect.width, 0f) || Mathf.Approximately(rect.rect.height, 0f)))
        {
            AddIssue(issues, limit, "warning", "zero_size_rect_transform", "RectTransform has zero width or height.", go);
        }

        var scale = rect.localScale;
        if (Mathf.Approximately(scale.x, 0f) || Mathf.Approximately(scale.y, 0f) || Mathf.Abs(scale.x) > 100f || Mathf.Abs(scale.y) > 100f)
        {
            AddIssue(issues, limit, "warning", "suspicious_rect_scale", "RectTransform local scale is zero or unusually large.", go);
        }

        if (rect.anchorMin.x > rect.anchorMax.x || rect.anchorMin.y > rect.anchorMax.y)
        {
            AddIssue(issues, limit, "error", "invalid_anchors", "RectTransform anchorMin is greater than anchorMax.", go);
        }
    }

    private static bool TryBuildValidationSet(
        string scenePath,
        string rootPath,
        bool includeInactive,
        out List<GameObject> objects,
        out string error)
    {
        objects = new List<GameObject>();
        error = string.Empty;

        if (!string.IsNullOrWhiteSpace(rootPath))
        {
            if (!UiToolUtility.TryResolveGameObject(scenePath, rootPath, out var root, out error))
            {
                return false;
            }

            objects.AddRange(UiToolUtility.EnumerateChildren(root, includeInactive, 128));
            return true;
        }

        objects.AddRange(UiToolUtility.EnumerateLoadedSceneObjects(scenePath, includeInactive));
        return true;
    }

    private static void ValidateResponsiveRect(GameObject go, List<object> issues, int limit)
    {
        var responsive = go.GetComponent<ResponsiveRectTransform>();
        if (responsive == null)
        {
            return;
        }

        var maxSize = responsive.MaxSize;
        var minSize = responsive.MinSize;
        if ((maxSize.x > 0f && minSize.x > maxSize.x)
            || (maxSize.y > 0f && minSize.y > maxSize.y))
        {
            AddIssue(issues, limit, "error", "invalid_responsive_rect_size", "ResponsiveRectTransform minSize is greater than maxSize.", go);
        }

        if (responsive.AspectRatio <= 0f)
        {
            AddIssue(issues, limit, "error", "invalid_responsive_aspect_ratio", "ResponsiveRectTransform aspectRatio must be greater than zero.", go);
        }
    }

    private static void ValidateGraphic(GameObject go, List<object> issues, int limit)
    {
        var graphic = go.GetComponent<Graphic>();
        var selectable = go.GetComponent<Selectable>();
        if (graphic != null
            && graphic.raycastTarget
            && selectable == null
            && go.GetComponent<Button>() == null
            && !HasSelectableAncestor(go.transform))
        {
            AddIssue(issues, limit, "info", "passive_raycast_target", "Graphic receives raycasts but is not Selectable.", go);
        }
    }

    private static bool HasSelectableAncestor(Transform transform)
    {
        var cursor = transform.parent;
        while (cursor != null)
        {
            if (cursor.GetComponent<Selectable>() != null)
            {
                return true;
            }

            cursor = cursor.parent;
        }

        return false;
    }

    private static bool IsExpectedDynamicZeroSize(GameObject go, RectTransform rect)
    {
        var cursor = go.transform;
        while (cursor != null)
        {
            var slider = cursor.GetComponent<Slider>();
            if (slider != null && slider.fillRect == rect)
            {
                return true;
            }

            cursor = cursor.parent;
        }

        return false;
    }

    private static void AddIssue(List<object> issues, int limit, string severity, string code, string message, GameObject go)
    {
        if (issues.Count >= limit)
        {
            return;
        }

        issues.Add(new
        {
            severity,
            code,
            message,
            target = go != null ? UiToolUtility.ElementSnapshot(go) : null
        });
    }
}
}
