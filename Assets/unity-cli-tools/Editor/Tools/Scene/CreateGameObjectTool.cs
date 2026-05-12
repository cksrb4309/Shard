using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace UnityCliTools.Scene
{

[UnityCliTool(
    Name = UnityCliToolNames.CreateGameObject,
    Description = "Create a GameObject in a loaded scene.",
    Group = UnityCliToolGroups.Scene)]
public static class CreateGameObjectTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path. If omitted, uses the active scene.")]
        public string ScenePath { get; set; }

        [ToolParameter("Parent hierarchy path for the new object.")]
        public string ParentHierarchyPath { get; set; }

        [ToolParameter("New GameObject name.", DefaultValue = "GameObject")]
        public string Name { get; set; }

        [ToolParameter("Set active state on creation.", DefaultValue = "true")]
        public bool Active { get; set; }

        [ToolParameter("Preview only without creating the object.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var parentHierarchyPath = p.Get("parentHierarchyPath", string.Empty)?.Trim();
            var name = string.IsNullOrWhiteSpace(p.Get("name", string.Empty)) ? "GameObject" : p.Get("name", string.Empty).Trim();
            var active = p.GetBool("active", true);
            var dryRun = p.GetBool("dryRun", true);

            if (!TryResolveTargetScene(scenePath, parentHierarchyPath, out var targetScene, out var parent, out var error))
            {
                return ToolResult.Error(error, new
                {
                    scenePath = scenePath ?? string.Empty,
                    parentHierarchyPath = parentHierarchyPath ?? string.Empty
                });
            }

            var parentPath = parent != null ? HierarchyUtility.GetHierarchyPath(parent.transform) : string.Empty;
            if (dryRun)
            {
                return ToolResult.Success(
                    "GameObject creation previewed.",
                    new
                    {
                        dryRun = true,
                        targetScenePath = targetScene.path,
                        parentHierarchyPath = parentPath,
                        name,
                        active
                    });
            }

            var created = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(created, "Create GameObject");
            if (targetScene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(created, targetScene);
            }

            if (parent != null)
            {
                created.transform.SetParent(parent.transform, false);
            }

            created.SetActive(active);
            EditorSceneManager.MarkSceneDirty(created.scene);

            return ToolResult.Success(
                "GameObject created.",
                new
                {
                    dryRun = false,
                    created = new
                    {
                        name = created.name,
                        hierarchyPath = HierarchyUtility.GetHierarchyPath(created.transform),
                        scenePath = created.scene.path,
                        instanceId = created.GetInstanceID(),
                        activeSelf = created.activeSelf,
                        activeInHierarchy = created.activeInHierarchy,
                        parentHierarchyPath = parentPath
                    }
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to create GameObject.");
        }
    }

    private static bool TryResolveTargetScene(
        string scenePath,
        string parentHierarchyPath,
        out UnityScene targetScene,
        out GameObject parent,
        out string error)
    {
        parent = null;
        error = string.Empty;

        if (!string.IsNullOrWhiteSpace(scenePath))
        {
            foreach (var loadedScene in HierarchyUtility.EnumerateLoadedScenes())
            {
                if (string.Equals(loadedScene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    targetScene = loadedScene;
                    if (!string.IsNullOrWhiteSpace(parentHierarchyPath))
                    {
                        parent = HierarchyUtility.FindByHierarchyPath(parentHierarchyPath, targetScene);
                        if (parent == null)
                        {
                            error = "Parent hierarchy path was not found in the requested scene.";
                            return false;
                        }
                    }

                    return true;
                }
            }

            targetScene = default;
            error = "Requested scene is not loaded.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(parentHierarchyPath))
        {
            parent = HierarchyUtility.FindByHierarchyPath(parentHierarchyPath, null);
            if (parent == null)
            {
                targetScene = default;
                error = "Parent hierarchy path was not found in loaded scenes.";
                return false;
            }

            targetScene = parent.scene;
            return true;
        }

        targetScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            error = "No active loaded scene is available.";
            return false;
        }

        return true;
    }
}
}
