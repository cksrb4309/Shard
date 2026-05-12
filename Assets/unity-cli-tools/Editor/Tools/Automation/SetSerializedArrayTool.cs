using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Automation
{

[UnityCliTool(
    Name = UnityCliToolNames.SetSerializedArray,
    Description = "Resize a serialized array/list and assign object reference elements.",
    Group = UnityCliToolGroups.Automation)]
public static class SetSerializedArrayTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Target component type name or full name.", Required = true)]
        public string ComponentType { get; set; }

        [ToolParameter("Zero-based index among target components of the same type.", DefaultValue = "0")]
        public int ComponentIndex { get; set; }

        [ToolParameter("Serialized array/list property path.", Required = true)]
        public string PropertyPath { get; set; }

        [ToolParameter("New array size. If omitted and referenceHierarchyPaths is provided, uses that count.")]
        public int Size { get; set; }

        [ToolParameter("Single element index to assign.", DefaultValue = "-1")]
        public int Index { get; set; }

        [ToolParameter("Clear assigned object references to null.", DefaultValue = "false")]
        public bool Clear { get; set; }

        [ToolParameter("Single reference scene object hierarchy path.")]
        public string ReferenceHierarchyPath { get; set; }

        [ToolParameter("Multiple reference scene object hierarchy paths.")]
        public string ReferenceHierarchyPaths { get; set; }

        [ToolParameter("Loaded scene path containing reference objects.")]
        public string ReferenceScenePath { get; set; }

        [ToolParameter("Component type to assign from each reference object.")]
        public string ReferenceComponentType { get; set; }

        [ToolParameter("Zero-based index among reference components.", DefaultValue = "0")]
        public int ReferenceComponentIndex { get; set; }

        [ToolParameter("Single reference asset path.")]
        public string ReferenceAssetPath { get; set; }

        [ToolParameter("Single reference asset GUID.")]
        public string ReferenceAssetGuid { get; set; }

        [ToolParameter("Preview only without modifying the component.", DefaultValue = "true")]
        public bool DryRun { get; set; }
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
            var propertyPath = p.Get("propertyPath", string.Empty)?.Trim();
            var requestedSize = p.GetInt("size");
            var index = p.GetInt("index") ?? -1;
            var clear = p.GetBool("clear", false);
            var dryRun = p.GetBool("dryRun", true);

            UnityCliToolShared.GuardRequired(hierarchyPath, nameof(hierarchyPath));
            UnityCliToolShared.GuardRequired(componentType, nameof(componentType));
            UnityCliToolShared.GuardRequired(propertyPath, nameof(propertyPath));

            if (!UiSerializedUtility.TryResolveComponent(scenePath, hierarchyPath, componentType, componentIndex, out var component, out var gameObject, out var error))
            {
                return ToolResult.Error(error, new { scenePath, hierarchyPath, componentType, componentIndex });
            }

            var serialized = new SerializedObject(component);
            var property = serialized.FindProperty(propertyPath);
            if (property == null)
            {
                return ToolResult.Error("Serialized property was not found.", new { propertyPath });
            }

            if (!property.isArray || property.propertyType == SerializedPropertyType.String)
            {
                return ToolResult.Error("Serialized property is not an array/list.", new { propertyPath, propertyType = property.propertyType.ToString() });
            }

            var referencePaths = ParamUtility.ReadStringList(parameters, "referenceHierarchyPaths");
            var references = ResolveReferences(parameters, referencePaths, out error);
            if (!string.IsNullOrWhiteSpace(error))
            {
                return ToolResult.Error(error);
            }

            var finalSize = requestedSize ?? property.arraySize;
            if (!requestedSize.HasValue && references.Count > 0 && index < 0)
            {
                finalSize = references.Count;
            }

            if (index >= 0)
            {
                finalSize = Math.Max(finalSize, index + 1);
            }

            if (finalSize < 0)
            {
                return ToolResult.Error("size cannot be negative.", new { size = finalSize });
            }

            var before = Snapshot(property, 50);
            var plan = new
            {
                currentSize = property.arraySize,
                finalSize,
                assignCount = references.Count,
                index,
                clear
            };

            if (!dryRun)
            {
                Undo.RecordObject(component, "Set Serialized Array");
                property.arraySize = finalSize;

                if (clear)
                {
                    for (var i = 0; i < property.arraySize; i++)
                    {
                        var element = property.GetArrayElementAtIndex(i);
                        if (element.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            element.objectReferenceValue = null;
                        }
                    }
                }

                if (references.Count > 0)
                {
                    if (index >= 0)
                    {
                        AssignReference(property.GetArrayElementAtIndex(index), references[0]);
                    }
                    else
                    {
                        for (var i = 0; i < references.Count && i < property.arraySize; i++)
                        {
                            AssignReference(property.GetArrayElementAtIndex(i), references[i]);
                        }
                    }
                }

                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(component);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            return ToolResult.Success(
                dryRun ? "Serialized array update previewed." : "Serialized array updated.",
                new
                {
                    dryRun,
                    target = new
                    {
                        scenePath = gameObject.scene.path,
                        hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                        componentType = component.GetType().FullName,
                        componentIndex,
                        propertyPath
                    },
                    plan,
                    before,
                    after = dryRun ? before : Snapshot(property, 50)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set serialized array.");
        }
    }

    private static List<UnityEngine.Object> ResolveReferences(JObject parameters, IReadOnlyList<string> referencePaths, out string error)
    {
        error = string.Empty;
        var references = new List<UnityEngine.Object>();
        if (referencePaths.Count > 0)
        {
            foreach (var path in referencePaths)
            {
                var clone = new JObject(parameters);
                clone["referenceHierarchyPath"] = path;
                if (!UiSerializedUtility.TryResolveObjectReference(
                    clone,
                    "referenceHierarchyPath",
                    "referenceAssetPath",
                    "referenceAssetGuid",
                    "referenceScenePath",
                    "referenceComponentType",
                    "referenceComponentIndex",
                    out var reference,
                    out _,
                    out error))
                {
                    return references;
                }

                references.Add(reference);
            }

            return references;
        }

        if (HasSingleReference(parameters))
        {
            if (!UiSerializedUtility.TryResolveObjectReference(
                parameters,
                "referenceHierarchyPath",
                "referenceAssetPath",
                "referenceAssetGuid",
                "referenceScenePath",
                "referenceComponentType",
                "referenceComponentIndex",
                out var reference,
                out _,
                out error))
            {
                return references;
            }

            references.Add(reference);
        }

        return references;
    }

    private static bool HasSingleReference(JObject parameters)
    {
        return !string.IsNullOrWhiteSpace(parameters["referenceHierarchyPath"]?.ToString())
            || !string.IsNullOrWhiteSpace(parameters["referenceAssetPath"]?.ToString())
            || !string.IsNullOrWhiteSpace(parameters["referenceAssetGuid"]?.ToString());
    }

    private static void AssignReference(SerializedProperty element, UnityEngine.Object reference)
    {
        if (element.propertyType != SerializedPropertyType.ObjectReference)
        {
            throw new InvalidOperationException("Array element is not an object reference.");
        }

        element.objectReferenceValue = reference;
    }

    private static object Snapshot(SerializedProperty property, int limit)
    {
        var rows = new List<object>();
        for (var i = 0; i < property.arraySize && rows.Count < limit; i++)
        {
            var element = property.GetArrayElementAtIndex(i);
            rows.Add(new
            {
                index = i,
                propertyType = element.propertyType.ToString(),
                reference = element.propertyType == SerializedPropertyType.ObjectReference && element.objectReferenceValue != null
                    ? new
                    {
                        name = element.objectReferenceValue.name,
                        type = element.objectReferenceValue.GetType().FullName,
                        instanceId = element.objectReferenceValue.GetInstanceID(),
                        assetPath = AssetDatabase.GetAssetPath(element.objectReferenceValue)
                    }
                    : null
            });
        }

        return new { size = property.arraySize, returned = rows.Count, elements = rows };
    }
}
}
