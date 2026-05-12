using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.InspectUiCanvases,
    Description = "Inspect Canvas objects and high-level UI hierarchy information.",
    Group = UnityCliToolGroups.Visual)]
public static class InspectUiCanvasesTool
{
    public sealed class Parameters
    {
        [ToolParameter("Include inactive canvases.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Include canvases not attached to loaded scenes, such as prefab assets.", DefaultValue = "false")]
        public bool IncludeAssets { get; set; }

        [ToolParameter("Max number of returned canvases.", DefaultValue = "100")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var includeInactive = p.GetBool("includeInactive", true);
            var includeAssets = p.GetBool("includeAssets", false);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 100, 1, 1000);
            var canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            var rows = new List<object>(Math.Min(limit, canvases.Length));

            foreach (var canvas in canvases)
            {
                if (rows.Count >= limit)
                {
                    break;
                }

                if (canvas == null || canvas.gameObject == null)
                {
                    continue;
                }

                var inLoadedScene = canvas.gameObject.scene.IsValid() && canvas.gameObject.scene.isLoaded;
                if (!includeAssets && !inLoadedScene)
                {
                    continue;
                }

                if (!includeInactive && !canvas.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var rect = canvas.transform as RectTransform;
                rows.Add(new
                {
                    name = canvas.name,
                    hierarchyPath = inLoadedScene ? HierarchyUtility.GetHierarchyPath(canvas.transform) : string.Empty,
                    scenePath = inLoadedScene ? canvas.gameObject.scene.path : string.Empty,
                    activeSelf = canvas.gameObject.activeSelf,
                    activeInHierarchy = canvas.gameObject.activeInHierarchy,
                    enabled = canvas.enabled,
                    renderMode = canvas.renderMode.ToString(),
                    sortingLayerName = canvas.sortingLayerName,
                    sortingOrder = canvas.sortingOrder,
                    pixelPerfect = canvas.pixelPerfect,
                    overrideSorting = canvas.overrideSorting,
                    childCount = canvas.transform.childCount,
                    rect = rect != null
                        ? new { width = rect.rect.width, height = rect.rect.height, pivotX = rect.pivot.x, pivotY = rect.pivot.y }
                        : null,
                    worldCamera = canvas.worldCamera != null ? canvas.worldCamera.name : string.Empty,
                    instanceId = canvas.GetInstanceID()
                });
            }

            return ToolResult.Success(
                "UI canvases inspected.",
                new
                {
                    includeInactive,
                    includeAssets,
                    totalFound = canvases.Length,
                    returned = rows.Count,
                    canvases = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect UI canvases.");
        }
    }
}
}
