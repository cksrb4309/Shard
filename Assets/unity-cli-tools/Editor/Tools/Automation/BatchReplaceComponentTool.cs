using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Reflection;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Automation
{

[UnityCliTool(
    Name = UnityCliToolNames.BatchReplaceComponent,
    Description = "Replace one component type with another across loaded scenes.",
    Group = UnityCliToolGroups.Automation)]
public static class BatchReplaceComponentTool
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

        [ToolParameter("Source component type to replace.", Required = true)]
        public string SourceType { get; set; }

        [ToolParameter("Replacement component type to add.", Required = true)]
        public string TargetType { get; set; }

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
            var sourceTypeName = p.Get("sourceType", string.Empty)?.Trim();
            var targetTypeName = p.Get("targetType", string.Empty)?.Trim();
            var dryRun = p.GetBool("dryRun", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 200, 1, 2000);

            UnityCliToolShared.GuardRequired(sourceTypeName, nameof(sourceTypeName));
            UnityCliToolShared.GuardRequired(targetTypeName, nameof(targetTypeName));

            if (!TypeUtility.TryResolveComponentType(sourceTypeName, out var sourceType))
            {
                return ToolResult.Error("sourceType could not be resolved to a Unity Component type.");
            }

            if (!TypeUtility.TryResolveComponentType(targetTypeName, out var targetType))
            {
                return ToolResult.Error("targetType could not be resolved to a Unity Component type.");
            }

            if (sourceType == targetType)
            {
                return ToolResult.Error("sourceType and targetType must be different.");
            }

            var results = new List<object>(Math.Min(limit, 128));
            var replaced = 0;
            var matchedObjects = 0;

            foreach (var gameObject in BatchTargetUtility.EnumerateMatchingObjects(scenePath, includeInactive, nameContains, tag, layer))
            {
                if (results.Count >= limit)
                {
                    break;
                }

                var components = gameObject.GetComponents<UnityComponent>();
                var sourceComponents = new List<UnityComponent>();
                for (var i = 0; i < components.Length; i++)
                {
                    var component = components[i];
                    if (component == null)
                    {
                        continue;
                    }

                    if (sourceType.IsAssignableFrom(component.GetType()))
                    {
                        sourceComponents.Add(component);
                    }
                }

                if (sourceComponents.Count == 0)
                {
                    continue;
                }

                matchedObjects++;
                foreach (var sourceComponent in sourceComponents)
                {
                    if (results.Count >= limit)
                    {
                        break;
                    }

                    if (dryRun)
                    {
                        var sourceInstanceId = sourceComponent.GetInstanceID();
                        results.Add(new
                        {
                            name = gameObject.name,
                            hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                            scenePath = gameObject.scene.path,
                            source = sourceComponent.GetType().FullName,
                            target = targetType.FullName,
                            sourceInstanceId
                        });
                        replaced++;
                        continue;
                    }

                    var sourceInstanceIdBeforeDestroy = sourceComponent.GetInstanceID();
                    Undo.RecordObject(gameObject, "Batch Replace Component");
                    var added = Undo.AddComponent(gameObject, targetType);
                    EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(sourceComponent), added);
                    Undo.DestroyObjectImmediate(sourceComponent);
                    EditorUtility.SetDirty(gameObject);
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);

                    results.Add(new
                    {
                        name = gameObject.name,
                        hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                        scenePath = gameObject.scene.path,
                        source = sourceComponent.GetType().FullName,
                        target = targetType.FullName,
                        sourceInstanceId = sourceInstanceIdBeforeDestroy,
                        targetInstanceId = added.GetInstanceID()
                    });
                    replaced++;
                }
            }

            return ToolResult.Success(
                dryRun ? "Batch component replacement previewed." : "Batch component replacement completed.",
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
                        sourceType = sourceType.FullName,
                        targetType = targetType.FullName,
                        limit
                    },
                    matchedObjects,
                    replaced,
                    returned = results.Count,
                    results
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to batch replace components.");
        }
    }
}
}
