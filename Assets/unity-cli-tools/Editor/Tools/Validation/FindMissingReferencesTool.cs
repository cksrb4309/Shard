using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Validation
{

[UnityCliTool(
    Name = UnityCliToolNames.FindMissingReferences,
    Description = "Find missing serialized object references in loaded scenes with repair identifiers.",
    Group = UnityCliToolGroups.Validation)]
public static class FindMissingReferencesTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path to scan. If omitted, scans all loaded scenes.")]
        public string ScenePath { get; set; }

        [ToolParameter("Include inactive objects during traversal.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Only include a specific component type name or full name.")]
        public string ComponentType { get; set; }

        [ToolParameter("Only include a specific serialized property path.")]
        public string PropertyPath { get; set; }

        [ToolParameter("Max number of returned missing references.", DefaultValue = "500")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePathFilter = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var includeInactive = p.GetBool("includeInactive", true);
            var componentTypeFilter = p.Get("componentType", string.Empty)?.Trim();
            var propertyPathFilter = p.Get("propertyPath", string.Empty)?.Trim();
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 500, 1, 5000);

            var rows = new List<object>(Math.Min(limit, 128));
            var inspectedObjectCount = 0;
            var inspectedComponentCount = 0;
            var matchedSceneCount = 0;

            foreach (var scene in HierarchyUtility.EnumerateLoadedScenes())
            {
                if (!string.IsNullOrWhiteSpace(scenePathFilter)
                    && !string.Equals(scene.path, scenePathFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                matchedSceneCount++;
                foreach (var gameObject in HierarchyUtility.EnumerateSceneObjects(scene, includeInactive))
                {
                    if (rows.Count >= limit)
                    {
                        break;
                    }

                    inspectedObjectCount++;
                    var components = gameObject.GetComponents<UnityComponent>();
                    var componentTypeOrdinals = new Dictionary<string, int>(StringComparer.Ordinal);
                    for (var i = 0; i < components.Length; i++)
                    {
                        if (rows.Count >= limit)
                        {
                            break;
                        }

                        var component = components[i];
                        if (component == null)
                        {
                            continue;
                        }

                        var componentType = component.GetType();
                        if (!MatchesComponentType(componentType, componentTypeFilter))
                        {
                            continue;
                        }

                        inspectedComponentCount++;
                        var componentTypeName = componentType.Name;
                        var componentIndex = GetAndIncrement(componentTypeOrdinals, componentType.FullName ?? componentTypeName);
                        AppendMissingReferenceRows(
                            rows,
                            limit,
                            scene.path,
                            gameObject,
                            component,
                            componentTypeName,
                            componentType.FullName ?? componentTypeName,
                            componentIndex,
                            propertyPathFilter);
                    }
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
                "Missing references searched.",
                new
                {
                    query = new
                    {
                        scenePath = scenePathFilter ?? string.Empty,
                        includeInactive,
                        componentType = componentTypeFilter ?? string.Empty,
                        propertyPath = propertyPathFilter ?? string.Empty,
                        limit
                    },
                    inspectedObjectCount,
                    inspectedComponentCount,
                    returned = rows.Count,
                    results = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to find missing references.");
        }
    }

    private static void AppendMissingReferenceRows(
        List<object> rows,
        int limit,
        string scenePath,
        GameObject gameObject,
        UnityComponent component,
        string componentType,
        string componentFullType,
        int componentIndex,
        string propertyPathFilter)
    {
        var serialized = new SerializedObject(component);
        var iterator = serialized.GetIterator();
        var enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (rows.Count >= limit)
            {
                return;
            }

            if (iterator.propertyType != SerializedPropertyType.ObjectReference)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(propertyPathFilter)
                && !string.Equals(iterator.propertyPath, propertyPathFilter, StringComparison.Ordinal))
            {
                continue;
            }

            if (iterator.objectReferenceValue != null || iterator.objectReferenceInstanceIDValue == 0)
            {
                continue;
            }

            var hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform);
            rows.Add(new
            {
                scenePath,
                hierarchyPath,
                targetInstanceId = gameObject.GetInstanceID(),
                componentType,
                componentFullType,
                componentIndex,
                componentInstanceId = component.GetInstanceID(),
                propertyPath = iterator.propertyPath,
                propertyDisplayName = iterator.displayName,
                missingReferenceInstanceId = iterator.objectReferenceInstanceIDValue,
                setReferenceInput = new
                {
                    scenePath,
                    hierarchyPath,
                    componentType = componentFullType,
                    componentIndex,
                    propertyPath = iterator.propertyPath
                }
            });
        }
    }

    private static bool MatchesComponentType(Type type, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return true;
        }

        return string.Equals(type.Name, filter, StringComparison.Ordinal)
            || string.Equals(type.FullName, filter, StringComparison.Ordinal)
            || string.Equals(type.Name, filter, StringComparison.OrdinalIgnoreCase)
            || string.Equals(type.FullName, filter, StringComparison.OrdinalIgnoreCase);
    }

    private static int GetAndIncrement(Dictionary<string, int> ordinals, string key)
    {
        ordinals.TryGetValue(key, out var value);
        ordinals[key] = value + 1;
        return value;
    }
}
}
