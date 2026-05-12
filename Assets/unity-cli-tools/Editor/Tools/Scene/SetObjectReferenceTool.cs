using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Reflection;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityScene = UnityEngine.SceneManagement.Scene;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Scene
{

[UnityCliTool(
    Name = UnityCliToolNames.SetObjectReference,
    Description = "Assign a serialized object reference on a scene component.",
    Group = UnityCliToolGroups.Scene)]
public static class SetObjectReferenceTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Target component type name or full name.", Required = true)]
        public string ComponentType { get; set; }

        [ToolParameter("Zero-based index among components with the same type.", DefaultValue = "0")]
        public int ComponentIndex { get; set; }

        [ToolParameter("Serialized object reference property path.", Required = true)]
        public string PropertyPath { get; set; }

        [ToolParameter("Replacement scene object hierarchy path.")]
        public string ReferenceHierarchyPath { get; set; }

        [ToolParameter("Optional component type to assign from the replacement scene object.")]
        public string ReferenceComponentType { get; set; }

        [ToolParameter("Zero-based index among replacement components with the same type.", DefaultValue = "0")]
        public int ReferenceComponentIndex { get; set; }

        [ToolParameter("Loaded scene path containing the replacement scene object.")]
        public string ReferenceScenePath { get; set; }

        [ToolParameter("Replacement asset path.")]
        public string ReferenceAssetPath { get; set; }

        [ToolParameter("Replacement asset guid.")]
        public string ReferenceAssetGuid { get; set; }

        [ToolParameter("Allow overwriting an existing non-null reference.", DefaultValue = "false")]
        public bool AllowOverwrite { get; set; }

        [ToolParameter("Preview the operation without modifying the scene.", DefaultValue = "true")]
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
            var propertyPath = p.Get("propertyPath", string.Empty)?.Trim();
            var componentIndex = Math.Max(0, p.GetInt("componentIndex") ?? 0);
            var allowOverwrite = p.GetBool("allowOverwrite", false);
            var dryRun = p.GetBool("dryRun", true);

            UnityCliToolShared.GuardRequired(hierarchyPath, nameof(hierarchyPath));
            UnityCliToolShared.GuardRequired(componentType, nameof(componentType));
            UnityCliToolShared.GuardRequired(propertyPath, nameof(propertyPath));

            if (!TryResolveLoadedScene(scenePath, out var targetScene, out var sceneError))
            {
                return ToolResult.Error(
                    sceneError,
                    new
                    {
                        scenePath
                    });
            }

            var gameObject = HierarchyUtility.FindByHierarchyPath(hierarchyPath, targetScene);
            if (gameObject == null)
            {
                return ToolResult.Error(
                    "Target GameObject was not found in loaded scenes.",
                    new
                    {
                        scenePath,
                        hierarchyPath
                    });
            }

            var component = ResolveComponent(gameObject, componentType, componentIndex);
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

            var reference = ResolveReference(parameters, out var referenceSource, out var referenceError);
            if (reference == null)
            {
                return ToolResult.Error(
                    referenceError,
                    new
                    {
                        hierarchyPath,
                        componentType,
                        componentIndex,
                        propertyPath
                    });
            }

            var serialized = new SerializedObject(component);
            var property = serialized.FindProperty(propertyPath);
            if (property == null)
            {
                return ToolResult.Error(
                    "Serialized property was not found.",
                    new
                    {
                        hierarchyPath,
                        componentType,
                        componentIndex,
                        propertyPath
                    });
            }

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return ToolResult.Error(
                    "Serialized property is not an object reference.",
                    new
                    {
                        propertyPath,
                        propertyType = property.propertyType.ToString()
                    });
            }

            var previousReference = property.objectReferenceValue;
            if (previousReference != null && !allowOverwrite)
            {
                return ToolResult.Error(
                    "Property already has a reference. Set allowOverwrite=true to replace it.",
                    new
                    {
                        propertyPath,
                        previousReferenceName = previousReference.name,
                        previousReferenceType = previousReference.GetType().FullName
                    });
            }

            if (!dryRun)
            {
                Undo.RecordObject(component, "Set object reference");
                property.objectReferenceValue = reference;
                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(component);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            return ToolResult.Success(
                dryRun ? "Object reference assignment previewed." : "Object reference assigned.",
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
                    reference = new
                    {
                        name = reference.name,
                        type = reference.GetType().FullName,
                        source = referenceSource,
                        assetPath = AssetDatabase.GetAssetPath(reference)
                    },
                    previousReference = previousReference == null
                        ? null
                        : new
                        {
                            name = previousReference.name,
                            type = previousReference.GetType().FullName,
                            assetPath = AssetDatabase.GetAssetPath(previousReference)
                        },
                    sceneMarkedDirty = !dryRun
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set object reference.");
        }
    }

    private static bool TryResolveLoadedScene(string scenePath, out UnityScene? scene, out string error)
    {
        scene = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(scenePath))
        {
            return true;
        }

        foreach (var loadedScene in HierarchyUtility.EnumerateLoadedScenes())
        {
            if (string.Equals(loadedScene.path, scenePath, StringComparison.OrdinalIgnoreCase))
            {
                scene = loadedScene;
                return true;
            }
        }

        error = "Requested scene is not loaded.";
        return false;
    }

    private static UnityComponent ResolveComponent(GameObject gameObject, string componentType, int componentIndex)
    {
        if (!TypeUtility.TryResolveComponentType(componentType, out var resolvedType))
        {
            return null;
        }

        var components = gameObject.GetComponents<UnityComponent>();
        var matched = 0;
        for (var i = 0; i < components.Length; i++)
        {
            var component = components[i];
            if (component == null)
            {
                continue;
            }

            if (!resolvedType.IsAssignableFrom(component.GetType()))
            {
                continue;
            }

            if (matched == componentIndex)
            {
                return component;
            }

            matched++;
        }

        return null;
    }

    private static UnityEngine.Object ResolveReference(
        JObject parameters,
        out string source,
        out string error)
    {
        var p = new ToolParams(parameters);
        source = string.Empty;
        error = string.Empty;

        var referenceAssetGuid = p.Get("referenceAssetGuid", string.Empty)?.Trim();
        if (!string.IsNullOrWhiteSpace(referenceAssetGuid))
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(referenceAssetGuid);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                error = "referenceAssetGuid was not found.";
                return null;
            }

            source = "assetGuid";
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                error = "referenceAssetGuid resolved to a path, but the asset is not loadable.";
            }

            return asset;
        }

        var referenceAssetPath = UnityCliToolShared.TryConvertToAssetPath(p.Get("referenceAssetPath", string.Empty));
        if (!string.IsNullOrWhiteSpace(referenceAssetPath))
        {
            source = "assetPath";
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(referenceAssetPath);
            if (asset == null)
            {
                error = "referenceAssetPath was not found or is not loadable.";
            }

            return asset;
        }

        var referenceHierarchyPath = p.Get("referenceHierarchyPath", string.Empty)?.Trim();
        if (!string.IsNullOrWhiteSpace(referenceHierarchyPath))
        {
            var referenceScenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("referenceScenePath", string.Empty));
            var referenceComponentType = p.Get("referenceComponentType", string.Empty)?.Trim();
            var referenceComponentIndex = Math.Max(0, p.GetInt("referenceComponentIndex") ?? 0);
            if (!TryResolveLoadedScene(referenceScenePath, out var referenceScene, out var referenceSceneError))
            {
                error = referenceSceneError;
                return null;
            }

            source = "hierarchyPath";
            var gameObject = HierarchyUtility.FindByHierarchyPath(referenceHierarchyPath, referenceScene);
            if (gameObject == null)
            {
                error = "referenceHierarchyPath was not found in loaded scenes.";
                return null;
            }

            if (!string.IsNullOrWhiteSpace(referenceComponentType))
            {
                var component = ResolveComponent(gameObject, referenceComponentType, referenceComponentIndex);
                if (component == null)
                {
                    error = "referenceComponentType was not found on referenceHierarchyPath.";
                    return null;
                }

                source = "hierarchyComponent";
                return component;
            }

            return gameObject;
        }

        error = "One of referenceHierarchyPath, referenceAssetPath, or referenceAssetGuid is required.";
        return null;
    }
}
}
