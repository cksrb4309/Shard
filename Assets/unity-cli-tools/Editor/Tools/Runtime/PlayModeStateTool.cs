using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Runtime
{

[UnityCliTool(
    Name = UnityCliToolNames.PlayModeState,
    Description = "Inspect PlayMode, time, focus, and runtime frame state.",
    Group = UnityCliToolGroups.Runtime)]
public static class PlayModeStateTool
{
    public static object HandleCommand(JObject parameters)
    {
        try
        {
            return ToolResult.Success(
                "Play mode state inspected.",
                new
                {
                    isPlaying = EditorApplication.isPlaying,
                    isPaused = EditorApplication.isPaused,
                    isPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode,
                    isCompiling = EditorApplication.isCompiling,
                    isUpdating = EditorApplication.isUpdating,
                    applicationIsPlaying = Application.isPlaying,
                    isFocused = Application.isFocused,
                    runInBackground = Application.runInBackground,
                    timeScale = Time.timeScale,
                    time = Time.time,
                    unscaledTime = Time.unscaledTime,
                    deltaTime = Time.deltaTime,
                    frameCount = Time.frameCount,
                    fixedDeltaTime = Time.fixedDeltaTime,
                    targetFrameRate = Application.targetFrameRate,
                    platform = Application.platform.ToString()
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect play mode state.");
        }
    }
}
}
