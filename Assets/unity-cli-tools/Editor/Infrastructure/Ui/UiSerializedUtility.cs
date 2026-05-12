using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Reflection;
using UnityCliTools.Infrastructure.Selection;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Infrastructure.Ui
{

public static class UiSerializedUtility
{
    public static bool TryResolveComponent(
        string scenePath,
        string hierarchyPath,
        string componentTypeName,
        int componentIndex,
        out UnityComponent component,
        out GameObject gameObject,
        out string error)
    {
        component = null;
        gameObject = null;
        error = string.Empty;

        if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out gameObject, out error))
        {
            return false;
        }

        if (!TypeUtility.TryResolveComponentType(componentTypeName, out var componentType))
        {
            error = "componentType could not be resolved to a Unity Component type.";
            return false;
        }

        var components = gameObject.GetComponents<UnityComponent>();
        var matched = 0;
        foreach (var candidate in components)
        {
            if (candidate == null || !componentType.IsAssignableFrom(candidate.GetType()))
            {
                continue;
            }

            if (matched == componentIndex)
            {
                component = candidate;
                return true;
            }

            matched++;
        }

        error = "Target component was not found on GameObject.";
        return false;
    }

    public static bool TryResolveObjectReference(
        JObject parameters,
        string hierarchyKey,
        string assetPathKey,
        string assetGuidKey,
        string scenePathKey,
        string componentTypeKey,
        string componentIndexKey,
        out UnityEngine.Object reference,
        out string source,
        out string error)
    {
        reference = null;
        source = string.Empty;
        error = string.Empty;

        var p = new ToolParams(parameters);
        var assetGuid = p.Get(assetGuidKey, string.Empty)?.Trim();
        if (!string.IsNullOrWhiteSpace(assetGuid))
        {
            var path = AssetDatabase.GUIDToAssetPath(assetGuid);
            if (string.IsNullOrWhiteSpace(path))
            {
                error = assetGuidKey + " was not found.";
                return false;
            }

            reference = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            source = assetGuidKey;
            if (reference == null)
            {
                error = assetGuidKey + " resolved to an unloadable asset.";
                return false;
            }

            return true;
        }

        var assetPath = UnityCliToolShared.TryConvertToAssetPath(p.Get(assetPathKey, string.Empty));
        if (!string.IsNullOrWhiteSpace(assetPath))
        {
            reference = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            source = assetPathKey;
            if (reference == null)
            {
                error = assetPathKey + " was not found or is not loadable.";
                return false;
            }

            return true;
        }

        var hierarchyPath = p.Get(hierarchyKey, string.Empty)?.Trim();
        if (!string.IsNullOrWhiteSpace(hierarchyPath))
        {
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get(scenePathKey, string.Empty));
            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var go, out error))
            {
                return false;
            }

            var componentTypeName = p.Get(componentTypeKey, string.Empty)?.Trim();
            if (!string.IsNullOrWhiteSpace(componentTypeName))
            {
                var componentIndex = Math.Max(0, p.GetInt(componentIndexKey) ?? 0);
                if (!TypeUtility.TryResolveComponentType(componentTypeName, out var componentType))
                {
                    error = componentTypeKey + " could not be resolved.";
                    return false;
                }

                reference = ComponentUtility.ResolveComponent(go, componentType.FullName ?? componentType.Name, componentIndex);
                source = componentTypeKey;
                if (reference == null)
                {
                    error = componentTypeKey + " was not found on referenced GameObject.";
                    return false;
                }

                return true;
            }

            reference = go;
            source = hierarchyKey;
            return true;
        }

        error = "No object reference source was provided.";
        return false;
    }
}
}
