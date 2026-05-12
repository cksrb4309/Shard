using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Reflection;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;

namespace UnityCliTools.Component
{

[UnityCliTool(
    Name = UnityCliToolNames.AddComponent,
    Description = "Add one component to a target GameObject.",
    Group = UnityCliToolGroups.Component)]
public static class AddComponentTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Component type name or full name.", Required = true)]
        public string ComponentType { get; set; }

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
            var componentTypeName = p.Get("componentType", string.Empty)?.Trim();
            var dryRun = p.GetBool("dryRun", true);

            UnityCliToolShared.GuardRequired(hierarchyPath, nameof(hierarchyPath));
            UnityCliToolShared.GuardRequired(componentTypeName, nameof(componentTypeName));

            if (!TryResolveTarget(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new
                {
                    scenePath = scenePath ?? string.Empty,
                    hierarchyPath
                });
            }

            if (!TypeUtility.TryResolveComponentType(componentTypeName, out var componentType))
            {
                return ToolResult.Error(
                    "componentType could not be resolved to a Unity Component type.",
                    new { componentType = componentTypeName });
            }

            if (dryRun)
            {
                return ToolResult.Success(
                    "Component addition previewed.",
                    new
                    {
                        dryRun = true,
                        target = DescribeTarget(target),
                        componentType = componentType.FullName
                    });
            }

            var added = Undo.AddComponent(target, componentType);
            EditorSceneManager.MarkSceneDirty(target.scene);

            return ToolResult.Success(
                "Component added.",
                new
                {
                    dryRun = false,
                    target = DescribeTarget(target),
                    added = added == null
                        ? null
                        : new
                        {
                            type = added.GetType().FullName,
                            instanceId = added.GetInstanceID()
                        }
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to add component.");
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
