using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityScene = UnityEngine.SceneManagement.Scene;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Hierarchy
{

[UnityCliTool(
    Name = UnityCliToolNames.DumpHierarchy,
    Description = "Dump a compact hierarchy tree from a root object or scene roots.",
    Group = UnityCliToolGroups.Hierarchy)]
public static class DumpHierarchyTool
{
    public sealed class Parameters
    {
        [ToolParameter("Hierarchy path for the root object, e.g. Root/UI/Panel.")]
        public string RootPath { get; set; }

        [ToolParameter("Loaded scene path to scope the dump.")]
        public string ScenePath { get; set; }

        [ToolParameter("Max child depth (0 means root only).", DefaultValue = "3")]
        public int MaxDepth { get; set; }

        [ToolParameter("Include component type names on each node.", DefaultValue = "false")]
        public bool IncludeComponents { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var rootPath = p.Get("rootPath", string.Empty)?.Trim();
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var maxDepth = ParamUtility.ClampOrDefault(p.GetInt("maxDepth"), 3, 0, 16);
            var includeComponents = p.GetBool("includeComponents", false);

            var targetScene = ResolveLoadedScene(scenePath);
            var roots = ResolveRoots(rootPath, targetScene);
            if (roots.Count == 0)
            {
                return ToolResult.Error(
                    "No matching roots found.",
                    new
                    {
                        rootPath = rootPath ?? string.Empty,
                        scenePath = targetScene?.path ?? string.Empty
                    });
            }

            var trees = roots.Select(root => BuildNode(root.transform, 0, maxDepth, includeComponents)).ToArray();
            return ToolResult.Success(
                "Hierarchy dumped.",
                new
                {
                    rootCount = trees.Length,
                    maxDepth,
                    includeComponents,
                    trees
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to dump hierarchy.");
        }
    }

    private static UnityScene? ResolveLoadedScene(string scenePath)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            return null;
        }

        foreach (var scene in HierarchyUtility.EnumerateLoadedScenes())
        {
            if (string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase))
            {
                return scene;
            }
        }

        return null;
    }

    private static List<GameObject> ResolveRoots(string rootPath, UnityScene? targetScene)
    {
        if (!string.IsNullOrWhiteSpace(rootPath))
        {
            var matched = HierarchyUtility.FindByHierarchyPath(rootPath, targetScene);
            return matched != null ? new List<GameObject> { matched } : new List<GameObject>();
        }

        var roots = new List<GameObject>();
        if (targetScene.HasValue)
        {
            roots.AddRange(targetScene.Value.GetRootGameObjects());
            return roots;
        }

        foreach (var scene in HierarchyUtility.EnumerateLoadedScenes())
        {
            roots.AddRange(scene.GetRootGameObjects());
        }

        return roots;
    }

    private static object BuildNode(Transform transform, int depth, int maxDepth, bool includeComponents)
    {
        var gameObject = transform.gameObject;
        var children = new List<object>();
        var hasMoreChildren = false;

        if (depth < maxDepth)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                children.Add(BuildNode(transform.GetChild(i), depth + 1, maxDepth, includeComponents));
            }
        }
        else
        {
            hasMoreChildren = transform.childCount > 0;
        }

        return new
        {
            name = gameObject.name,
            hierarchyPath = HierarchyUtility.GetHierarchyPath(transform),
            scenePath = gameObject.scene.path,
            instanceId = gameObject.GetInstanceID(),
            activeSelf = gameObject.activeSelf,
            activeInHierarchy = gameObject.activeInHierarchy,
            childCount = transform.childCount,
            hasMoreChildren,
            components = includeComponents
                ? gameObject.GetComponents<UnityComponent>()
                    .Where(component => component != null)
                    .Select(component => component.GetType().Name)
                    .ToArray()
                : Array.Empty<string>(),
            children
        };
    }
}
}
