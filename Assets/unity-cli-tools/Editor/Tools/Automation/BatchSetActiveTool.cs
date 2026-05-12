using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;

namespace UnityCliTools.Automation
{

[UnityCliTool(
    Name = UnityCliToolNames.BatchSetActive,
    Description = "Set active state in bulk for matching loaded scene GameObjects.",
    Group = UnityCliToolGroups.Automation)]
public static class BatchSetActiveTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path. If omitted, scans all loaded scenes.")]
        public string ScenePath { get; set; }

        [ToolParameter("Include inactive objects in traversal.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Only include names containing this text.")]
        public string NameContains { get; set; }

        [ToolParameter("Exact Unity tag filter.")]
        public string Tag { get; set; }

        [ToolParameter("Layer index filter.")]
        public string Layer { get; set; }

        [ToolParameter("Desired active state.", DefaultValue = "true")]
        public bool Active { get; set; }

        [ToolParameter("Preview only without modifying the objects.", DefaultValue = "true")]
        public bool DryRun { get; set; }

        [ToolParameter("Max number of affected objects to return.", DefaultValue = "500")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var includeInactive = p.GetBool("includeInactive", true);
            var nameContains = p.Get("nameContains", string.Empty)?.Trim();
            var tag = p.Get("tag", string.Empty)?.Trim();
            var layerText = p.Get("layer", string.Empty)?.Trim();
            int? layer = int.TryParse(layerText, out var parsedLayer) ? parsedLayer : (int?)null;
            var active = p.GetBool("active", true);
            var dryRun = p.GetBool("dryRun", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 500, 1, 5000);

            var results = new List<object>(Math.Min(limit, 128));
            var matched = 0;
            foreach (var gameObject in BatchTargetUtility.EnumerateMatchingObjects(scenePath, includeInactive, nameContains, tag, layer))
            {
                if (results.Count >= limit)
                {
                    break;
                }

                matched++;
                if (gameObject.activeSelf == active)
                {
                    continue;
                }

                if (!dryRun)
                {
                    Undo.RecordObject(gameObject, "Batch Set Active");
                    gameObject.SetActive(active);
                    EditorUtility.SetDirty(gameObject);
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }

                results.Add(new
                {
                    name = gameObject.name,
                    hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                    scenePath = gameObject.scene.path,
                    instanceId = gameObject.GetInstanceID(),
                    before = gameObject.activeSelf,
                    after = active
                });
            }

            return ToolResult.Success(
                dryRun ? "Batch active change previewed." : "Batch active change completed.",
                new
                {
                    dryRun,
                    query = new
                    {
                        scenePath = scenePath ?? string.Empty,
                        includeInactive,
                        nameContains = nameContains ?? string.Empty,
                        tag = tag ?? string.Empty,
                        layer = layer,
                        active,
                        limit
                    },
                    matched,
                    changed = results.Count,
                    results
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to batch set active.");
        }
    }
}
}
