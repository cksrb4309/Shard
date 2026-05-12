using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Scene
{

[UnityCliTool(
    Name = UnityCliToolNames.OpenScene,
    Description = "Open a scene by asset path.",
    Group = UnityCliToolGroups.Scene)]
public static class OpenSceneTool
{
    public sealed class Parameters
    {
        [ToolParameter("Scene asset path.", Required = true)]
        public string Path { get; set; }

        [ToolParameter("Open mode: single or additive.", DefaultValue = "single")]
        public string Mode { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var path = UnityCliToolShared.TryConvertToAssetPath(p.Get("path", string.Empty));
            UnityCliToolShared.GuardRequired(path, nameof(path));

            if (AssetDatabase.GetMainAssetTypeAtPath(path) != typeof(SceneAsset))
            {
                return ToolResult.Error(
                    "Target path is not a scene asset.",
                    new
                    {
                        path
                    });
            }

            var modeText = p.Get("mode", "single");
            var mode = ParseMode(modeText);
            var scene = EditorSceneManager.OpenScene(path, mode);
            var active = EditorSceneManager.GetActiveScene();

            return ToolResult.Success(
                "Scene opened.",
                new
                {
                    openedScenePath = scene.path,
                    openedSceneName = scene.name,
                    mode = mode == OpenSceneMode.Additive ? "additive" : "single",
                    activeScenePath = active.path,
                    openSceneCount = SceneManager.sceneCount
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to open scene.");
        }
    }

    private static OpenSceneMode ParseMode(string modeText)
    {
        if (string.Equals(modeText, "additive", StringComparison.OrdinalIgnoreCase))
        {
            return OpenSceneMode.Additive;
        }

        return OpenSceneMode.Single;
    }
}
}
