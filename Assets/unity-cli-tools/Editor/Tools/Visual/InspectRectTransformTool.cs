using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.InspectRectTransform,
    Description = "Inspect RectTransform anchors, pivot, offsets, size, and world corners.",
    Group = UnityCliToolGroups.Visual)]
public static class InspectRectTransformTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target UI GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = p.Get("scenePath", string.Empty);
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, hierarchyPath = hierarchyPath ?? string.Empty });
            }

            var rectTransform = target.transform as RectTransform;
            if (rectTransform == null)
            {
                return ToolResult.Error("Target does not have a RectTransform.", UiToolUtility.ElementSnapshot(target));
            }

            return ToolResult.Success(
                "RectTransform inspected.",
                new
                {
                    target = UiToolUtility.ElementSnapshot(target),
                    rectTransform = UiToolUtility.RectSnapshot(rectTransform)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect RectTransform.");
        }
    }
}
}
