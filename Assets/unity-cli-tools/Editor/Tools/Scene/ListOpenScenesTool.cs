using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Scene
{

[UnityCliTool(
    Name = UnityCliToolNames.ListOpenScenes,
    Description = "List loaded scenes and active scene metadata.",
    Group = UnityCliToolGroups.Scene)]
public static class ListOpenScenesTool
{
    public sealed class Parameters
    {
        [ToolParameter("Include root object count per scene.", DefaultValue = "false")]
        public bool IncludeRootCount { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var includeRootCount = p.GetBool("includeRootCount", false);
            var activeScene = EditorSceneManager.GetActiveScene();

            var scenes = new List<object>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                scenes.Add(new
                {
                    name = scene.name,
                    path = scene.path,
                    buildIndex = scene.buildIndex,
                    isLoaded = scene.isLoaded,
                    isDirty = scene.isDirty,
                    isActive = scene.path == activeScene.path,
                    rootCount = includeRootCount ? scene.rootCount : 0
                });
            }

            return ToolResult.Success(
                "Open scenes listed.",
                new
                {
                    activeScenePath = activeScene.path,
                    activeSceneName = activeScene.name,
                    sceneCount = scenes.Count,
                    includeRootCount,
                    scenes
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to list open scenes.");
        }
    }
}
}
