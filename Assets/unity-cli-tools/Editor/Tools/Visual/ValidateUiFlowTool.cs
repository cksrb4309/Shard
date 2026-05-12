using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.ValidateUiFlow,
    Description = "Validate a UI flow: Canvas setup, EventSystem, Button onClick, controller references, pages array, and active state.",
    Group = UnityCliToolGroups.Visual)]
public static class ValidateUiFlowTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path to validate.")]
        public string ScenePath { get; set; }

        [ToolParameter("Canvas hierarchy path. If omitted, validates all loaded scene canvases.")]
        public string CanvasHierarchyPath { get; set; }

        [ToolParameter("Controller GameObject hierarchy path.")]
        public string ControllerHierarchyPath { get; set; }

        [ToolParameter("Controller component type name.")]
        public string ControllerComponentType { get; set; }

        [ToolParameter("Serialized pages array property path.", DefaultValue = "pages")]
        public string PagesPropertyPath { get; set; }

        [ToolParameter("Serialized previous label property path.", DefaultValue = "previousLabel")]
        public string PreviousLabelPropertyPath { get; set; }

        [ToolParameter("Serialized next label property path.", DefaultValue = "nextLabel")]
        public string NextLabelPropertyPath { get; set; }

        [ToolParameter("Previous Button hierarchy path.")]
        public string PreviousButtonHierarchyPath { get; set; }

        [ToolParameter("Next Button hierarchy path.")]
        public string NextButtonHierarchyPath { get; set; }

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
            var canvasPath = p.Get("canvasHierarchyPath", string.Empty)?.Trim();
            var includeInactive = p.GetBool("includeInactive", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 500, 1, 5000);
            var issues = new List<object>();
            var canvases = ResolveCanvases(scenePath, canvasPath, includeInactive);

            foreach (var canvasObject in canvases)
            {
                ValidateCanvas(canvasObject, issues, limit);
                ValidateButtons(canvasObject, includeInactive, issues, limit);
            }

            ValidateNamedButton(scenePath, p.Get("previousButtonHierarchyPath", string.Empty), "previous_button", issues, limit);
            ValidateNamedButton(scenePath, p.Get("nextButtonHierarchyPath", string.Empty), "next_button", issues, limit);
            ValidateController(parameters, issues, limit);

            return ToolResult.Success(
                "UI flow validated.",
                new
                {
                    query = new
                    {
                        scenePath = scenePath ?? string.Empty,
                        canvasHierarchyPath = canvasPath ?? string.Empty,
                        includeInactive,
                        limit
                    },
                    canvasCount = canvases.Count,
                    issueCount = issues.Count,
                    issues
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to validate UI flow.");
        }
    }

    private static List<GameObject> ResolveCanvases(string scenePath, string canvasPath, bool includeInactive)
    {
        var canvases = new List<GameObject>();
        if (!string.IsNullOrWhiteSpace(canvasPath))
        {
            if (UiToolUtility.TryResolveGameObject(scenePath, canvasPath, out var canvasObject, out _))
            {
                canvases.Add(canvasObject);
            }

            return canvases;
        }

        foreach (var go in UiToolUtility.EnumerateLoadedSceneObjects(scenePath, includeInactive))
        {
            if (go.GetComponent<Canvas>() != null)
            {
                canvases.Add(go);
            }
        }

        return canvases;
    }

    private static void ValidateCanvas(GameObject canvasObject, List<object> issues, int limit)
    {
        var canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            AddIssue(issues, limit, "error", "target_not_canvas", "Target does not have a Canvas component.", canvasObject);
            return;
        }

        if (!canvasObject.activeSelf)
        {
            AddIssue(issues, limit, "warning", "canvas_inactive_on_start", "Canvas GameObject is inactive.", canvasObject);
        }

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            AddIssue(issues, limit, "warning", "missing_canvas_scaler", "Canvas has no CanvasScaler.", canvasObject);
        }
        else if (canvas.renderMode != RenderMode.WorldSpace && scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            AddIssue(issues, limit, "info", "canvas_scaler_not_responsive", "CanvasScaler is not ScaleWithScreenSize.", canvasObject);
        }

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            AddIssue(issues, limit, "warning", "missing_graphic_raycaster", "Canvas has no GraphicRaycaster.", canvasObject);
        }

        var hasEventSystem = false;
        foreach (var go in UiToolUtility.EnumerateLoadedSceneObjects(canvasObject.scene.path, true))
        {
            if (go.GetComponent<EventSystem>() != null)
            {
                hasEventSystem = true;
                break;
            }
        }

        if (!hasEventSystem)
        {
            AddIssue(issues, limit, "warning", "missing_event_system", "Canvas scene has no EventSystem.", canvasObject);
        }
    }

    private static void ValidateButtons(GameObject root, bool includeInactive, List<object> issues, int limit)
    {
        foreach (var go in UiToolUtility.EnumerateChildren(root, includeInactive, 128))
        {
            var button = go.GetComponent<Button>();
            if (button == null)
            {
                continue;
            }

            if (!button.interactable)
            {
                AddIssue(issues, limit, "info", "button_not_interactable", "Button is not interactable.", go);
            }

            if (button.onClick.GetPersistentEventCount() == 0)
            {
                AddIssue(issues, limit, "warning", "button_missing_on_click", "Button has no persistent onClick listener.", go);
            }
        }
    }

    private static void ValidateNamedButton(string scenePath, string path, string codePrefix, List<object> issues, int limit)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (!UiToolUtility.TryResolveGameObject(scenePath, path, out var go, out var error))
        {
            AddIssue(issues, limit, "error", codePrefix + "_not_found", error, null);
            return;
        }

        var button = go.GetComponent<Button>();
        if (button == null)
        {
            AddIssue(issues, limit, "error", codePrefix + "_missing_button", "Named button target has no Button component.", go);
            return;
        }

        if (button.onClick.GetPersistentEventCount() == 0)
        {
            AddIssue(issues, limit, "warning", codePrefix + "_missing_on_click", "Named button has no persistent onClick listener.", go);
        }
    }

    private static void ValidateController(JObject parameters, List<object> issues, int limit)
    {
        var p = new ToolParams(parameters);
        var controllerPath = p.Get("controllerHierarchyPath", string.Empty)?.Trim();
        var componentType = p.Get("controllerComponentType", string.Empty)?.Trim();
        if (string.IsNullOrWhiteSpace(controllerPath) && string.IsNullOrWhiteSpace(componentType))
        {
            return;
        }

        if (!UiSerializedUtility.TryResolveComponent(
            p.Get("scenePath", string.Empty),
            controllerPath,
            componentType,
            0,
            out var component,
            out var gameObject,
            out var error))
        {
            AddIssue(issues, limit, "error", "controller_not_resolved", error, null);
            return;
        }

        var serialized = new SerializedObject(component);
        ValidateArrayProperty(serialized, p.Get("pagesPropertyPath", "pages"), "pages", gameObject, issues, limit);
        ValidateObjectProperty<TMP_Text>(serialized, p.Get("previousLabelPropertyPath", "previousLabel"), "previous_label", gameObject, issues, limit);
        ValidateObjectProperty<TMP_Text>(serialized, p.Get("nextLabelPropertyPath", "nextLabel"), "next_label", gameObject, issues, limit);
    }

    private static void ValidateArrayProperty(SerializedObject serialized, string propertyPath, string codePrefix, GameObject owner, List<object> issues, int limit)
    {
        var property = serialized.FindProperty(propertyPath);
        if (property == null)
        {
            AddIssue(issues, limit, "warning", codePrefix + "_property_missing", "Controller array property was not found.", owner);
            return;
        }

        if (!property.isArray || property.propertyType == SerializedPropertyType.String)
        {
            AddIssue(issues, limit, "error", codePrefix + "_not_array", "Controller property is not an array/list.", owner);
            return;
        }

        if (property.arraySize == 0)
        {
            AddIssue(issues, limit, "warning", codePrefix + "_empty", "Controller pages array is empty.", owner);
        }

        for (var i = 0; i < property.arraySize; i++)
        {
            var element = property.GetArrayElementAtIndex(i);
            if (element.propertyType == SerializedPropertyType.ObjectReference && element.objectReferenceValue == null)
            {
                AddIssue(issues, limit, "warning", codePrefix + "_null_element", "Controller pages array has a null element.", owner);
            }
        }
    }

    private static void ValidateObjectProperty<T>(SerializedObject serialized, string propertyPath, string codePrefix, GameObject owner, List<object> issues, int limit)
        where T : UnityEngine.Object
    {
        var property = serialized.FindProperty(propertyPath);
        if (property == null)
        {
            AddIssue(issues, limit, "info", codePrefix + "_property_missing", "Controller object reference property was not found.", owner);
            return;
        }

        if (property.propertyType != SerializedPropertyType.ObjectReference)
        {
            AddIssue(issues, limit, "warning", codePrefix + "_not_object_reference", "Controller property is not an object reference.", owner);
            return;
        }

        if (property.objectReferenceValue == null)
        {
            AddIssue(issues, limit, "warning", codePrefix + "_null", "Controller object reference is null.", owner);
            return;
        }

        if (!(property.objectReferenceValue is T))
        {
            AddIssue(issues, limit, "warning", codePrefix + "_wrong_type", "Controller object reference has an unexpected type.", owner);
        }
    }

    private static void AddIssue(List<object> issues, int limit, string severity, string code, string message, GameObject target)
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
            target = target != null ? UiToolUtility.ElementSnapshot(target) : null
        });
    }
}
}
