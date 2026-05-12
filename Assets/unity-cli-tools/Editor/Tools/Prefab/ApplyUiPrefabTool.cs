using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Parameters;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Prefab
{

[UnityCliTool(
    Name = UnityCliToolNames.ApplyUiPrefab,
    Description = "Instantiate a UI prefab into a loaded scene under an optional parent.",
    Group = UnityCliToolGroups.Prefab)]
public static class ApplyUiPrefabTool
{
    public sealed class Parameters
    {
        [ToolParameter("Prefab name.")]
        public string PrefabName { get; set; }

        [ToolParameter("Prefab asset path.")]
        public string PrefabPath { get; set; }

        [ToolParameter("Prefab asset GUID.")]
        public string PrefabGuid { get; set; }

        [ToolParameter("Target loaded scene path. If omitted, uses parent scene or active scene.")]
        public string ScenePath { get; set; }

        [ToolParameter("Parent hierarchy path.")]
        public string ParentHierarchyPath { get; set; }

        [ToolParameter("Override instantiated name.")]
        public string Name { get; set; }

        [ToolParameter("Anchored position as x,y.")]
        public string AnchoredPosition { get; set; }

        [ToolParameter("Size delta as x,y.")]
        public string SizeDelta { get; set; }

        [ToolParameter("Preview only without instantiating the prefab.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            if (!PrefabResolver.TryResolvePrefab(parameters, out var prefab, out var prefabPath, out var prefabGuid, out var error))
            {
                return ToolResult.Error(error);
            }

            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var parentPath = p.Get("parentHierarchyPath", string.Empty)?.Trim();
            var name = p.Get("name", string.Empty)?.Trim();
            var dryRun = p.GetBool("dryRun", true);

            if (!TryResolveTarget(scenePath, parentPath, out var scene, out var parent, out error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, parentHierarchyPath = parentPath ?? string.Empty });
            }

            if (dryRun)
            {
                return ToolResult.Success(
                    "UI prefab instantiation previewed.",
                    new { dryRun, prefabPath, prefabGuid, scenePath = scene.path, parentHierarchyPath = parentPath ?? string.Empty, name = name ?? string.Empty });
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (instance == null)
            {
                return ToolResult.Error("Prefab could not be instantiated.", new { prefabPath });
            }

            Undo.RegisterCreatedObjectUndo(instance, "Apply UI Prefab");
            if (parent != null)
            {
                instance.transform.SetParent(parent.transform, false);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                instance.name = name;
            }

            ApplyRect(parameters, instance.transform as RectTransform);
            EditorSceneManager.MarkSceneDirty(scene);

            return ToolResult.Success(
                "UI prefab instantiated.",
                new { dryRun = false, instance = UiToolUtility.ElementSnapshot(instance), prefabPath, prefabGuid });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to apply UI prefab.");
        }
    }

    private static bool TryResolveTarget(string scenePath, string parentPath, out UnityEngine.SceneManagement.Scene scene, out GameObject parent, out string error)
    {
        parent = null;
        error = string.Empty;
        if (!string.IsNullOrWhiteSpace(parentPath))
        {
            if (!UiToolUtility.TryResolveGameObject(scenePath, parentPath, out parent, out error))
            {
                scene = default;
                return false;
            }

            scene = parent.scene;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(scenePath))
        {
            foreach (var loadedScene in HierarchyUtility.EnumerateLoadedScenes())
            {
                if (string.Equals(loadedScene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    scene = loadedScene;
                    return true;
                }
            }

            scene = default;
            error = "Requested scene is not loaded.";
            return false;
        }

        scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            error = "No active loaded scene is available.";
            return false;
        }

        return true;
    }

    private static void ApplyRect(JObject parameters, RectTransform rect)
    {
        if (rect == null) return;
        if (ParamUtility.TryReadVector2(parameters, "anchoredPosition", out var position)) rect.anchoredPosition = position;
        if (ParamUtility.TryReadVector2(parameters, "sizeDelta", out var size)) rect.sizeDelta = size;
    }
}
}
