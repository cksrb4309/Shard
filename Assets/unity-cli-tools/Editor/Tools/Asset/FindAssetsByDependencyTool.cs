using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Asset
{

[UnityCliTool(
    Name = UnityCliToolNames.FindAssetsByDependency,
    Description = "Find assets that depend on a target asset.",
    Group = UnityCliToolGroups.Asset)]
public static class FindAssetsByDependencyTool
{
    public sealed class Parameters
    {
        [ToolParameter("Target asset name.")]
        public string Name { get; set; }

        [ToolParameter("Target asset path.")]
        public string Path { get; set; }

        [ToolParameter("Target asset GUID.")]
        public string Guid { get; set; }

        [ToolParameter("Folder filter as string or string array.")]
        public string Folder { get; set; }

        [ToolParameter("Max number of returned results.", DefaultValue = "100")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            if (!TryResolveTarget(parameters, out var targetPath, out var targetGuid, out var error))
            {
                return ToolResult.Error(error);
            }

            var p = new ToolParams(parameters);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 100, 1, 1000);
            var folders = ParamUtility.ReadStringList(parameters, "folder");
            var candidateFolders = NormalizeFolders(folders);
            var candidateGuids = candidateFolders.Length > 0
                ? AssetDatabase.FindAssets(string.Empty, candidateFolders)
                : AssetDatabase.FindAssets(string.Empty);

            var rows = new List<object>(Math.Min(limit, candidateGuids.Length));
            for (var i = 0; i < candidateGuids.Length && rows.Count < limit; i++)
            {
                var guid = candidateGuids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                if (string.Equals(path, targetPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var dependencies = AssetDatabase.GetDependencies(path, true);
                if (!dependencies.Any(dep => string.Equals(dep, targetPath, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                rows.Add(new
                {
                    name = mainAsset != null ? mainAsset.name : System.IO.Path.GetFileNameWithoutExtension(path),
                    path,
                    guid,
                    dependencyCount = dependencies.Length,
                    matchesDependency = true
                });
            }

            return ToolResult.Success(
                "Asset dependency search completed.",
                new
                {
                    target = new
                    {
                        path = targetPath,
                        guid = targetGuid
                    },
                    totalFound = candidateGuids.Length,
                    returned = rows.Count,
                    results = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to find assets by dependency.");
        }
    }

    private static bool TryResolveTarget(JObject parameters, out string targetPath, out string targetGuid, out string error)
    {
        targetPath = string.Empty;
        targetGuid = string.Empty;
        error = string.Empty;

        var p = new ToolParams(parameters);
        var name = p.Get("name", string.Empty)?.Trim();
        var inputPath = UnityCliToolShared.TryConvertToAssetPath(p.Get("path", string.Empty));
        var inputGuid = p.Get("guid", string.Empty)?.Trim();

        if (!string.IsNullOrWhiteSpace(inputGuid))
        {
            var resolvedPath = AssetDatabase.GUIDToAssetPath(inputGuid);
            if (string.IsNullOrWhiteSpace(resolvedPath))
            {
                error = "guid not found.";
                return false;
            }

            inputPath = resolvedPath;
        }

        if (!string.IsNullOrWhiteSpace(inputPath))
        {
            if (string.IsNullOrWhiteSpace(AssetDatabase.AssetPathToGUID(inputPath)))
            {
                error = "path does not point to a valid asset.";
                return false;
            }

            targetPath = inputPath;
            targetGuid = AssetDatabase.AssetPathToGUID(inputPath);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            var search = AssetDatabase.FindAssets(name);
            for (var i = 0; i < search.Length; i++)
            {
                var candidatePath = AssetDatabase.GUIDToAssetPath(search[i]);
                if (string.IsNullOrWhiteSpace(candidatePath))
                {
                    continue;
                }

                var mainAsset = AssetDatabase.LoadMainAssetAtPath(candidatePath);
                if (mainAsset == null)
                {
                    continue;
                }

                if (string.Equals(mainAsset.name, name, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(mainAsset.name, name, StringComparison.Ordinal))
                {
                    targetPath = candidatePath;
                    targetGuid = search[i];
                    return true;
                }
            }

            error = "name not found.";
            return false;
        }

        error = "One of name, path, or guid is required.";
        return false;
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
            if (AssetDatabase.IsValidFolder(candidate))
            {
                normalized.Add(candidate);
            }
        }

        return normalized.ToArray();
    }
}
}
