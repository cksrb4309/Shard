using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;

namespace UnityCliTools.Component
{

[UnityCliTool(
    Name = UnityCliToolNames.FindObjectsWithComponent,
    Description = "Find loaded scene GameObjects that have a given component type.",
    Group = UnityCliToolGroups.Component)]
public static class FindObjectsWithComponentTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path. If omitted, scans all loaded scenes.")]
        public string ScenePath { get; set; }

        [ToolParameter("Component type name or full name.", Required = true)]
        public string ComponentType { get; set; }

        [ToolParameter("Include inactive objects in traversal.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Max number of returned results.", DefaultValue = "200")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var componentTypeName = p.Get("componentType", string.Empty)?.Trim();
            var includeInactive = p.GetBool("includeInactive", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 200, 1, 2000);

            UnityCliToolShared.GuardRequired(componentTypeName, nameof(componentTypeName));

            var rows = new List<object>(Math.Min(limit, 128));
            var matchedObjectCount = 0;
            var componentCount = 0;
            foreach (var gameObject in BatchTargetUtility.EnumerateMatchingObjects(scenePath, includeInactive, null, null, null))
            {
                if (rows.Count >= limit)
                {
                    break;
                }

                var components = gameObject.GetComponents<UnityEngine.Component>();
                var matches = new List<object>();
                for (var i = 0; i < components.Length; i++)
                {
                    if (rows.Count >= limit)
                    {
                        break;
                    }

                    var component = components[i];
                    if (component == null)
                    {
                        continue;
                    }

                    var type = component.GetType();
                    if (!string.Equals(type.Name, componentTypeName, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(type.FullName, componentTypeName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    matches.Add(new
                    {
                        type = type.FullName,
                        instanceId = component.GetInstanceID()
                    });
                    componentCount++;
                }

                if (matches.Count == 0)
                {
                    continue;
                }

                matchedObjectCount++;
                rows.Add(new
                {
                    name = gameObject.name,
                    hierarchyPath = HierarchyUtility.GetHierarchyPath(gameObject.transform),
                    scenePath = gameObject.scene.path,
                    instanceId = gameObject.GetInstanceID(),
                    componentMatches = matches.Count,
                    components = matches
                });
            }

            return ToolResult.Success(
                "Objects with component searched.",
                new
                {
                    query = new
                    {
                        scenePath = scenePath ?? string.Empty,
                        componentType = componentTypeName,
                        includeInactive,
                        limit
                    },
                    matchedObjectCount,
                    componentCount,
                    returned = rows.Count,
                    results = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to find objects with component.");
        }
    }
}
}
