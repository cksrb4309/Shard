using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Parameters;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Component
{

[UnityCliTool(
    Name = UnityCliToolNames.FindMissingScripts,
    Description = "Find GameObjects or prefabs with missing MonoBehaviour scripts.",
    Group = UnityCliToolGroups.Component)]
public static class FindMissingScriptsTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path to scan. If omitted, scans all loaded scenes.")]
        public string ScenePath { get; set; }

        [ToolParameter("Prefab asset name to scan.")]
        public string PrefabName { get; set; }

        [ToolParameter("Prefab asset path to scan.")]
        public string PrefabPath { get; set; }

        [ToolParameter("Prefab asset GUID to scan.")]
        public string PrefabGuid { get; set; }

        [ToolParameter("Include inactive objects during traversal.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Max number of returned results.", DefaultValue = "500")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePathFilter = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var includeInactive = p.GetBool("includeInactive", true);
            var limit = Math.Max(1, Math.Min(5000, p.GetInt("limit") ?? 500));
            var prefabRequested = !string.IsNullOrWhiteSpace(p.Get("prefabName", string.Empty))
                || !string.IsNullOrWhiteSpace(p.Get("prefabPath", string.Empty))
                || !string.IsNullOrWhiteSpace(p.Get("prefabGuid", string.Empty));

            var results = new List<object>(Math.Min(limit, 128));

            if (prefabRequested)
            {
                if (!PrefabResolver.TryResolvePrefab(parameters, out var prefab, out var prefabPath, out var prefabGuid, out var error))
                {
                    return ToolResult.Error(error);
                }

                ScanPrefab(results, prefabPath, prefabGuid, includeInactive, limit);
                return ToolResult.Success(
                    "Missing scripts searched in prefab.",
                    new
                    {
                        scope = "prefab",
                        prefab = new
                        {
                            name = prefab.name,
                            path = prefabPath,
                            guid = prefabGuid
                        },
                        includeInactive,
                        limit,
                        returned = results.Count,
                        results
                    });
            }

            var matchedSceneCount = 0;
            foreach (var scene in HierarchyUtility.EnumerateLoadedScenes())
            {
                if (!string.IsNullOrWhiteSpace(scenePathFilter)
                    && !string.Equals(scene.path, scenePathFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                matchedSceneCount++;
                ScanScene(results, scene, includeInactive, limit);
                if (results.Count >= limit)
                {
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(scenePathFilter) && matchedSceneCount == 0)
            {
                return ToolResult.Error(
                    "Requested scene is not loaded.",
                    new
                    {
                        scenePath = scenePathFilter
                    });
            }

            return ToolResult.Success(
                "Missing scripts searched.",
                new
                {
                    scope = "scene",
                    scenePath = scenePathFilter ?? string.Empty,
                    includeInactive,
                    limit,
                    returned = results.Count,
                    results
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to find missing scripts.");
        }
    }

    private static void ScanScene(List<object> results, UnityEngine.SceneManagement.Scene scene, bool includeInactive, int limit)
    {
        foreach (var gameObject in HierarchyUtility.EnumerateSceneObjects(scene, includeInactive))
        {
            if (results.Count >= limit)
            {
                return;
            }

            var missingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (missingScripts <= 0)
            {
                continue;
            }

            results.Add(new
            {
                scope = "scene",
                scenePath = scene.path,
                hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                gameObjectName = gameObject.name,
                instanceId = gameObject.GetInstanceID(),
                missingScriptCount = missingScripts
            });
        }
    }

    private static void ScanPrefab(List<object> results, string prefabPath, string prefabGuid, bool includeInactive, int limit)
    {
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            foreach (var gameObject in HierarchyUtility.EnumerateHierarchy(root, includeInactive))
            {
                if (results.Count >= limit)
                {
                    return;
                }

                var missingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                if (missingScripts <= 0)
                {
                    continue;
                }

                results.Add(new
                {
                    scope = "prefab",
                    prefabPath,
                    prefabGuid,
                    hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                    gameObjectName = gameObject.name,
                    instanceId = gameObject.GetInstanceID(),
                    missingScriptCount = missingScripts
                });
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
}
