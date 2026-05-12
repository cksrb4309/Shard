using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace UnityCliTools.Infrastructure.Selection
{

public static class HierarchyUtility
{
    public static IEnumerable<UnityScene> EnumerateLoadedScenes()
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.IsValid() && scene.isLoaded)
            {
                yield return scene;
            }
        }
    }

    public static IEnumerable<GameObject> EnumerateSceneObjects(UnityScene scene, bool includeInactive)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            yield break;
        }

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var item in EnumerateHierarchy(root, includeInactive))
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<GameObject> EnumerateHierarchy(GameObject root, bool includeInactive)
    {
        if (root == null)
        {
            yield break;
        }

        var stack = new Stack<Transform>();
        stack.Push(root.transform);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == null)
            {
                continue;
            }

            var gameObject = current.gameObject;
            if (includeInactive || gameObject.activeInHierarchy)
            {
                yield return gameObject;
            }

            for (var i = current.childCount - 1; i >= 0; i--)
            {
                var child = current.GetChild(i);
                if (child != null)
                {
                    stack.Push(child);
                }
            }
        }
    }

    public static string GetHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            return string.Empty;
        }

        var names = new Stack<string>();
        var cursor = transform;
        while (cursor != null)
        {
            names.Push(cursor.name);
            cursor = cursor.parent;
        }

        return string.Join("/", names);
    }

    public static GameObject FindByHierarchyPath(string hierarchyPath, UnityScene? targetScene = null)
    {
        if (string.IsNullOrWhiteSpace(hierarchyPath))
        {
            return null;
        }

        var normalizedPath = hierarchyPath.Replace('\\', '/').Trim('/');
        foreach (var scene in EnumerateLoadedScenes())
        {
            if (targetScene.HasValue && scene.path != targetScene.Value.path)
            {
                continue;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                var path = GetHierarchyPath(root.transform);
                if (string.Equals(path, normalizedPath, StringComparison.Ordinal))
                {
                    return root;
                }

                foreach (var child in EnumerateHierarchy(root, includeInactive: true))
                {
                    var childPath = GetHierarchyPath(child.transform);
                    if (string.Equals(childPath, normalizedPath, StringComparison.Ordinal))
                    {
                        return child;
                    }
                }
            }
        }

        return null;
    }

    public static string SafeTag(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return string.Empty;
        }

        try
        {
            return gameObject.tag;
        }
        catch
        {
            return string.Empty;
        }
    }
}
}
