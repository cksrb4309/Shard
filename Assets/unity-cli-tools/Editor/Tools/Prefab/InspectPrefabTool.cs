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
    Name = UnityCliToolNames.InspectPrefab,
    Description = "Inspect prefab hierarchy and component counts.",
    Group = UnityCliToolGroups.Prefab)]
public static class InspectPrefabTool
{
    public sealed class Parameters
    {
        [ToolParameter("Prefab name.")]
        public string PrefabName { get; set; }

        [ToolParameter("Prefab asset path.")]
        public string PrefabPath { get; set; }

        [ToolParameter("Prefab asset GUID.")]
        public string PrefabGuid { get; set; }

        [ToolParameter("Include flattened hierarchy rows.", DefaultValue = "true")]
        public bool IncludeHierarchy { get; set; }

        [ToolParameter("Include inactive objects in hierarchy traversal.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Max number of hierarchy rows.", DefaultValue = "200")]
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
            var includeHierarchy = p.GetBool("includeHierarchy", true);
            var includeInactive = p.GetBool("includeInactive", true);
            var limit = Math.Max(1, Math.Min(2000, p.GetInt("limit") ?? 200));

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var hierarchyRows = new List<object>();
                if (includeHierarchy)
                {
                    foreach (var gameObject in HierarchyUtility.EnumerateHierarchy(root, includeInactive))
                    {
                        if (hierarchyRows.Count >= limit)
                        {
                            break;
                        }

                        hierarchyRows.Add(new
                        {
                            name = gameObject.name,
                            hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                            activeSelf = gameObject.activeSelf,
                            activeInHierarchy = gameObject.activeInHierarchy,
                            tag = HierarchyUtility.SafeTag(gameObject),
                            layer = gameObject.layer,
                            childCount = gameObject.transform.childCount,
                            componentCount = gameObject.GetComponents<UnityComponent>().Length,
                            missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject)
                        });
                    }
                }

                return ToolResult.Success(
                    "Prefab inspected.",
                    new
                    {
                        prefab = new
                        {
                            name = prefab.name,
                            path = prefabPath,
                            guid = prefabGuid,
                            prefabAssetType = PrefabUtility.GetPrefabAssetType(prefab).ToString(),
                            rootChildCount = prefab.transform.childCount,
                            componentCount = prefab.GetComponents<UnityComponent>().Length,
                            missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab)
                        },
                        includeHierarchy,
                        includeInactive,
                        hierarchyCount = hierarchyRows.Count,
                        hierarchy = hierarchyRows
                    });
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect prefab.");
        }
    }
}
}
