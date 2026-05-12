using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Parameters;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.ShaderTools
{

[UnityCliTool(
    Name = UnityCliToolNames.FindShaders,
    Description = "Search shader assets by name and folder.",
    Group = UnityCliToolGroups.Shader)]
public static class FindShadersTool
{
    public sealed class Parameters
    {
        [ToolParameter("Shader name keyword.")]
        public string Name { get; set; }

        [ToolParameter("Folder filter as string or string array.")]
        public string Folder { get; set; }

        [ToolParameter("Include hidden shaders (Hidden/*).", DefaultValue = "false")]
        public bool IncludeHidden { get; set; }

        [ToolParameter("Include unsupported shaders.", DefaultValue = "true")]
        public bool IncludeUnsupported { get; set; }

        [ToolParameter("Max number of returned results.", DefaultValue = "100")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var name = p.Get("name", string.Empty)?.Trim();
            var includeHidden = p.GetBool("includeHidden", false);
            var includeUnsupported = p.GetBool("includeUnsupported", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 100, 1, 1000);
            var folders = ShaderResolver.NormalizeAssetFolders(ParamUtility.ReadStringList(parameters, "folder"));

            var filter = string.IsNullOrWhiteSpace(name)
                ? "t:Shader"
                : name + " t:Shader";

            var guids = folders.Length > 0
                ? AssetDatabase.FindAssets(filter, folders)
                : AssetDatabase.FindAssets(filter);

            var rows = new List<ShaderRow>(Math.Min(limit, guids.Length));
            for (var i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (shader == null)
                {
                    continue;
                }

                var isHidden = shader.name.StartsWith("Hidden/", StringComparison.OrdinalIgnoreCase);
                if (!includeHidden && isHidden)
                {
                    continue;
                }

                if (!includeUnsupported && !shader.isSupported)
                {
                    continue;
                }

                rows.Add(new ShaderRow
                {
                    Name = shader.name,
                    Path = path,
                    Guid = guid,
                    IsHidden = isHidden,
                    IsSupported = shader.isSupported,
                    RenderPipeline = ShaderResolver.GuessRenderPipeline(shader.name, path)
                });
            }

            var ordered = rows
                .OrderBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.Path, StringComparer.OrdinalIgnoreCase)
                .Take(limit)
                .Select(row => new
                {
                    name = row.Name,
                    path = row.Path,
                    guid = row.Guid,
                    isHidden = row.IsHidden,
                    isSupported = row.IsSupported,
                    renderPipeline = row.RenderPipeline
                })
                .ToArray();

            return ToolResult.Success(
                "Shaders searched.",
                new
                {
                    query = new
                    {
                        name = name ?? string.Empty,
                        folder = folders,
                        includeHidden,
                        includeUnsupported,
                        limit
                    },
                    totalFound = guids.Length,
                    returned = ordered.Length,
                    results = ordered
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to search shaders.");
        }
    }

    private sealed class ShaderRow
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Guid { get; set; }
        public bool IsHidden { get; set; }
        public bool IsSupported { get; set; }
        public string RenderPipeline { get; set; }
    }
}
}
