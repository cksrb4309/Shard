using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.SetButtonOnClick,
    Description = "Add, clear, or inspect persistent Button.onClick listeners for parameterless public methods.",
    Group = UnityCliToolGroups.Visual)]
public static class SetButtonOnClickTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the Button.")]
        public string ScenePath { get; set; }

        [ToolParameter("Button GameObject hierarchy path.", Required = true)]
        public string ButtonHierarchyPath { get; set; }

        [ToolParameter("Listener GameObject hierarchy path. Required for add.")]
        public string TargetHierarchyPath { get; set; }

        [ToolParameter("Listener component type name.")]
        public string ComponentType { get; set; }

        [ToolParameter("Public parameterless method name to invoke.")]
        public string MethodName { get; set; }

        [ToolParameter("Action: inspect, add, clear, replace.", DefaultValue = "inspect")]
        public string Action { get; set; }

        [ToolParameter("Preview only without modifying the Button.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = p.Get("scenePath", string.Empty);
            var buttonPath = p.Get("buttonHierarchyPath", string.Empty)?.Trim();
            var targetPath = p.Get("targetHierarchyPath", string.Empty)?.Trim();
            var componentType = p.Get("componentType", string.Empty)?.Trim();
            var methodName = p.Get("methodName", string.Empty)?.Trim();
            var action = (p.Get("action", "inspect") ?? "inspect").Trim().ToLowerInvariant();
            var dryRun = p.GetBool("dryRun", true);

            if (!UiToolUtility.TryResolveGameObject(scenePath, buttonPath, out var buttonGo, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, buttonHierarchyPath = buttonPath ?? string.Empty });
            }

            var button = buttonGo.GetComponent<Button>();
            if (button == null)
            {
                return ToolResult.Error("Target has no Button component.", UiToolUtility.ElementSnapshot(buttonGo));
            }

            var before = Snapshot(button);
            if (action == "inspect")
            {
                return ToolResult.Success("Button onClick inspected.", new { dryRun, button = UiToolUtility.ElementSnapshot(buttonGo), listeners = before });
            }

            if (action != "add" && action != "clear" && action != "replace")
            {
                return ToolResult.Error("Unsupported action. Use inspect, add, clear, or replace.", new { action });
            }

            MonoBehaviour listener = null;
            MethodInfo method = null;
            if (action == "add" || action == "replace")
            {
                if (!TryResolveListener(scenePath, targetPath, componentType, methodName, out listener, out method, out error))
                {
                    return ToolResult.Error(error, new { targetHierarchyPath = targetPath ?? string.Empty, componentType = componentType ?? string.Empty, methodName = methodName ?? string.Empty });
                }
            }

            if (action == "clear" || action == "replace")
            {
                if (!dryRun)
                {
                    Undo.RecordObject(button, "Clear Button onClick");
                    ClearPersistentListeners(button);
                    EditorUtility.SetDirty(button);
                }
            }

            if (action == "add" || action == "replace")
            {
                if (!dryRun)
                {
                    Undo.RecordObject(button, "Set Button onClick");
                    UnityEventTools.AddPersistentListener(button.onClick, (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), listener, method));
                    EditorUtility.SetDirty(button);
                    UiToolUtility.MarkUiObjectDirty(buttonGo, "Set Button onClick");
                }
            }

            return ToolResult.Success(
                dryRun ? "Button onClick change previewed." : "Button onClick updated.",
                new
                {
                    dryRun,
                    action,
                    button = UiToolUtility.ElementSnapshot(buttonGo),
                    before,
                    after = Snapshot(button)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set Button onClick.");
        }
    }

    private static bool TryResolveListener(
        string scenePath,
        string targetPath,
        string componentType,
        string methodName,
        out MonoBehaviour listener,
        out MethodInfo method,
        out string error)
    {
        listener = null;
        method = null;
        error = string.Empty;

        if (!UiToolUtility.TryResolveGameObject(scenePath, targetPath, out var target, out error))
        {
            return false;
        }

        foreach (var component in target.GetComponents<MonoBehaviour>())
        {
            if (component == null)
            {
                continue;
            }

            var type = component.GetType();
            var matchesType = string.IsNullOrWhiteSpace(componentType)
                || string.Equals(type.Name, componentType, StringComparison.OrdinalIgnoreCase)
                || string.Equals(type.FullName, componentType, StringComparison.OrdinalIgnoreCase);
            if (!matchesType)
            {
                continue;
            }

            var candidate = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            if (candidate == null || candidate.ReturnType != typeof(void))
            {
                continue;
            }

            listener = component;
            method = candidate;
            return true;
        }

        error = "Listener component and public void parameterless method could not be resolved.";
        return false;
    }

    private static void ClearPersistentListeners(Button button)
    {
        for (var i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(button.onClick, i);
        }

        button.onClick.RemoveAllListeners();
    }

    private static object Snapshot(Button button)
    {
        var count = button.onClick.GetPersistentEventCount();
        var rows = new object[count];
        for (var i = 0; i < count; i++)
        {
            var target = button.onClick.GetPersistentTarget(i);
            rows[i] = new
            {
                index = i,
                targetName = target != null ? target.name : string.Empty,
                targetType = target != null ? target.GetType().FullName : string.Empty,
                methodName = button.onClick.GetPersistentMethodName(i),
                callState = button.onClick.GetPersistentListenerState(i).ToString()
            };
        }

        return rows;
    }
}
}
