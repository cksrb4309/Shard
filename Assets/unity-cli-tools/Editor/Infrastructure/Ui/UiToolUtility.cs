using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Selection;
using UnityScene = UnityEngine.SceneManagement.Scene;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Infrastructure.Ui
{

public static class UiToolUtility
{
    public static bool TryResolveGameObject(string scenePath, string hierarchyPath, out GameObject target, out string error)
    {
        target = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(hierarchyPath))
        {
            error = "hierarchyPath is required.";
            return false;
        }

        scenePath = UnityCliToolShared.TryConvertToAssetPath(scenePath);
        if (!string.IsNullOrWhiteSpace(scenePath))
        {
            foreach (var scene in HierarchyUtility.EnumerateLoadedScenes())
            {
                if (!string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                target = HierarchyUtility.FindByHierarchyPath(hierarchyPath, scene);
                if (target != null)
                {
                    return true;
                }

                error = "Target GameObject was not found in the requested scene.";
                return false;
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

    public static IEnumerable<GameObject> EnumerateLoadedSceneObjects(string scenePath, bool includeInactive)
    {
        scenePath = UnityCliToolShared.TryConvertToAssetPath(scenePath);
        foreach (var scene in HierarchyUtility.EnumerateLoadedScenes())
        {
            if (!string.IsNullOrWhiteSpace(scenePath)
                && !string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var item in HierarchyUtility.EnumerateSceneObjects(scene, includeInactive))
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<GameObject> EnumerateChildren(GameObject root, bool includeInactive, int maxDepth)
    {
        if (root == null)
        {
            yield break;
        }

        var stack = new Stack<Tuple<Transform, int>>();
        stack.Push(Tuple.Create(root.transform, 0));
        while (stack.Count > 0)
        {
            var item = stack.Pop();
            var transform = item.Item1;
            var depth = item.Item2;
            if (transform == null)
            {
                continue;
            }

            var go = transform.gameObject;
            if (includeInactive || go.activeInHierarchy)
            {
                yield return go;
            }

            if (depth >= maxDepth)
            {
                continue;
            }

            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                stack.Push(Tuple.Create(transform.GetChild(i), depth + 1));
            }
        }
    }

    public static int GetDepth(Transform root, Transform target)
    {
        var depth = 0;
        var cursor = target;
        while (cursor != null && cursor != root)
        {
            depth++;
            cursor = cursor.parent;
        }

        return depth;
    }

    public static object RectSnapshot(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            return null;
        }

        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return new
        {
            anchorMin = Vector2Snapshot(rectTransform.anchorMin),
            anchorMax = Vector2Snapshot(rectTransform.anchorMax),
            pivot = Vector2Snapshot(rectTransform.pivot),
            anchoredPosition = Vector2Snapshot(rectTransform.anchoredPosition),
            sizeDelta = Vector2Snapshot(rectTransform.sizeDelta),
            offsetMin = Vector2Snapshot(rectTransform.offsetMin),
            offsetMax = Vector2Snapshot(rectTransform.offsetMax),
            rect = new
            {
                x = rectTransform.rect.x,
                y = rectTransform.rect.y,
                width = rectTransform.rect.width,
                height = rectTransform.rect.height
            },
            localScale = Vector3Snapshot(rectTransform.localScale),
            worldCorners = new[]
            {
                Vector3Snapshot(corners[0]),
                Vector3Snapshot(corners[1]),
                Vector3Snapshot(corners[2]),
                Vector3Snapshot(corners[3])
            }
        };
    }

    public static object ElementSnapshot(GameObject gameObject, Transform root = null)
    {
        if (gameObject == null)
        {
            return null;
        }

        var rectTransform = gameObject.transform as RectTransform;
        var text = gameObject.GetComponent<TMP_Text>();
        var image = gameObject.GetComponent<Image>();
        var button = gameObject.GetComponent<Button>();
        var selectable = gameObject.GetComponent<Selectable>();
        var graphic = gameObject.GetComponent<Graphic>();
        var components = gameObject.GetComponents<UnityComponent>();
        var componentNames = new List<string>();
        foreach (var component in components)
        {
            componentNames.Add(component != null ? component.GetType().Name : "MissingScript");
        }

        var inScene = gameObject.scene.IsValid() && gameObject.scene.isLoaded;
        return new
        {
            name = gameObject.name,
            hierarchyPath = inScene ? HierarchyUtility.GetHierarchyPath(gameObject.transform) : string.Empty,
            scenePath = inScene ? gameObject.scene.path : string.Empty,
            depth = root != null ? GetDepth(root, gameObject.transform) : 0,
            activeSelf = gameObject.activeSelf,
            activeInHierarchy = gameObject.activeInHierarchy,
            layer = gameObject.layer,
            tag = HierarchyUtility.SafeTag(gameObject),
            componentNames,
            rectTransform = RectSnapshot(rectTransform),
            text = text != null ? new { value = text.text, textType = text.GetType().Name, raycastTarget = text.raycastTarget } : null,
            image = image != null ? new { sprite = image.sprite != null ? image.sprite.name : string.Empty, color = ColorSnapshot(image.color), raycastTarget = image.raycastTarget, type = image.type.ToString() } : null,
            button = button != null ? new { interactable = button.interactable, onClickCount = button.onClick.GetPersistentEventCount() } : null,
            selectable = selectable != null ? new { interactable = selectable.interactable, navigationMode = selectable.navigation.mode.ToString() } : null,
            graphic = graphic != null ? new { raycastTarget = graphic.raycastTarget, color = ColorSnapshot(graphic.color) } : null,
            instanceId = gameObject.GetInstanceID()
        };
    }

    public static object ColorSnapshot(Color color)
    {
        return new { r = color.r, g = color.g, b = color.b, a = color.a };
    }

    public static object Vector2Snapshot(Vector2 value)
    {
        return new { x = value.x, y = value.y };
    }

    public static object Vector3Snapshot(Vector3 value)
    {
        return new { x = value.x, y = value.y, z = value.z };
    }

    public static object Vector4Snapshot(Vector4 value)
    {
        return new { x = value.x, y = value.y, z = value.z, w = value.w };
    }

    public static void MarkUiObjectDirty(GameObject gameObject, string undoName)
    {
        if (gameObject == null)
        {
            return;
        }

        EditorUtility.SetDirty(gameObject);
        var scene = gameObject.scene;
        if (scene.IsValid() && scene.isLoaded)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }

    public static bool TryResolveSprite(string path, string guid, out Sprite sprite, out string resolvedPath)
    {
        sprite = null;
        resolvedPath = string.Empty;

        if (!string.IsNullOrWhiteSpace(guid))
        {
            resolvedPath = AssetDatabase.GUIDToAssetPath(guid.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(path))
        {
            resolvedPath = UnityCliToolShared.TryConvertToAssetPath(path);
        }

        if (string.IsNullOrWhiteSpace(resolvedPath))
        {
            return false;
        }

        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(resolvedPath);
        return sprite != null;
    }

    public static GameObject CreateUiGameObject(string name, Transform parent, params Type[] components)
    {
        var go = new GameObject(name, components);
        Undo.RegisterCreatedObjectUndo(go, "Create UI Element");
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }

        var rect = go.transform as RectTransform;
        if (rect != null)
        {
            rect.localScale = Vector3.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(160f, 30f);
        }

        return go;
    }

    public static void EnsureEventSystem(UnityScene targetScene)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go != null
                && go.scene.IsValid()
                && go.scene.isLoaded
                && string.Equals(go.scene.path, targetScene.path, StringComparison.OrdinalIgnoreCase)
                && go.GetComponent<EventSystem>() != null)
            {
                return;
            }
        }

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        if (targetScene.IsValid() && targetScene.isLoaded)
        {
            SceneManager.MoveGameObjectToScene(eventSystem, targetScene);
            EditorSceneManager.MarkSceneDirty(targetScene);
        }
    }
}
}
