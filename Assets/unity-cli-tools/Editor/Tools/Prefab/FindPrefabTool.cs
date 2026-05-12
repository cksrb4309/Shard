using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Prefab
{

[UnityCliTool(
    Name = UnityCliToolNames.FindPrefab,
    Description = "Search prefab assets by name and folder.",
    Group = UnityCliToolGroups.Prefab)]
public static class FindPrefabTool
{
    public sealed class Parameters
    {
        [ToolParameter("Prefab name keyword (partial match allowed).")]
        public string Name { get; set; }

        [ToolParameter("Folder filter as string or string array.")]
        public string Folder { get; set; }

        [ToolParameter("Max number of returned results.", DefaultValue = "100")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var name = p.Get("name", string.Empty)?.Trim();
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 100, 1, 1000);
            var folders = ParamUtility.ReadStringList(parameters, "folder");

            var filterParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(name))
            {
                filterParts.Add(name);
            }

            filterParts.Add("t:Prefab");
            var filter = string.Join(" ", filterParts);
            var folderArray = NormalizeFolders(folders);
            var guids = folderArray.Length > 0
                ? AssetDatabase.FindAssets(filter, folderArray)
                : AssetDatabase.FindAssets(filter);

            var rows = new List<object>(Math.Min(limit, guids.Length));
            for (var i = 0; i < guids.Length && rows.Count < limit; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                rows.Add(new
                {
                    name = prefab.name,
                    path,
                    guid,
                    prefabAssetType = PrefabUtility.GetPrefabAssetType(prefab).ToString(),
                    childCount = prefab.transform.childCount,
                    componentCount = prefab.GetComponents<UnityComponent>().Length,
                    missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab)
                });
            }

            return ToolResult.Success(
                "Prefabs searched.",
                new
                {
                    query = new
                    {
                        name = name ?? string.Empty,
                        folder = folderArray,
                        limit
                    },
                    totalFound = guids.Length,
                    returned = rows.Count,
                    results = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to search prefabs.");
        }
    }

    private static string[] NormalizeFolders(IReadOnlyList<string> folders)
    {
        if (folders == null || folders.Count == 0)
        {
            return Array.Empty<string>();
        }

        var normalized = new List<string>(folders.Count);
        foreach (var folder in folders)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                continue;
            }

            var candidate = UnityCliToolShared.TryConvertToAssetPath(folder);
            if (!candidate.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(candidate, "Assets", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            normalized.Add(candidate);
        }

        return normalized.ToArray();
    }
}
}
