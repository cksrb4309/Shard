using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Asset
{

[UnityCliTool(
    Name = UnityCliToolNames.ReserializeTargets,
    Description = "Force reserialize specific assets or folders.",
    Group = UnityCliToolGroups.Asset)]
public static class ReserializeTargetsTool
{
    public sealed class Parameters
    {
        [ToolParameter("Asset path or comma-separated list of asset paths.")]
        public string Path { get; set; }

        [ToolParameter("Folder path or comma-separated list of folder paths.")]
        public string Folder { get; set; }

        [ToolParameter("Preview only without reserializing.", DefaultValue = "true")]
        public bool DryRun { get; set; }

        [ToolParameter("Max number of paths to include in the response.", DefaultValue = "500")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var dryRun = p.GetBool("dryRun", true);
            var limit = Math.Max(1, Math.Min(5000, p.GetInt("limit") ?? 500));
            var paths = ResolvePaths(parameters);

            if (paths.Count == 0)
            {
                return ToolResult.Error("At least one path or folder is required.");
            }

            if (!dryRun)
            {
                AssetDatabase.ForceReserializeAssets(paths);
            }

            var preview = new List<string>(Math.Min(limit, paths.Count));
            for (var i = 0; i < paths.Count && preview.Count < limit; i++)
            {
                preview.Add(paths[i]);
            }

            return ToolResult.Success(
                dryRun ? "Reserialize preview prepared." : "Targets reserialized.",
                new
                {
                    dryRun,
                    totalTargets = paths.Count,
                    returned = preview.Count,
                    paths = preview
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to reserialize targets.");
        }
    }

    private static List<string> ResolvePaths(JObject parameters)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var explicitPaths = ParamUtility.ReadStringList(parameters, "path");
        var folders = ParamUtility.ReadStringList(parameters, "folder");

        foreach (var path in explicitPaths)
        {
            var candidate = UnityCliToolShared.TryConvertToAssetPath(path);
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (AssetDatabase.IsValidFolder(candidate))
            {
                AddFolderContents(result, candidate);
            }
            else
            {
                result.Add(candidate);
            }
        }

        foreach (var folder in folders)
        {
            var candidate = UnityCliToolShared.TryConvertToAssetPath(folder);
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (AssetDatabase.IsValidFolder(candidate))
            {
                AddFolderContents(result, candidate);
            }
        }

            var paths = new List<string>(result);
            paths.Sort(StringComparer.OrdinalIgnoreCase);
            return paths;
        }

    private static void AddFolderContents(HashSet<string> result, string folder)
    {
        var guids = AssetDatabase.FindAssets(string.Empty, new[] { folder });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrWhiteSpace(path))
            {
                result.Add(path);
            }
        }
    }
}
}
