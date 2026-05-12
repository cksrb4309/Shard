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
    Name = UnityCliToolNames.ListCameras,
    Description = "List cameras in loaded scenes and optionally camera assets.",
    Group = UnityCliToolGroups.Visual)]
public static class ListCamerasTool
{
    public sealed class Parameters
    {
        [ToolParameter("Include inactive cameras.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Include cameras not attached to loaded scenes, such as prefab assets.", DefaultValue = "false")]
        public bool IncludeAssets { get; set; }

        [ToolParameter("Max number of returned cameras.", DefaultValue = "200")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var includeInactive = p.GetBool("includeInactive", true);
            var includeAssets = p.GetBool("includeAssets", false);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 200, 1, 2000);
            var cameras = Resources.FindObjectsOfTypeAll<Camera>();
            var rows = new List<object>(Math.Min(limit, cameras.Length));

            foreach (var camera in cameras)
            {
                if (rows.Count >= limit)
                {
                    break;
                }

                if (camera == null || camera.gameObject == null)
                {
                    continue;
                }

                var inLoadedScene = camera.gameObject.scene.IsValid() && camera.gameObject.scene.isLoaded;
                if (!includeAssets && !inLoadedScene)
                {
                    continue;
                }

                if (!includeInactive && !camera.gameObject.activeInHierarchy)
                {
                    continue;
                }

                rows.Add(new
                {
                    name = camera.name,
                    hierarchyPath = inLoadedScene ? HierarchyUtility.GetHierarchyPath(camera.transform) : string.Empty,
                    scenePath = inLoadedScene ? camera.gameObject.scene.path : string.Empty,
                    activeSelf = camera.gameObject.activeSelf,
                    activeInHierarchy = camera.gameObject.activeInHierarchy,
                    enabled = camera.enabled,
                    tag = camera.tag,
                    depth = camera.depth,
                    clearFlags = camera.clearFlags.ToString(),
                    fieldOfView = camera.fieldOfView,
                    orthographic = camera.orthographic,
                    orthographicSize = camera.orthographicSize,
                    nearClipPlane = camera.nearClipPlane,
                    farClipPlane = camera.farClipPlane,
                    targetTexture = camera.targetTexture != null ? camera.targetTexture.name : string.Empty,
                    instanceId = camera.GetInstanceID()
                });
            }

            return ToolResult.Success(
                "Cameras listed.",
                new
                {
                    includeInactive,
                    includeAssets,
                    totalFound = cameras.Length,
                    returned = rows.Count,
                    cameras = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to list cameras.");
        }
    }
}
}
