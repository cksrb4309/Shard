using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;

namespace UnityCliTools.Scene
{

[UnityCliTool(
    Name = UnityCliToolNames.SetActive,
    Description = "Set the active state of a scene GameObject.",
    Group = UnityCliToolGroups.Scene)]
public static class SetActiveTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Desired active state.", DefaultValue = "true")]
        public bool Active { get; set; }

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
            var active = p.GetBool("active", true);
            var dryRun = p.GetBool("dryRun", true);

            UnityCliToolShared.GuardRequired(hierarchyPath, nameof(hierarchyPath));

            if (!TryResolveTarget(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new
                {
                    scenePath = scenePath ?? string.Empty,
                    hierarchyPath
                });
            }

            var before = new
            {
                activeSelf = target.activeSelf,
                activeInHierarchy = target.activeInHierarchy
            };

            if (!dryRun)
            {
                Undo.RecordObject(target, "Set Active");
                target.SetActive(active);
                EditorUtility.SetDirty(target);
                EditorSceneManager.MarkSceneDirty(target.scene);
            }

            return ToolResult.Success(
                dryRun ? "Active state change previewed." : "Active state updated.",
                new
                {
                    dryRun,
                    target = new
                    {
                        scenePath = target.scene.path,
                        hierarchyPath = HierarchyUtility.GetHierarchyPath(target.transform),
                        instanceId = target.GetInstanceID()
                    },
                    before,
                    after = new
                    {
                        activeSelf = target.activeSelf,
                        activeInHierarchy = target.activeInHierarchy
                    }
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set active state.");
        }
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
