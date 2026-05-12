using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Parameters;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.ShaderTools
{

[UnityCliTool(
    Name = UnityCliToolNames.InspectShaderProperties,
    Description = "Inspect shader property schema and defaults.",
    Group = UnityCliToolGroups.Shader)]
public static class InspectShaderPropertiesTool
{
    public sealed class Parameters
    {
        [ToolParameter("Shader name.")]
        public string ShaderName { get; set; }

        [ToolParameter("Shader asset path.")]
        public string ShaderPath { get; set; }

        [ToolParameter("Shader asset GUID.")]
        public string ShaderGuid { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            if (!ShaderResolver.TryResolveShader(parameters, out var shader, out var shaderPath, out var shaderGuid, out var error))
            {
                return ToolResult.Error(error);
            }

            var propertyCount = shader.GetPropertyCount();
            var rows = new List<object>(propertyCount);
            for (var i = 0; i < propertyCount; i++)
            {
                var propertyType = shader.GetPropertyType(i);
                var row = new
                {
                    index = i,
                    name = shader.GetPropertyName(i),
                    description = shader.GetPropertyDescription(i),
                    type = propertyType.ToString(),
                    flags = shader.GetPropertyFlags(i).ToString(),
                    attributes = shader.GetPropertyAttributes(i),
                    @default = ReadDefaultValue(shader, i, propertyType)
                };
                rows.Add(row);
            }

            return ToolResult.Success(
                "Shader properties inspected.",
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
                    propertyCount,
                    properties = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect shader properties.");
        }
    }

    private static object ReadDefaultValue(Shader shader, int index, ShaderPropertyType type)
    {
        switch (type)
        {
            case ShaderPropertyType.Color:
            {
                var value = shader.GetPropertyDefaultVectorValue(index);
                return new
                {
                    r = value.x,
                    g = value.y,
                    b = value.z,
                    a = value.w
                };
            }
            case ShaderPropertyType.Vector:
            {
                var value = shader.GetPropertyDefaultVectorValue(index);
                return new
                {
                    x = value.x,
                    y = value.y,
                    z = value.z,
                    w = value.w
                };
            }
            case ShaderPropertyType.Float:
            {
                return shader.GetPropertyDefaultFloatValue(index);
            }
            case ShaderPropertyType.Range:
            {
                var range = shader.GetPropertyRangeLimits(index);
                return new
                {
                    defaultValue = shader.GetPropertyDefaultFloatValue(index),
                    min = range.x,
                    max = range.y
                };
            }
            case ShaderPropertyType.Texture:
            {
                return new
                {
                    defaultName = shader.GetPropertyTextureDefaultName(index),
                    dimension = shader.GetPropertyTextureDimension(index).ToString()
                };
            }
            default:
                return null;
        }
    }
}
}
