using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditorInternal;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Project
{

[UnityCliTool(
    Name = UnityCliToolNames.InspectTagsLayers,
    Description = "Inspect project tags, sorting layers, and named layers.",
    Group = UnityCliToolGroups.Project)]
public static class InspectTagsLayersTool
{
    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var layers = new List<object>();
            var layerNames = InternalEditorUtility.layers;
            for (var i = 0; i < 32; i++)
            {
                var name = UnityEngine.LayerMask.LayerToName(i);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    layers.Add(new { index = i, name });
                }
            }

            return ToolResult.Success(
                "Tags and layers inspected.",
                new
                {
                    tags = InternalEditorUtility.tags,
                    layers,
                    namedLayerCount = layers.Count,
                    editorLayerNames = layerNames,
                    sortingLayers = UnityEngine.SortingLayer.layers
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect tags and layers.");
        }
    }
}
}
