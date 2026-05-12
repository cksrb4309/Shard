using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Parameters;
using UnityCliTools.Infrastructure.Reflection;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Automation
{

[UnityCliTool(
    Name = UnityCliToolNames.BatchUpdateSerializedProperty,
    Description = "Update a serialized property across matching loaded scene components.",
    Group = UnityCliToolGroups.Automation)]
public static class BatchUpdateSerializedPropertyTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path. If omitted, scans all loaded scenes.")]
        public string ScenePath { get; set; }

        [ToolParameter("Include inactive objects in traversal.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Only include names containing this text.")]
        public string NameContains { get; set; }

        [ToolParameter("Exact Unity tag filter.")]
        public string Tag { get; set; }

        [ToolParameter("Layer index filter.")]
        public string Layer { get; set; }

        [ToolParameter("Component type to inspect.", Required = true)]
        public string ComponentType { get; set; }

        [ToolParameter("Serialized property path to update.", Required = true)]
        public string PropertyPath { get; set; }

        [ToolParameter("Scalar or string value to assign.")]
        public string Value { get; set; }

        [ToolParameter("Replacement asset path for object reference properties.")]
        public string ReferenceAssetPath { get; set; }

        [ToolParameter("Replacement asset GUID for object reference properties.")]
        public string ReferenceAssetGuid { get; set; }

        [ToolParameter("Replacement scene hierarchy path for object reference properties.")]
        public string ReferenceHierarchyPath { get; set; }

        [ToolParameter("Replacement scene path for hierarchy references.")]
        public string ReferenceScenePath { get; set; }

        [ToolParameter("Optional component type to assign from the replacement scene object.")]
        public string ReferenceComponentType { get; set; }

        [ToolParameter("Zero-based index among replacement components with the same type.", DefaultValue = "0")]
        public int ReferenceComponentIndex { get; set; }

        [ToolParameter("Preview only without modifying the objects.", DefaultValue = "true")]
        public bool DryRun { get; set; }

        [ToolParameter("Max number of affected components to return.", DefaultValue = "200")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var includeInactive = p.GetBool("includeInactive", true);
            var nameContains = p.Get("nameContains", string.Empty)?.Trim();
            var tag = p.Get("tag", string.Empty)?.Trim();
            var layerText = p.Get("layer", string.Empty)?.Trim();
            int? layer = int.TryParse(layerText, out var parsedLayer) ? parsedLayer : (int?)null;
            var componentTypeName = p.Get("componentType", string.Empty)?.Trim();
            var propertyPath = p.Get("propertyPath", string.Empty)?.Trim();
            var dryRun = p.GetBool("dryRun", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 200, 1, 2000);

            UnityCliToolShared.GuardRequired(componentTypeName, nameof(componentTypeName));
            UnityCliToolShared.GuardRequired(propertyPath, nameof(propertyPath));

            if (!TypeUtility.TryResolveComponentType(componentTypeName, out var componentType))
            {
                return ToolResult.Error("componentType could not be resolved to a Unity Component type.");
            }

            var results = new List<object>(Math.Min(limit, 128));
            var matchedComponents = 0;
            var changedComponents = 0;
            var referenceProvided = HasReference(parameters);
            UnityEngine.Object reference = null;
            if (referenceProvided && !TryResolveReference(parameters, out reference, out var referenceError))
            {
                return ToolResult.Error(referenceError);
            }

            var valueToken = parameters["value"];
            var valueText = valueToken?.Type == JTokenType.Null ? string.Empty : valueToken?.ToString();

            foreach (var gameObject in BatchTargetUtility.EnumerateMatchingObjects(scenePath, includeInactive, nameContains, tag, layer))
            {
                if (results.Count >= limit)
                {
                    break;
                }

                var components = gameObject.GetComponents<UnityComponent>();
                foreach (var component in components)
                {
                    if (results.Count >= limit)
                    {
                        break;
                    }

                    if (component == null || !componentType.IsAssignableFrom(component.GetType()))
                    {
                        continue;
                    }

                    var serialized = new SerializedObject(component);
                    var property = serialized.FindProperty(propertyPath);
                    if (property == null)
                    {
                        continue;
                    }

                    matchedComponents++;
                    if (!dryRun)
                    {
                        Undo.RecordObject(component, "Batch Update Serialized Property");
                    }

                    if (!TryApplyPropertyValue(property, valueText, reference, parameters, out var appliedValue, out var error))
                    {
                        results.Add(new
                        {
                            name = gameObject.name,
                            hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                            scenePath = gameObject.scene.path,
                            component = component.GetType().FullName,
                            propertyPath,
                            success = false,
                            error
                        });
                        continue;
                    }

                    if (!dryRun)
                    {
                        serialized.ApplyModifiedProperties();
                        EditorUtility.SetDirty(component);
                        EditorSceneManager.MarkSceneDirty(gameObject.scene);
                    }

                    results.Add(new
                    {
                        name = gameObject.name,
                        hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                        scenePath = gameObject.scene.path,
                        component = component.GetType().FullName,
                        propertyPath,
                        success = true,
                        value = appliedValue
                    });
                    changedComponents++;
                }
            }

            return ToolResult.Success(
                dryRun ? "Serialized property update previewed." : "Serialized property update completed.",
                new
                {
                    dryRun,
                    query = new
                    {
                        scenePath = scenePath ?? string.Empty,
                        includeInactive,
                        nameContains = nameContains ?? string.Empty,
                        tag = tag ?? string.Empty,
                        layer = layer,
                        componentType = componentType.FullName,
                        propertyPath,
                        value = valueText ?? string.Empty,
                        referenceProvided,
                        limit
                    },
                    matchedComponents,
                    changedComponents,
                    returned = results.Count,
                    results
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to batch update serialized properties.");
        }
    }

    private static bool HasReference(JObject parameters)
    {
        return !string.IsNullOrWhiteSpace(parameters["referenceAssetPath"]?.ToString())
            || !string.IsNullOrWhiteSpace(parameters["referenceAssetGuid"]?.ToString())
            || !string.IsNullOrWhiteSpace(parameters["referenceHierarchyPath"]?.ToString());
    }

    private static bool TryResolveReference(
        JObject parameters,
        out UnityEngine.Object reference,
        out string error)
    {
        reference = null;
        error = string.Empty;

        var p = new ToolParams(parameters);
        var referenceAssetGuid = p.Get("referenceAssetGuid", string.Empty)?.Trim();
        if (!string.IsNullOrWhiteSpace(referenceAssetGuid))
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(referenceAssetGuid);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                error = "referenceAssetGuid was not found.";
                return false;
            }

            reference = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (reference == null)
            {
                error = "referenceAssetGuid resolved to an asset path, but the asset is not loadable.";
                return false;
            }

            return true;
        }

        var referenceAssetPath = UnityCliToolShared.TryConvertToAssetPath(p.Get("referenceAssetPath", string.Empty));
        if (!string.IsNullOrWhiteSpace(referenceAssetPath))
        {
            reference = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(referenceAssetPath);
            if (reference == null)
            {
                error = "referenceAssetPath was not found or is not loadable.";
                return false;
            }

            return true;
        }

        var referenceHierarchyPath = p.Get("referenceHierarchyPath", string.Empty)?.Trim();
        if (!string.IsNullOrWhiteSpace(referenceHierarchyPath))
        {
            var referenceScenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("referenceScenePath", string.Empty));
            if (!string.IsNullOrWhiteSpace(referenceScenePath))
            {
                if (!BatchTargetUtility.TryResolveLoadedScene(referenceScenePath, out var scene))
                {
                    error = "referenceScenePath is not loaded.";
                    return false;
                }

                var go = HierarchyUtility.FindByHierarchyPath(referenceHierarchyPath, scene);
                if (go == null)
                {
                    error = "referenceHierarchyPath was not found in the requested scene.";
                    return false;
                }

                var referenceComponentType = p.Get("referenceComponentType", string.Empty)?.Trim();
                if (!string.IsNullOrWhiteSpace(referenceComponentType))
                {
                    if (!TypeUtility.TryResolveComponentType(referenceComponentType, out var type))
                    {
                        error = "referenceComponentType could not be resolved.";
                        return false;
                    }

                    reference = ComponentUtility.ResolveComponent(go, type.FullName ?? type.Name, Math.Max(0, p.GetInt("referenceComponentIndex") ?? 0));
                    if (reference == null)
                    {
                        error = "referenceComponentType was not found on referenceHierarchyPath.";
                        return false;
                    }

                    return true;
                }

                reference = go;
                return true;
            }

            var found = HierarchyUtility.FindByHierarchyPath(referenceHierarchyPath, null);
            if (found == null)
            {
                error = "referenceHierarchyPath was not found in loaded scenes.";
                return false;
            }

            var refComponentType = p.Get("referenceComponentType", string.Empty)?.Trim();
            if (!string.IsNullOrWhiteSpace(refComponentType))
            {
                if (!TypeUtility.TryResolveComponentType(refComponentType, out var type))
                {
                    error = "referenceComponentType could not be resolved.";
                    return false;
                }

                reference = ComponentUtility.ResolveComponent(found, type.FullName ?? type.Name, Math.Max(0, p.GetInt("referenceComponentIndex") ?? 0));
                if (reference == null)
                {
                    error = "referenceComponentType was not found on referenceHierarchyPath.";
                    return false;
                }

                return true;
            }

            reference = found;
            return true;
        }

        error = "One of referenceAssetPath, referenceAssetGuid, or referenceHierarchyPath is required for object reference properties.";
        return false;
    }

    private static bool TryApplyPropertyValue(
        SerializedProperty property,
        string valueText,
        UnityEngine.Object reference,
        JObject parameters,
        out object appliedValue,
        out string error)
    {
        appliedValue = null;
        error = string.Empty;

        switch (property.propertyType)
        {
            case SerializedPropertyType.Boolean:
            {
                if (!bool.TryParse(valueText, out var boolValue))
                {
                    error = "value could not be parsed as bool.";
                    return false;
                }

                property.boolValue = boolValue;
                appliedValue = boolValue;
                return true;
            }
            case SerializedPropertyType.Integer:
            case SerializedPropertyType.LayerMask:
            {
                if (!int.TryParse(valueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
                {
                    error = "value could not be parsed as integer.";
                    return false;
                }

                property.intValue = intValue;
                appliedValue = intValue;
                return true;
            }
            case SerializedPropertyType.Float:
            {
                if (!float.TryParse(valueText, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue))
                {
                    error = "value could not be parsed as float.";
                    return false;
                }

                property.floatValue = floatValue;
                appliedValue = floatValue;
                return true;
            }
            case SerializedPropertyType.String:
                property.stringValue = valueText ?? string.Empty;
                appliedValue = property.stringValue;
                return true;
            case SerializedPropertyType.Color:
            {
                if (!ParamUtility.TryReadColor(parameters, "value", out var color))
                {
                    error = "value could not be parsed as color.";
                    return false;
                }

                property.colorValue = color;
                appliedValue = new
                {
                    color.r,
                    color.g,
                    color.b,
                    color.a
                };
                return true;
            }
            case SerializedPropertyType.Vector2:
            {
                if (!ParamUtility.TryReadVector2(parameters, "value", out var vector))
                {
                    error = "value could not be parsed as vector2.";
                    return false;
                }

                property.vector2Value = vector;
                appliedValue = new { x = vector.x, y = vector.y };
                return true;
            }
            case SerializedPropertyType.Vector3:
            {
                if (!ParamUtility.TryReadVector3(parameters, "value", out var vector))
                {
                    error = "value could not be parsed as vector3.";
                    return false;
                }

                property.vector3Value = vector;
                appliedValue = new { x = vector.x, y = vector.y, z = vector.z };
                return true;
            }
            case SerializedPropertyType.Vector4:
            {
                if (!ParamUtility.TryReadVector4(parameters, "value", out var vector))
                {
                    error = "value could not be parsed as vector4.";
                    return false;
                }

                property.vector4Value = vector;
                appliedValue = new { x = vector.x, y = vector.y, z = vector.z, w = vector.w };
                return true;
            }
            case SerializedPropertyType.Rect:
            {
                if (!ParamUtility.TryReadVector4(parameters, "value", out var vector))
                {
                    error = "value could not be parsed as rect.";
                    return false;
                }

                property.rectValue = new Rect(vector.x, vector.y, vector.z, vector.w);
                appliedValue = new { x = vector.x, y = vector.y, width = vector.z, height = vector.w };
                return true;
            }
            case SerializedPropertyType.Bounds:
            {
                if (!ParamUtility.TryReadVector3(parameters, "value", out var vector))
                {
                    error = "value could not be parsed as bounds center.";
                    return false;
                }

                property.boundsValue = new Bounds(vector, property.boundsValue.size);
                appliedValue = new
                {
                    center = new { x = vector.x, y = vector.y, z = vector.z },
                    size = new { x = property.boundsValue.size.x, y = property.boundsValue.size.y, z = property.boundsValue.size.z }
                };
                return true;
            }
            case SerializedPropertyType.Quaternion:
            {
                if (!ParamUtility.TryReadQuaternion(parameters, "value", out var quaternion))
                {
                    error = "value could not be parsed as quaternion.";
                    return false;
                }

                property.quaternionValue = quaternion;
                appliedValue = new { x = quaternion.x, y = quaternion.y, z = quaternion.z, w = quaternion.w };
                return true;
            }
            case SerializedPropertyType.Enum:
            {
                if (int.TryParse(valueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var enumIndex))
                {
                    property.enumValueIndex = enumIndex;
                    appliedValue = enumIndex;
                    return true;
                }

                for (var i = 0; i < property.enumNames.Length; i++)
                {
                    if (string.Equals(property.enumNames[i], valueText, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(property.enumDisplayNames[i], valueText, StringComparison.OrdinalIgnoreCase))
                    {
                        property.enumValueIndex = i;
                        appliedValue = property.enumNames[i];
                        return true;
                    }
                }

                error = "value could not be resolved to an enum value.";
                return false;
            }
            case SerializedPropertyType.ObjectReference:
                if (reference == null)
                {
                    error = "reference source is required for object reference properties.";
                    return false;
                }

                property.objectReferenceValue = reference;
                appliedValue = new
                {
                    name = reference.name,
                    type = reference.GetType().FullName,
                    instanceId = reference.GetInstanceID(),
                    assetPath = AssetDatabase.GetAssetPath(reference)
                };
                return true;
            default:
                error = $"Unsupported property type: {property.propertyType}.";
                return false;
        }
    }
}
}
