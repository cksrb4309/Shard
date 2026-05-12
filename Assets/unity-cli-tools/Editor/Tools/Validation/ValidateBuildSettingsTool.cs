using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Validation
{

[UnityCliTool(
    Name = UnityCliToolNames.ValidateBuildSettings,
    Description = "Validate scenes listed in Build Settings.",
    Group = UnityCliToolGroups.Validation)]
public static class ValidateBuildSettingsTool
{
    public sealed class Parameters
    {
        [ToolParameter("Include disabled scenes in the response.", DefaultValue = "true")]
        public bool IncludeDisabled { get; set; }

        [ToolParameter("Max number of returned rows.", DefaultValue = "200")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var includeDisabled = p.GetBool("includeDisabled", true);
            var limit = Math.Max(1, Math.Min(2000, p.GetInt("limit") ?? 200));
            var scenes = EditorBuildSettings.scenes;
            var rows = new List<object>(Math.Min(limit, scenes.Length));
            var issues = new List<object>();
            var enabledSceneCount = 0;
            var missingSceneCount = 0;
            var duplicateSceneCount = 0;
            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < scenes.Length; i++)
            {
                var scene = scenes[i];
                if (!includeDisabled && !scene.enabled)
                {
                    continue;
                }

                var scenePath = UnityCliToolShared.TryConvertToAssetPath(scene.path);
                var sceneAsset = string.IsNullOrWhiteSpace(scenePath)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                var exists = sceneAsset != null;
                if (scene.enabled)
                {
                    enabledSceneCount++;
                }

                if (!seenPaths.Add(scenePath))
                {
                    duplicateSceneCount++;
                    if (issues.Count < limit)
                    {
                        issues.Add(new
                        {
                            severity = "warning",
                            code = "duplicate_scene_path",
                            index = i,
                            path = scenePath,
                            message = "Duplicate scene path in Build Settings."
                        });
                    }
                }

                if (!exists)
                {
                    missingSceneCount++;
                    if (issues.Count < limit)
                    {
                        issues.Add(new
                        {
                            severity = "error",
                            code = "missing_scene_asset",
                            index = i,
                            path = scenePath,
                            message = "Scene asset is missing or invalid."
                        });
                    }
                }

                if (rows.Count < limit)
                {
                    rows.Add(new
                    {
                        index = i,
                        enabled = scene.enabled,
                        path = scenePath,
                        name = sceneAsset != null ? sceneAsset.name : Path.GetFileNameWithoutExtension(scenePath),
                        exists,
                        guid = string.IsNullOrWhiteSpace(scenePath) ? string.Empty : AssetDatabase.AssetPathToGUID(scenePath)
                    });
                }
            }

            return ToolResult.Success(
                "Build settings validated.",
                new
                {
                    includeDisabled,
                    totalScenes = scenes.Length,
                    enabledSceneCount,
                    missingSceneCount,
                    duplicateSceneCount,
                    returned = rows.Count,
                    scenes = rows,
                    issues
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to validate build settings.");
        }
    }
}
}
