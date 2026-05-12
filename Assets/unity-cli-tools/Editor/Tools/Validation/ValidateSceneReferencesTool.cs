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
    Name = UnityCliToolNames.ValidateSceneReferences,
    Description = "Validate scene missing scripts and missing object references.",
    Group = UnityCliToolGroups.Validation)]
public static class ValidateSceneReferencesTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path to validate. If omitted, validates all loaded scenes.")]
        public string ScenePath { get; set; }

        [ToolParameter("Include inactive objects in validation.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Max number of returned issues.", DefaultValue = "2000")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePathFilter = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var includeInactive = p.GetBool("includeInactive", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 2000, 1, 10000);

            var issues = new List<object>(Math.Min(limit, 128));
            var missingScriptCount = 0;
            var missingReferenceCount = 0;
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
                    if (issues.Count >= limit)
                    {
                        break;
                    }

                    var missingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                    if (missingScripts > 0)
                    {
                        missingScriptCount += missingScripts;
                        issues.Add(new
                        {
                            severity = "error",
                            code = "missing_script",
                            scenePath = scene.path,
                            hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                            component = "MissingMonoBehaviour",
                            message = $"GameObject has {missingScripts} missing script component(s)."
                        });

                        if (issues.Count >= limit)
                        {
                            break;
                        }
                    }

                    var components = gameObject.GetComponents<UnityComponent>();
                    for (var i = 0; i < components.Length; i++)
                    {
                        if (issues.Count >= limit)
                        {
                            break;
                        }

                        var component = components[i];
                        if (component == null)
                        {
                            continue;
                        }

                        try
                        {
                            var serialized = new SerializedObject(component);
                            var iterator = serialized.GetIterator();
                            var enterChildren = true;
                            while (iterator.NextVisible(enterChildren))
                            {
                                enterChildren = false;
                                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                                {
                                    continue;
                                }

                                if (iterator.objectReferenceValue != null)
                                {
                                    continue;
                                }

                                if (iterator.objectReferenceInstanceIDValue == 0)
                                {
                                    continue;
                                }

                                missingReferenceCount++;
                                issues.Add(new
                                {
                                    severity = "error",
                                    code = "missing_reference",
                                    scenePath = scene.path,
                                    hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                                    component = component.GetType().Name,
                                    property = iterator.propertyPath,
                                    message = "Serialized object reference is missing."
                                });

                                if (issues.Count >= limit)
                                {
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (issues.Count >= limit)
                            {
                                break;
                            }

                            issues.Add(new
                            {
                                severity = "warning",
                                code = "inspection_error",
                                scenePath = scene.path,
                                hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                                component = component.GetType().Name,
                                message = "Component inspection failed.",
                                detail = ex.Message
                            });
                        }
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
                "Scene reference validation completed.",
                new
                {
                    scenePath = scenePathFilter ?? string.Empty,
                    includeInactive,
                    issueLimit = limit,
                    totalIssues = issues.Count,
                    missingScriptCount,
                    missingReferenceCount,
                    issues
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to validate scene references.");
        }
    }
}
}
