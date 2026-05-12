using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.InspectCanvasScaler,
    Description = "Inspect CanvasScaler configuration on one or more canvases.",
    Group = UnityCliToolGroups.Visual)]
public static class InspectCanvasScalerTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target canvas.")]
        public string ScenePath { get; set; }

        [ToolParameter("Canvas hierarchy path. If omitted, all loaded scene canvases are inspected.")]
        public string HierarchyPath { get; set; }

        [ToolParameter("Include inactive canvases.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Max number of returned canvases.", DefaultValue = "100")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var includeInactive = p.GetBool("includeInactive", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 100, 1, 1000);
            var rows = new List<object>();

            if (!string.IsNullOrWhiteSpace(hierarchyPath))
            {
                if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var target, out var error))
                {
                    return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, hierarchyPath });
                }

                rows.Add(Snapshot(target));
            }
            else
            {
                foreach (var go in UiToolUtility.EnumerateLoadedSceneObjects(scenePath, includeInactive))
                {
                    if (rows.Count >= limit)
                    {
                        break;
                    }

                    if (go.GetComponent<Canvas>() != null)
                    {
                        rows.Add(Snapshot(go));
                    }
                }
            }

            return ToolResult.Success(
                "CanvasScaler inspected.",
                new
                {
                    query = new { scenePath = scenePath ?? string.Empty, hierarchyPath = hierarchyPath ?? string.Empty, includeInactive, limit },
                    returned = rows.Count,
                    canvases = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect CanvasScaler.");
        }
    }

    private static object Snapshot(GameObject go)
    {
        var scaler = go.GetComponent<CanvasScaler>();
        return new
        {
            target = UiToolUtility.ElementSnapshot(go),
            hasCanvasScaler = scaler != null,
            scaler = scaler != null
                ? new
                {
                    uiScaleMode = scaler.uiScaleMode.ToString(),
                    referenceResolution = UiToolUtility.Vector2Snapshot(scaler.referenceResolution),
                    screenMatchMode = scaler.screenMatchMode.ToString(),
                    matchWidthOrHeight = scaler.matchWidthOrHeight,
                    scaleFactor = scaler.scaleFactor,
                    referencePixelsPerUnit = scaler.referencePixelsPerUnit,
                    physicalUnit = scaler.physicalUnit.ToString(),
                    fallbackScreenDPI = scaler.fallbackScreenDPI,
                    defaultSpriteDPI = scaler.defaultSpriteDPI,
                    dynamicPixelsPerUnit = scaler.dynamicPixelsPerUnit
                }
                : null
        };
    }
}
}
