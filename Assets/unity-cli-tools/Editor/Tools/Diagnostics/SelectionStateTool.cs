using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Diagnostics
{

[UnityCliTool(
    Name = UnityCliToolNames.SelectionState,
    Description = "Return currently selected scene objects and assets.",
    Group = UnityCliToolGroups.Diagnostics)]
public static class SelectionStateTool
{
    public sealed class Parameters
    {
        [ToolParameter("Include component type names for scene object selections.", DefaultValue = "false")]
        public bool IncludeComponents { get; set; }

        [ToolParameter("Include label metadata for selected assets.", DefaultValue = "true")]
        public bool IncludeAssetInfo { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var includeComponents = p.GetBool("includeComponents", false);
            var includeAssetInfo = p.GetBool("includeAssetInfo", true);

            var selected = Selection.objects ?? Array.Empty<UnityEngine.Object>();
            var sceneObjects = BuildSelectedSceneObjectRows(selected, includeComponents);
            var assets = BuildSelectedAssetRows(selected, includeAssetInfo);

            return ToolResult.Success(
                "Selection state retrieved.",
                new
                {
                    totalSelectionCount = selected.Length,
                    sceneObjectCount = sceneObjects.Count,
                    assetCount = assets.Count,
                    sceneObjects,
                    assets
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to read selection state.");
        }
    }

    private static List<object> BuildSelectedSceneObjectRows(
        IEnumerable<UnityEngine.Object> selectedObjects,
        bool includeComponents)
    {
        var rows = new List<object>();
        var seen = new HashSet<int>();

        foreach (var selected in selectedObjects)
        {
            var gameObject = selected as GameObject ?? (selected as UnityComponent)?.gameObject;
            if (gameObject == null)
            {
                continue;
            }

            var instanceId = gameObject.GetInstanceID();
            if (!seen.Add(instanceId))
            {
                continue;
            }

            var componentNames = includeComponents
                ? gameObject.GetComponents<UnityComponent>()
                    .Where(component => component != null)
                    .Select(component => component.GetType().Name)
                    .ToArray()
                : Array.Empty<string>();

            rows.Add(new
            {
                name = gameObject.name,
                hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                scenePath = gameObject.scene.path ?? string.Empty,
                instanceId,
                activeSelf = gameObject.activeSelf,
                activeInHierarchy = gameObject.activeInHierarchy,
                tag = HierarchyUtility.SafeTag(gameObject),
                layer = gameObject.layer,
                components = componentNames
            });
        }

        return rows;
    }

    private static List<object> BuildSelectedAssetRows(
        IEnumerable<UnityEngine.Object> selectedObjects,
        bool includeAssetInfo)
    {
        var rows = new List<object>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var selected in selectedObjects)
        {
            if (selected == null)
            {
                continue;
            }

            var path = AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            if (!seenPaths.Add(path))
            {
                continue;
            }

            var labels = includeAssetInfo ? AssetDatabase.GetLabels(selected) : Array.Empty<string>();
            rows.Add(new
            {
                name = selected.name,
                path,
                guid = AssetDatabase.AssetPathToGUID(path),
                type = selected.GetType().Name,
                instanceId = selected.GetInstanceID(),
                labels
            });
        }

        return rows;
    }
}
}
