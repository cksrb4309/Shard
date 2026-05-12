using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;

namespace UnityCliTools.Infrastructure.Parameters
{

public static class PrefabResolver
{
    public static bool TryResolvePrefab(
        JObject parameters,
        out GameObject prefab,
        out string prefabPath,
        out string prefabGuid,
        out string error)
    {
        prefab = null;
        prefabPath = string.Empty;
        prefabGuid = string.Empty;
        error = string.Empty;

        var p = new ToolParams(parameters);
        var prefabName = p.Get("prefabName", string.Empty)?.Trim();
        var inputPath = UnityCliToolShared.TryConvertToAssetPath(p.Get("prefabPath", string.Empty));
        var inputGuid = p.Get("prefabGuid", string.Empty)?.Trim();

        if (!string.IsNullOrWhiteSpace(inputGuid))
        {
            var pathFromGuid = AssetDatabase.GUIDToAssetPath(inputGuid);
            if (string.IsNullOrWhiteSpace(pathFromGuid))
            {
                error = "prefabGuid not found.";
                return false;
            }

            inputPath = pathFromGuid;
        }

        if (!string.IsNullOrWhiteSpace(inputPath))
        {
            var prefabFromPath = AssetDatabase.LoadAssetAtPath<GameObject>(inputPath);
            if (prefabFromPath == null)
            {
                error = "prefabPath is not a valid prefab asset.";
                return false;
            }

            prefab = prefabFromPath;
            prefabPath = inputPath;
            prefabGuid = AssetDatabase.AssetPathToGUID(inputPath);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(prefabName))
        {
            var search = AssetDatabase.FindAssets(prefabName + " t:Prefab");
            GameObject firstPartial = null;
            for (var i = 0; i < search.Length; i++)
            {
                var candidatePath = AssetDatabase.GUIDToAssetPath(search[i]);
                if (string.IsNullOrWhiteSpace(candidatePath))
                {
                    continue;
                }

                var candidate = AssetDatabase.LoadAssetAtPath<GameObject>(candidatePath);
                if (candidate == null)
                {
                    continue;
                }

                if (string.Equals(candidate.name, prefabName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(candidate.name, prefabName, StringComparison.Ordinal))
                {
                    prefab = candidate;
                    prefabPath = candidatePath;
                    prefabGuid = search[i];
                    return true;
                }

                if (firstPartial == null)
                {
                    firstPartial = candidate;
                    prefabPath = candidatePath;
                    prefabGuid = search[i];
                }
            }

            if (firstPartial != null)
            {
                prefab = firstPartial;
                return true;
            }

            error = "prefabName not found.";
            return false;
        }

        error = "One of prefabName, prefabPath, or prefabGuid is required.";
        return false;
    }
}
}
