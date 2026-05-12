using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Asset
{

[UnityCliTool(
    Name = UnityCliToolNames.GetAssetInfo,
    Description = "Inspect metadata for a single asset.",
    Group = UnityCliToolGroups.Asset)]
public static class GetAssetInfoTool
{
    public sealed class Parameters
    {
        [ToolParameter("Asset name to resolve. If multiple assets match, the first exact match is preferred.")]
        public string Name { get; set; }

        [ToolParameter("Asset path.")]
        public string Path { get; set; }

        [ToolParameter("Asset GUID.")]
        public string Guid { get; set; }

        [ToolParameter("Include direct dependencies in the response.", DefaultValue = "false")]
        public bool IncludeDependencies { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            if (!TryResolveAsset(parameters, out var asset, out var assetPath, out var assetGuid, out var error))
            {
                return ToolResult.Error(error);
            }

            var importer = AssetImporter.GetAtPath(assetPath);
            var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            var labels = mainAsset != null ? AssetDatabase.GetLabels(mainAsset) : Array.Empty<string>();
            var includeDependencies = new ToolParams(parameters).GetBool("includeDependencies", false);
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            var subAssetRows = new List<object>();
            for (var i = 0; i < subAssets.Length; i++)
            {
                var subAsset = subAssets[i];
                if (subAsset == null || subAsset == mainAsset)
                {
                    continue;
                }

                subAssetRows.Add(new
                {
                    name = subAsset.name,
                    type = subAsset.GetType().FullName,
                    instanceId = subAsset.GetInstanceID()
                });
            }

            return ToolResult.Success(
                "Asset info retrieved.",
                new
                {
                    asset = new
                    {
                        name = asset.name,
                        type = asset.GetType().FullName,
                        path = assetPath,
                        guid = assetGuid,
                        mainAssetType = mainAssetType?.FullName ?? "Unknown",
                        importerType = importer?.GetType().FullName ?? string.Empty,
                        isFolder = AssetDatabase.IsValidFolder(assetPath),
                        labels,
                        assetBundleName = importer?.assetBundleName ?? string.Empty,
                        assetBundleVariant = importer?.assetBundleVariant ?? string.Empty,
                        subAssetCount = subAssets.Length,
                        subAssets = subAssetRows
                    },
                    dependencies = includeDependencies
                        ? AssetDatabase.GetDependencies(assetPath, false)
                        : Array.Empty<string>()
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect asset.");
        }
    }

    private static bool TryResolveAsset(
        JObject parameters,
        out UnityEngine.Object asset,
        out string assetPath,
        out string assetGuid,
        out string error)
    {
        asset = null;
        assetPath = string.Empty;
        assetGuid = string.Empty;
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
            var loadedAsset = AssetDatabase.LoadMainAssetAtPath(inputPath);
            if (loadedAsset == null)
            {
                error = "path does not point to a valid asset.";
                return false;
            }

            asset = loadedAsset;
            assetPath = inputPath;
            assetGuid = AssetDatabase.AssetPathToGUID(inputPath);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            var search = AssetDatabase.FindAssets(name);
            UnityEngine.Object firstPartial = null;
            string firstPartialPath = string.Empty;
            string firstPartialGuid = string.Empty;

            for (var i = 0; i < search.Length; i++)
            {
                var candidatePath = AssetDatabase.GUIDToAssetPath(search[i]);
                if (string.IsNullOrWhiteSpace(candidatePath))
                {
                    continue;
                }

                var candidate = AssetDatabase.LoadMainAssetAtPath(candidatePath);
                if (candidate == null)
                {
                    continue;
                }

                if (string.Equals(candidate.name, name, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(candidate.name, name, StringComparison.Ordinal))
                {
                    asset = candidate;
                    assetPath = candidatePath;
                    assetGuid = search[i];
                    return true;
                }

                if (firstPartial == null)
                {
                    firstPartial = candidate;
                    firstPartialPath = candidatePath;
                    firstPartialGuid = search[i];
                }
            }

            if (firstPartial != null)
            {
                asset = firstPartial;
                assetPath = firstPartialPath;
                assetGuid = firstPartialGuid;
                return true;
            }

            error = "name not found.";
            return false;
        }

        error = "One of name, path, or guid is required.";
        return false;
    }
}
}
