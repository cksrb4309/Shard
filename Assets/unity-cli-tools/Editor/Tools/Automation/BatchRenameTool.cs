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
    Name = UnityCliToolNames.BatchRename,
    Description = "Rename matching loaded scene GameObjects in bulk.",
    Group = UnityCliToolGroups.Automation)]
public static class BatchRenameTool
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

        [ToolParameter("Prefix to prepend.")]
        public string Prefix { get; set; }

        [ToolParameter("Suffix to append.")]
        public string Suffix { get; set; }

        [ToolParameter("Search text to replace within each name.")]
        public string Search { get; set; }

        [ToolParameter("Replacement text for Search.")]
        public string Replace { get; set; }

        [ToolParameter("Override the base name before prefix/suffix are applied.")]
        public string NewName { get; set; }

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
            var prefix = p.Get("prefix", string.Empty) ?? string.Empty;
            var suffix = p.Get("suffix", string.Empty) ?? string.Empty;
            var search = p.Get("search", string.Empty) ?? string.Empty;
            var replace = p.Get("replace", string.Empty) ?? string.Empty;
            var newName = p.Get("newName", string.Empty)?.Trim();
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
                var candidate = string.IsNullOrWhiteSpace(newName) ? gameObject.name : newName;
                if (!string.IsNullOrWhiteSpace(search))
                {
                    candidate = candidate.Replace(search, replace);
                }

                candidate = prefix + candidate + suffix;
                if (string.Equals(candidate, gameObject.name, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!dryRun)
                {
                    Undo.RecordObject(gameObject, "Batch Rename");
                    gameObject.name = candidate;
                    EditorUtility.SetDirty(gameObject);
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }

                results.Add(new
                {
                    name = gameObject.name,
                    newName = candidate,
                    hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                    scenePath = gameObject.scene.path,
                    instanceId = gameObject.GetInstanceID()
                });
            }

            return ToolResult.Success(
                dryRun ? "Batch rename previewed." : "Batch rename completed.",
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
                        prefix,
                        suffix,
                        search,
                        replace,
                        newName = newName ?? string.Empty,
                        limit
                    },
                    matched,
                    changed = results.Count,
                    results
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to batch rename objects.");
        }
    }
}
}
