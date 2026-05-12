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

namespace UnityCliTools.Validation
{

[UnityCliTool(
    Name = UnityCliToolNames.ValidateSelection,
    Description = "Validate missing scripts and object references on the current selection.",
    Group = UnityCliToolGroups.Validation)]
public static class ValidateSelectionTool
{
    public sealed class Parameters
    {
        [ToolParameter("Include inactive selected scene objects.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Max number of returned issues.", DefaultValue = "200")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var includeInactive = p.GetBool("includeInactive", true);
            var limit = Math.Max(1, Math.Min(2000, p.GetInt("limit") ?? 200));

            var selected = Selection.objects ?? Array.Empty<UnityEngine.Object>();
            var targets = new List<GameObject>();
            var seen = new HashSet<int>();
            foreach (var selectedObject in selected)
            {
                var gameObject = selectedObject as GameObject ?? (selectedObject as UnityComponent)?.gameObject;
                if (gameObject == null)
                {
                    continue;
                }

                var instanceId = gameObject.GetInstanceID();
                if (!seen.Add(instanceId))
                {
                    continue;
                }

                if (!includeInactive && !gameObject.activeInHierarchy)
                {
                    continue;
                }

                targets.Add(gameObject);
            }

            var issues = new List<object>();
            var missingScriptCount = 0;
            var missingReferenceCount = 0;

            foreach (var gameObject in targets)
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
                        scenePath = gameObject.scene.path,
                        hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                        component = "MissingMonoBehaviour",
                        message = $"GameObject has {missingScripts} missing script component(s)."
                    });
                }

                var components = gameObject.GetComponents<UnityComponent>();
                foreach (var component in components)
                {
                    if (issues.Count >= limit)
                    {
                        break;
                    }

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

                            if (iterator.objectReferenceValue != null || iterator.objectReferenceInstanceIDValue == 0)
                            {
                                continue;
                            }

                            missingReferenceCount++;
                            issues.Add(new
                            {
                                severity = "error",
                                code = "missing_reference",
                                scenePath = gameObject.scene.path,
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
                        issues.Add(new
                        {
                            severity = "warning",
                            code = "inspection_error",
                            scenePath = gameObject.scene.path,
                            hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                            component = component.GetType().Name,
                            message = "Component inspection failed.",
                            detail = ex.Message
                        });
                    }
                }
            }

            return ToolResult.Success(
                "Selection validated.",
                new
                {
                    includeInactive,
                    selectionCount = selected.Length,
                    targetCount = targets.Count,
                    issueLimit = limit,
                    totalIssues = issues.Count,
                    missingScriptCount,
                    missingReferenceCount,
                    issues
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to validate selection.");
        }
    }
}
}
