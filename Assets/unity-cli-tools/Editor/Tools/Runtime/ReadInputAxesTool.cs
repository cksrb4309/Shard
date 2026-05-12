using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Runtime
{

[UnityCliTool(
    Name = UnityCliToolNames.ReadInputAxes,
    Description = "Read legacy UnityEngine.Input axes and basic mouse/key state.",
    Group = UnityCliToolGroups.Runtime)]
public static class ReadInputAxesTool
{
    public sealed class Parameters
    {
        [ToolParameter("Axis names as string array or comma-separated list.", DefaultValue = "Horizontal,Vertical,Mouse X,Mouse Y")]
        public string Axes { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var axes = ParamUtility.ReadStringList(parameters, "axes");
            if (axes.Count == 0)
            {
                axes = new[] { "Horizontal", "Vertical", "Mouse X", "Mouse Y" };
            }

            var rows = new List<object>();
            foreach (var axis in axes)
            {
                try
                {
                    rows.Add(new
                    {
                        name = axis,
                        value = Input.GetAxis(axis),
                        rawValue = Input.GetAxisRaw(axis),
                        success = true,
                        error = string.Empty
                    });
                }
                catch (Exception ex)
                {
                    rows.Add(new
                    {
                        name = axis,
                        value = 0f,
                        rawValue = 0f,
                        success = false,
                        error = ex.Message
                    });
                }
            }

            return ToolResult.Success(
                "Input axes read.",
                new
                {
                    applicationIsPlaying = Application.isPlaying,
                    anyKey = SafeBool(() => Input.anyKey),
                    anyKeyDown = SafeBool(() => Input.anyKeyDown),
                    mousePosition = SafeVector3(() => Input.mousePosition),
                    axes = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to read input axes.");
        }
    }

    private static bool SafeBool(Func<bool> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return false;
        }
    }

    private static object SafeVector3(Func<Vector3> read)
    {
        try
        {
            var value = read();
            return new { x = value.x, y = value.y, z = value.z };
        }
        catch
        {
            return new { x = 0f, y = 0f, z = 0f };
        }
    }
}
}
