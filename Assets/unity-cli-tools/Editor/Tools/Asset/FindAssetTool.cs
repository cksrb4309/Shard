using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Asset
{

[UnityCliTool(
    Name = UnityCliToolNames.FindAsset,
    Description = "Search AssetDatabase by name, type, and folders.",
    Group = UnityCliToolGroups.Asset)]
public static class FindAssetTool
{
    public sealed class Parameters
    {
        [ToolParameter("Asset name keyword (partial match allowed).")]
        public string Name { get; set; }

        [ToolParameter("Unity type filter, e.g. Texture2D, Material, SceneAsset.")]
        public string Type { get; set; }

        [ToolParameter("Folder filter as string or string array.")]
        public string Folder { get; set; }

        [ToolParameter("Max number of returned results.", DefaultValue = "100")]
        public int Limit { get; set; }

        [ToolParameter("Include AssetDatabase labels.", DefaultValue = "false")]
        public bool IncludeLabels { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var name = p.Get("name", string.Empty)?.Trim();
            var type = p.Get("type", string.Empty)?.Trim();
            var includeLabels = p.GetBool("includeLabels", false);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 100, 1, 1000);
            var folders = ParamUtility.ReadStringList(parameters, "folder");

            var filterParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(name))
            {
                filterParts.Add(name);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                filterParts.Add("t:" + type);
            }

            var filter = string.Join(" ", filterParts);
            var folderArray = folders.Count > 0 ? NormalizeFolders(folders) : Array.Empty<string>();
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

                var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                rows.Add(new
                {
                    name = mainAsset != null ? mainAsset.name : Path.GetFileNameWithoutExtension(path),
                    path,
                    guid,
                    type = AssetDatabase.GetMainAssetTypeAtPath(path)?.Name ?? "Unknown",
                    labels = includeLabels && mainAsset != null ? AssetDatabase.GetLabels(mainAsset) : Array.Empty<string>()
                });
            }

            return ToolResult.Success(
                "Assets searched.",
                new
                {
                    query = new
                    {
                        name = name ?? string.Empty,
                        type = type ?? string.Empty,
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
            return ToolResult.FromException(ex, "Failed to search assets.");
        }
    }

    private static string[] NormalizeFolders(IReadOnlyList<string> folders)
    {
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
