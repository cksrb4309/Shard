using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;

namespace UnityCliTools.Component
{

[UnityCliTool(
    Name = UnityCliToolNames.RemoveComponent,
    Description = "Remove one component from a target GameObject.",
    Group = UnityCliToolGroups.Component)]
public static class RemoveComponentTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Component type name or full name.", Required = true)]
        public string ComponentType { get; set; }

        [ToolParameter("Zero-based index among components with the same type.", DefaultValue = "0")]
        public int ComponentIndex { get; set; }

        [ToolParameter("Preview only without modifying the object.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var componentType = p.Get("componentType", string.Empty)?.Trim();
            var componentIndex = Math.Max(0, p.GetInt("componentIndex") ?? 0);
            var dryRun = p.GetBool("dryRun", true);

            UnityCliToolShared.GuardRequired(hierarchyPath, nameof(hierarchyPath));
            UnityCliToolShared.GuardRequired(componentType, nameof(componentType));

            if (!TryResolveTarget(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new
                {
                    scenePath = scenePath ?? string.Empty,
                    hierarchyPath
                });
            }

            var component = ComponentUtility.ResolveComponent(target, componentType, componentIndex);
            if (component == null)
            {
                return ToolResult.Error(
                    "Target component was not found on GameObject.",
                    new
                    {
                        hierarchyPath,
                        componentType,
                        componentIndex
                    });
            }

            if (component is Transform)
            {
                return ToolResult.Error("Transform cannot be removed.");
            }

            if (dryRun)
            {
                return ToolResult.Success(
                    "Component removal previewed.",
                    new
                    {
                        dryRun = true,
                        target = DescribeTarget(target),
                        removed = new
                        {
                            type = component.GetType().FullName,
                            instanceId = component.GetInstanceID()
                        }
                    });
            }

            Undo.DestroyObjectImmediate(component);
            EditorSceneManager.MarkSceneDirty(target.scene);

            return ToolResult.Success(
                "Component removed.",
                new
                {
                    dryRun = false,
                    target = DescribeTarget(target),
                    removed = new
                    {
                        type = component.GetType().FullName,
                        instanceId = component.GetInstanceID()
                    }
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to remove component.");
        }
    }

    private static object DescribeTarget(GameObject target)
    {
        return new
        {
            scenePath = target.scene.path,
            hierarchyPath = HierarchyUtility.GetHierarchyPath(target.transform),
            instanceId = target.GetInstanceID(),
            name = target.name
        };
    }

    private static bool TryResolveTarget(string scenePath, string hierarchyPath, out GameObject target, out string error)
    {
        target = null;
        error = string.Empty;

        if (!string.IsNullOrWhiteSpace(scenePath))
        {
            foreach (var loadedScene in HierarchyUtility.EnumerateLoadedScenes())
            {
                if (string.Equals(loadedScene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    target = HierarchyUtility.FindByHierarchyPath(hierarchyPath, loadedScene);
                    if (target != null)
                    {
                        return true;
                    }

                    error = "Target GameObject was not found in the requested scene.";
                    return false;
                }
            }

            error = "Requested scene is not loaded.";
            return false;
        }

        target = HierarchyUtility.FindByHierarchyPath(hierarchyPath, null);
        if (target == null)
        {
            error = "Target GameObject was not found in loaded scenes.";
            return false;
        }

        return true;
    }
}
}
