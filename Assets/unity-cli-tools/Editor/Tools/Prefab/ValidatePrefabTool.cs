using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Parameters;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Prefab
{

[UnityCliTool(
    Name = UnityCliToolNames.ValidatePrefab,
    Description = "Validate prefab missing scripts and missing object references.",
    Group = UnityCliToolGroups.Prefab)]
public static class ValidatePrefabTool
{
    public sealed class Parameters
    {
        [ToolParameter("Prefab name.")]
        public string PrefabName { get; set; }

        [ToolParameter("Prefab asset path.")]
        public string PrefabPath { get; set; }

        [ToolParameter("Prefab asset GUID.")]
        public string PrefabGuid { get; set; }

        [ToolParameter("Include inactive objects in validation.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Max number of returned issues.", DefaultValue = "2000")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            if (!PrefabResolver.TryResolvePrefab(parameters, out var prefab, out var prefabPath, out var prefabGuid, out var error))
            {
                return ToolResult.Error(error);
            }

            var p = new ToolParams(parameters);
            var includeInactive = p.GetBool("includeInactive", true);
            var limit = Math.Max(1, Math.Min(10000, p.GetInt("limit") ?? 2000));

            var issues = new List<object>(Math.Min(limit, 128));
            var missingScriptCount = 0;
            var missingReferenceCount = 0;
            var root = PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                foreach (var gameObject in HierarchyUtility.EnumerateHierarchy(root, includeInactive))
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
                            prefabPath,
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
                                    prefabPath,
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
                                prefabPath,
                                hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                                component = component.GetType().Name,
                                message = "Component inspection failed.",
                                detail = ex.Message
                            });
                        }
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            return ToolResult.Success(
                "Prefab validation completed.",
                new
                {
                    prefab = new
                    {
                        name = prefab.name,
                        path = prefabPath,
                        guid = prefabGuid,
                        prefabAssetType = PrefabUtility.GetPrefabAssetType(prefab).ToString()
                    },
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
            return ToolResult.FromException(ex, "Failed to validate prefab.");
        }
    }
}
}
