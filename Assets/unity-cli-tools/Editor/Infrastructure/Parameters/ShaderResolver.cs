using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;

namespace UnityCliTools.Infrastructure.Parameters
{

public static class ShaderResolver
{
    public static bool TryResolveShader(
        JObject parameters,
        out Shader shader,
        out string shaderPath,
        out string shaderGuid,
        out string error)
    {
        shader = null;
        shaderPath = string.Empty;
        shaderGuid = string.Empty;
        error = string.Empty;

        var p = new ToolParams(parameters);
        var shaderName = p.Get("shaderName", string.Empty)?.Trim();
        var inputPath = UnityCliToolShared.TryConvertToAssetPath(p.Get("shaderPath", string.Empty));
        var inputGuid = p.Get("shaderGuid", string.Empty)?.Trim();

        if (!string.IsNullOrWhiteSpace(inputGuid))
        {
            var pathFromGuid = AssetDatabase.GUIDToAssetPath(inputGuid);
            if (string.IsNullOrWhiteSpace(pathFromGuid))
            {
                error = "shaderGuid not found.";
                return false;
            }

            inputPath = pathFromGuid;
        }

        if (!string.IsNullOrWhiteSpace(inputPath))
        {
            var shaderFromPath = AssetDatabase.LoadAssetAtPath<Shader>(inputPath);
            if (shaderFromPath == null)
            {
                error = "shaderPath is not a valid shader asset.";
                return false;
            }

            shader = shaderFromPath;
            shaderPath = inputPath;
            shaderGuid = AssetDatabase.AssetPathToGUID(inputPath);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(shaderName))
        {
            var direct = Shader.Find(shaderName);
            if (direct != null)
            {
                shader = direct;
                shaderPath = AssetDatabase.GetAssetPath(direct);
                shaderGuid = string.IsNullOrWhiteSpace(shaderPath)
                    ? string.Empty
                    : AssetDatabase.AssetPathToGUID(shaderPath);
                return true;
            }

            var search = AssetDatabase.FindAssets(shaderName + " t:Shader");
            Shader firstPartial = null;
            for (var i = 0; i < search.Length; i++)
            {
                var candidatePath = AssetDatabase.GUIDToAssetPath(search[i]);
                if (string.IsNullOrWhiteSpace(candidatePath))
                {
                    continue;
                }

                var candidate = AssetDatabase.LoadAssetAtPath<Shader>(candidatePath);
                if (candidate == null)
                {
                    continue;
                }

                if (string.Equals(candidate.name, shaderName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(candidate.name, shaderName, StringComparison.Ordinal))
                {
                    shader = candidate;
                    shaderPath = candidatePath;
                    shaderGuid = search[i];
                    return true;
                }

                if (firstPartial == null)
                {
                    firstPartial = candidate;
                    shaderPath = candidatePath;
                    shaderGuid = search[i];
                }
            }

            if (firstPartial != null)
            {
                shader = firstPartial;
                return true;
            }

            error = "shaderName not found.";
            return false;
        }

        error = "One of shaderName, shaderPath, or shaderGuid is required.";
        return false;
    }

    public static string GuessRenderPipeline(string shaderName, string shaderPath)
    {
        var combined = ((shaderName ?? string.Empty) + " " + (shaderPath ?? string.Empty)).ToLowerInvariant();

        if ((shaderName ?? string.Empty).StartsWith("Hidden/", StringComparison.OrdinalIgnoreCase))
        {
            return "hidden";
        }

        if (combined.Contains("render-pipelines.universal")
            || combined.Contains("universal render pipeline")
            || combined.Contains(" urp")
            || (shaderName ?? string.Empty).StartsWith("Universal Render Pipeline/", StringComparison.OrdinalIgnoreCase))
        {
            return "urp";
        }

        if (combined.Contains("render-pipelines.high-definition")
            || combined.Contains("high definition render pipeline")
            || combined.Contains(" hdrp")
            || (shaderName ?? string.Empty).StartsWith("HDRP/", StringComparison.OrdinalIgnoreCase))
        {
            return "hdrp";
        }

        return "builtin-or-custom";
    }

    public static string[] NormalizeAssetFolders(IReadOnlyList<string> folders)
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
