using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;

namespace UnityCliTools.Hierarchy
{

[UnityCliTool(
    Name = UnityCliToolNames.FindGameObjects,
    Description = "Find GameObjects in loaded scenes by name, tag, layer, and active state.",
    Group = UnityCliToolGroups.Hierarchy)]
public static class FindGameObjectsTool
{
    public sealed class Parameters
    {
        [ToolParameter("Name keyword (case-insensitive).")]
        public string Name { get; set; }

        [ToolParameter("Exact Unity tag filter.")]
        public string Tag { get; set; }

        [ToolParameter("Layer index filter.")]
        public int Layer { get; set; }

        [ToolParameter("Only include objects active in hierarchy.", DefaultValue = "false")]
        public bool ActiveOnly { get; set; }

        [ToolParameter("Include inactive objects during traversal.", DefaultValue = "false")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Filter to one loaded scene path.")]
        public string ScenePath { get; set; }

        [ToolParameter("Max number of returned results.", DefaultValue = "200")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var name = p.Get("name", string.Empty)?.Trim();
            var tag = p.Get("tag", string.Empty)?.Trim();
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var includeInactive = p.GetBool("includeInactive", false);
            var activeOnly = p.GetBool("activeOnly", false);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 200, 1, 2000);
            var hasLayer = int.TryParse(p.Get("layer"), out var layer);

            var rows = new List<object>(limit);
            foreach (var scene in HierarchyUtility.EnumerateLoadedScenes())
            {
                if (rows.Count >= limit)
                {
                    break;
                }

                if (!string.IsNullOrWhiteSpace(scenePath)
                    && !string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var gameObject in HierarchyUtility.EnumerateSceneObjects(scene, includeInactive))
                {
                    if (rows.Count >= limit)
                    {
                        break;
                    }

                    if (activeOnly && !gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (hasLayer && gameObject.layer != layer)
                    {
                        continue;
                    }

                    var safeTag = HierarchyUtility.SafeTag(gameObject);
                    if (!string.IsNullOrWhiteSpace(tag)
                        && !string.Equals(safeTag, tag, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(name)
                        && gameObject.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    rows.Add(new
                    {
                        name = gameObject.name,
                        hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                        scenePath = scene.path,
                        instanceId = gameObject.GetInstanceID(),
                        activeSelf = gameObject.activeSelf,
                        activeInHierarchy = gameObject.activeInHierarchy,
                        tag = safeTag,
                        layer = gameObject.layer,
                        componentCount = gameObject.GetComponents<UnityEngine.Component>().Length
                    });
                }
            }

            return ToolResult.Success(
                "GameObjects searched.",
                new
                {
                    query = new
                    {
                        name = name ?? string.Empty,
                        tag = tag ?? string.Empty,
                        layer = hasLayer ? layer : (int?)null,
                        activeOnly,
                        includeInactive,
                        scenePath = scenePath ?? string.Empty,
                        limit
                    },
                    returned = rows.Count,
                    results = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to search GameObjects.");
        }
    }
}
}
