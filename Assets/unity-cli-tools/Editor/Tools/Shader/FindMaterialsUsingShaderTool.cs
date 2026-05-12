using System;
using System.Collections.Generic;
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
    Name = UnityCliToolNames.FindMaterialsUsingShader,
    Description = "Find materials that use a target shader.",
    Group = UnityCliToolGroups.Shader)]
public static class FindMaterialsUsingShaderTool
{
    public sealed class Parameters
    {
        [ToolParameter("Shader name.")]
        public string ShaderName { get; set; }

        [ToolParameter("Shader asset path.")]
        public string ShaderPath { get; set; }

        [ToolParameter("Shader asset GUID.")]
        public string ShaderGuid { get; set; }

        [ToolParameter("Folder filter as string or string array.")]
        public string Folder { get; set; }

        [ToolParameter("Max number of returned results.", DefaultValue = "200")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            if (!ShaderResolver.TryResolveShader(parameters, out var shader, out var shaderPath, out var shaderGuid, out var error))
            {
                return ToolResult.Error(error);
            }

            var p = new ToolParams(parameters);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 200, 1, 5000);
            var folders = ShaderResolver.NormalizeAssetFolders(ParamUtility.ReadStringList(parameters, "folder"));
            var guids = folders.Length > 0
                ? AssetDatabase.FindAssets("t:Material", folders)
                : AssetDatabase.FindAssets("t:Material");

            var rows = new List<object>(Math.Min(limit, guids.Length));
            for (var i = 0; i < guids.Length && rows.Count < limit; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null || material.shader == null)
                {
                    continue;
                }

                if (material.shader != shader)
                {
                    continue;
                }

                rows.Add(new
                {
                    name = material.name,
                    path,
                    guid,
                    shaderName = material.shader.name,
                    renderQueue = material.renderQueue,
                    enableInstancing = material.enableInstancing
                });
            }

            return ToolResult.Success(
                "Materials using shader searched.",
                new
                {
                    shader = new
                    {
                        name = shader.name,
                        path = shaderPath ?? string.Empty,
                        guid = shaderGuid ?? string.Empty,
                        isSupported = shader.isSupported,
                        renderPipeline = ShaderResolver.GuessRenderPipeline(shader.name, shaderPath)
                    },
                    query = new
                    {
                        folder = folders,
                        limit
                    },
                    totalScanned = guids.Length,
                    returned = rows.Count,
                    results = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to find materials by shader.");
        }
    }
}
}
