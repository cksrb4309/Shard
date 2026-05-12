using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityComponent = UnityEngine.Component;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace UnityCliTools.Component
{

[UnityCliTool(
    Name = UnityCliToolNames.InspectComponents,
    Description = "Inspect serialized component data for a GameObject.",
    Group = UnityCliToolGroups.Component)]
public static class InspectComponentsTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object. If omitted, searches all loaded scenes.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Optional component type filter.")]
        public string ComponentType { get; set; }

        [ToolParameter("Zero-based index among components with the same type.", DefaultValue = "0")]
        public int ComponentIndex { get; set; }

        [ToolParameter("Include all components on the target GameObject.", DefaultValue = "true")]
        public bool IncludeAllComponents { get; set; }

        [ToolParameter("Max number of serialized properties per component.", DefaultValue = "200")]
        public int PropertyLimit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var componentType = p.Get("componentType", string.Empty)?.Trim();
            var componentIndex = Math.Max(0, p.GetInt("componentIndex") ?? 0);
            var includeAllComponents = p.GetBool("includeAllComponents", true);
            var propertyLimit = Math.Max(1, Math.Min(2000, p.GetInt("propertyLimit") ?? 200));

            UnityCliToolShared.GuardRequired(hierarchyPath, nameof(hierarchyPath));

            if (!TryResolveTarget(scenePath, hierarchyPath, out var gameObject, out var sceneError))
            {
                return ToolResult.Error(sceneError, new { scenePath, hierarchyPath });
            }

            var components = gameObject.GetComponents<UnityComponent>();
            var rows = new List<object>();
            var missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);

            if (includeAllComponents)
            {
                for (var i = 0; i < components.Length; i++)
                {
                    var component = components[i];
                    if (component == null)
                    {
                        rows.Add(new
                        {
                            index = i,
                            isMissing = true,
                            type = "MissingMonoBehaviour",
                            fullType = "MissingMonoBehaviour",
                            properties = Array.Empty<object>()
                        });
                        continue;
                    }

                    rows.Add(InspectComponent(component, i, propertyLimit));
                }
            }
            else
            {
                var component = ComponentUtility.ResolveComponent(gameObject, componentType, componentIndex);
                if (component == null)
                {
                    return ToolResult.Error(
                        "Target component was not found on GameObject.",
                        new
                        {
                            hierarchyPath,
                            componentType,
                            componentIndex
                        });
                }

                rows.Add(InspectComponent(component, componentIndex, propertyLimit));
            }

            return ToolResult.Success(
                "Components inspected.",
                new
                {
                    target = new
                    {
                        scenePath = gameObject.scene.path,
                        hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                        gameObjectName = gameObject.name,
                        instanceId = gameObject.GetInstanceID(),
                        activeSelf = gameObject.activeSelf,
                        activeInHierarchy = gameObject.activeInHierarchy,
                        componentCount = components.Length,
                        missingScriptCount
                    },
                    includeAllComponents,
                    componentType = componentType ?? string.Empty,
                    componentIndex,
                    returned = rows.Count,
                    components = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect components.");
        }
    }

    private static bool TryResolveTarget(string scenePath, string hierarchyPath, out GameObject gameObject, out string error)
    {
        gameObject = null;
        error = string.Empty;

        UnityScene? targetScene = null;
        if (!string.IsNullOrWhiteSpace(scenePath))
        {
            var foundScene = false;
            foreach (var scene in HierarchyUtility.EnumerateLoadedScenes())
            {
                if (string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    targetScene = scene;
                    foundScene = true;
                    break;
                }
            }

            if (!foundScene)
            {
                error = "Requested scene is not loaded.";
                return false;
            }
        }

        gameObject = HierarchyUtility.FindByHierarchyPath(hierarchyPath, targetScene);
        if (gameObject == null)
        {
            error = "Target GameObject was not found in loaded scenes.";
            return false;
        }

        return true;
    }

    private static object InspectComponent(UnityComponent component, int index, int propertyLimit)
    {
        var serialized = new SerializedObject(component);
        var iterator = serialized.GetIterator();
        var enterChildren = true;
        var properties = new List<object>();
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (properties.Count >= propertyLimit)
            {
                break;
            }

            properties.Add(new
            {
                path = iterator.propertyPath,
                name = iterator.name,
                displayName = iterator.displayName,
                type = iterator.propertyType.ToString(),
                value = ReadPropertyValue(iterator)
            });
        }

        return new
        {
            index,
            isMissing = false,
            type = component.GetType().Name,
            fullType = component.GetType().FullName,
            instanceId = component.GetInstanceID(),
            propertiesCount = properties.Count,
            properties
        };
    }

    private static object ReadPropertyValue(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Boolean:
                return property.boolValue;
            case SerializedPropertyType.Integer:
                return property.longValue;
            case SerializedPropertyType.Float:
                return property.doubleValue;
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.Color:
                return new
                {
                    r = property.colorValue.r,
                    g = property.colorValue.g,
                    b = property.colorValue.b,
                    a = property.colorValue.a
                };
            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue == null
                    ? null
                    : new
                    {
                        name = property.objectReferenceValue.name,
                        type = property.objectReferenceValue.GetType().FullName,
                        instanceId = property.objectReferenceValue.GetInstanceID(),
                        assetPath = AssetDatabase.GetAssetPath(property.objectReferenceValue)
                    };
            case SerializedPropertyType.LayerMask:
                return property.intValue;
            case SerializedPropertyType.Enum:
                return new
                {
                    index = property.enumValueIndex,
                    displayName = property.enumDisplayNames != null && property.enumValueIndex >= 0 && property.enumValueIndex < property.enumDisplayNames.Length
                        ? property.enumDisplayNames[property.enumValueIndex]
                        : property.enumNames != null && property.enumValueIndex >= 0 && property.enumValueIndex < property.enumNames.Length
                            ? property.enumNames[property.enumValueIndex]
                            : string.Empty,
                    names = property.enumNames
                };
            case SerializedPropertyType.Vector2:
                return new { x = property.vector2Value.x, y = property.vector2Value.y };
            case SerializedPropertyType.Vector3:
                return new { x = property.vector3Value.x, y = property.vector3Value.y, z = property.vector3Value.z };
            case SerializedPropertyType.Vector4:
                return new { x = property.vector4Value.x, y = property.vector4Value.y, z = property.vector4Value.z, w = property.vector4Value.w };
            case SerializedPropertyType.Rect:
                return new { x = property.rectValue.x, y = property.rectValue.y, width = property.rectValue.width, height = property.rectValue.height };
            case SerializedPropertyType.Bounds:
                return new
                {
                    center = new { x = property.boundsValue.center.x, y = property.boundsValue.center.y, z = property.boundsValue.center.z },
                    size = new { x = property.boundsValue.size.x, y = property.boundsValue.size.y, z = property.boundsValue.size.z }
                };
            case SerializedPropertyType.Quaternion:
                return new
                {
                    x = property.quaternionValue.x,
                    y = property.quaternionValue.y,
                    z = property.quaternionValue.z,
                    w = property.quaternionValue.w
                };
            case SerializedPropertyType.Vector2Int:
                return new { x = property.vector2IntValue.x, y = property.vector2IntValue.y };
            case SerializedPropertyType.Vector3Int:
                return new { x = property.vector3IntValue.x, y = property.vector3IntValue.y, z = property.vector3IntValue.z };
            case SerializedPropertyType.RectInt:
                return new { x = property.rectIntValue.x, y = property.rectIntValue.y, width = property.rectIntValue.width, height = property.rectIntValue.height };
            case SerializedPropertyType.BoundsInt:
                return new
                {
                    position = new { x = property.boundsIntValue.position.x, y = property.boundsIntValue.position.y, z = property.boundsIntValue.position.z },
                    size = new { x = property.boundsIntValue.size.x, y = property.boundsIntValue.size.y, z = property.boundsIntValue.size.z }
                };
            case SerializedPropertyType.ManagedReference:
                return property.managedReferenceValue == null
                    ? null
                    : new
                    {
                        type = property.managedReferenceFullTypename,
                        value = property.managedReferenceValue.ToString()
                    };
            default:
                return property.propertyType.ToString();
        }
    }
}
}
