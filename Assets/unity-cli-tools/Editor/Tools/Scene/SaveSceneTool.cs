using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;

namespace UnityCliTools.Scene
{

[UnityCliTool(
    Name = UnityCliToolNames.SaveScene,
    Description = "Save the active scene, a loaded scene, or all loaded scenes.",
    Group = UnityCliToolGroups.Scene)]
public static class SaveSceneTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path to save. If omitted, saves the active scene.", DefaultValue = "")]
        public string ScenePath { get; set; }

        [ToolParameter("Save every loaded scene instead of a single scene.", DefaultValue = "false")]
        public bool SaveAllLoadedScenes { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePathFilter = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var saveAllLoadedScenes = p.GetBool("saveAllLoadedScenes", false);

            var scenes = new List<UnityEngine.SceneManagement.Scene>();
            if (saveAllLoadedScenes)
            {
                scenes.AddRange(HierarchyUtility.EnumerateLoadedScenes());
            }
            else if (!string.IsNullOrWhiteSpace(scenePathFilter))
            {
                if (!TryFindLoadedScene(scenePathFilter, out var scene))
                {
                    return ToolResult.Error(
                        "Requested scene is not loaded.",
                        new
                        {
                            scenePath = scenePathFilter
                        });
                }

                scenes.Add(scene);
            }
            else
            {
                var activeScene = EditorSceneManager.GetActiveScene();
                if (!activeScene.IsValid() || !activeScene.isLoaded)
                {
                    return ToolResult.Error("No active loaded scene is available to save.");
                }

                scenes.Add(activeScene);
            }

            var results = new List<object>(scenes.Count);
            var savedCount = 0;
            foreach (var scene in scenes)
            {
                var sceneName = scene.name;
                var scenePath = scene.path;
                var wasDirty = scene.isDirty;

                if (string.IsNullOrWhiteSpace(scenePath))
                {
                    results.Add(new
                    {
                        name = sceneName,
                        path = string.Empty,
                        wasDirty,
                        saved = false,
                        message = "Scene has no asset path."
                    });
                    continue;
                }

                var saved = EditorSceneManager.SaveScene(scene);
                if (saved)
                {
                    savedCount++;
                }

                results.Add(new
                {
                    name = sceneName,
                    path = scenePath,
                    wasDirty,
                    saved
                });
            }

            return ToolResult.Success(
                saveAllLoadedScenes ? "Loaded scenes saved." : "Scene saved.",
                new
                {
                    saveAllLoadedScenes,
                    requestedScenePath = scenePathFilter ?? string.Empty,
                    savedCount,
                    results
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to save scene.");
        }
    }

    private static bool TryFindLoadedScene(string scenePath, out UnityEngine.SceneManagement.Scene scene)
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
        return false;
    }
}
}
