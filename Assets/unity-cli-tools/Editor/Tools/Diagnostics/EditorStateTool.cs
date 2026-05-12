using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Diagnostics
{

[UnityCliTool(
    Name = UnityCliToolNames.EditorState,
    Description = "Return current Unity Editor state for tool branching.",
    Group = UnityCliToolGroups.Diagnostics)]
public static class EditorStateTool
{
    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            return ToolResult.Success(
                "Editor state retrieved.",
                new
                {
                    isPlaying = EditorApplication.isPlaying,
                    isPaused = EditorApplication.isPaused,
                    isCompiling = EditorApplication.isCompiling,
                    isUpdating = EditorApplication.isUpdating,
                    projectPath = UnityCliToolShared.GetProjectPath(),
                    activeScenePath = activeScene.path ?? string.Empty,
                    activeSceneName = activeScene.name ?? string.Empty,
                    openSceneCount = SceneManager.sceneCount,
                    selectionCount = Selection.objects?.Length ?? 0
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to read editor state.");
        }
    }
}
}
