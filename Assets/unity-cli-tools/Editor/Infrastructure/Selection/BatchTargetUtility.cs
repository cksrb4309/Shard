using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace UnityCliTools.Infrastructure.Selection
{

public static class BatchTargetUtility
{
    public static bool TryResolveLoadedScene(string scenePath, out UnityScene scene)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            scene = default;
            return false;
        }

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

    public static IEnumerable<GameObject> EnumerateMatchingObjects(
        string scenePath,
        bool includeInactive,
        string nameContains,
        string tag,
        int? layer)
    {
        foreach (var scene in HierarchyUtility.EnumerateLoadedScenes())
        {
            if (!string.IsNullOrWhiteSpace(scenePath)
                && !string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var gameObject in HierarchyUtility.EnumerateSceneObjects(scene, includeInactive))
            {
                if (Matches(gameObject, nameContains, tag, layer))
                {
                    yield return gameObject;
                }
            }
        }
    }

    public static bool Matches(GameObject gameObject, string nameContains, string tag, int? layer)
    {
        if (gameObject == null)
        {
            return false;
        }

        if (layer.HasValue && gameObject.layer != layer.Value)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var safeTag = HierarchyUtility.SafeTag(gameObject);
            if (!string.Equals(safeTag, tag, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(nameContains)
            && gameObject.name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) < 0)
        {
            return false;
        }

        return true;
    }
}
}
