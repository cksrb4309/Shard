using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.InspectUiHierarchy,
    Description = "Inspect UI hierarchy under a Canvas or RectTransform.",
    Group = UnityCliToolGroups.Visual)]
public static class InspectUiHierarchyTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target UI root.")]
        public string ScenePath { get; set; }

        [ToolParameter("Canvas or UI root hierarchy path. If omitted, all loaded scene canvases are inspected.")]
        public string CanvasHierarchyPath { get; set; }

        [ToolParameter("Include inactive objects.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Max child depth from each root.", DefaultValue = "8")]
        public int MaxDepth { get; set; }

        [ToolParameter("Max number of returned elements.", DefaultValue = "500")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var rootPath = p.Get("canvasHierarchyPath", string.Empty)?.Trim();
            var includeInactive = p.GetBool("includeInactive", true);
            var maxDepth = ParamUtility.ClampOrDefault(p.GetInt("maxDepth"), 8, 0, 32);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 500, 1, 5000);

            var roots = ResolveRoots(scenePath, rootPath, includeInactive);
            var rows = new List<object>();
            foreach (var root in roots)
            {
                foreach (var go in UiToolUtility.EnumerateChildren(root, includeInactive, maxDepth))
                {
                    if (rows.Count >= limit)
                    {
                        break;
                    }

                    rows.Add(UiToolUtility.ElementSnapshot(go, root.transform));
                }
            }

            return ToolResult.Success(
                "UI hierarchy inspected.",
                new
                {
                    query = new { scenePath = scenePath ?? string.Empty, canvasHierarchyPath = rootPath ?? string.Empty, includeInactive, maxDepth, limit },
                    rootCount = roots.Count,
                    returned = rows.Count,
                    elements = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect UI hierarchy.");
        }
    }

    private static List<GameObject> ResolveRoots(string scenePath, string rootPath, bool includeInactive)
    {
        var roots = new List<GameObject>();
        if (!string.IsNullOrWhiteSpace(rootPath)
            && UiToolUtility.TryResolveGameObject(scenePath, rootPath, out var root, out _))
        {
            roots.Add(root);
            return roots;
        }

        foreach (var go in UiToolUtility.EnumerateLoadedSceneObjects(scenePath, includeInactive))
        {
            if (go.GetComponent<Canvas>() != null)
            {
                roots.Add(go);
            }
        }

        return roots;
    }
}
}
