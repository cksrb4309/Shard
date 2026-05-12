using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Build
{

[UnityCliTool(
    Name = UnityCliToolNames.BuildPreflight,
    Description = "Inspect build target, output, scenes, and common blockers without starting a build.",
    Group = UnityCliToolGroups.Build)]
public static class BuildPreflightTool
{
    public sealed class Parameters
    {
        [ToolParameter("Build target enum name. Defaults to the active build target.")]
        public string BuildTarget { get; set; }

        [ToolParameter("Build output path to validate. Optional.")]
        public string OutputPath { get; set; }

        [ToolParameter("Include disabled Build Settings scenes.", DefaultValue = "false")]
        public bool IncludeDisabledScenes { get; set; }

        [ToolParameter("Max number of scenes or issues returned.", DefaultValue = "200")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var targetText = p.Get("buildTarget", string.Empty)?.Trim();
            var target = EditorUserBuildSettings.activeBuildTarget;
            var targetParsed = string.IsNullOrWhiteSpace(targetText)
                || Enum.TryParse(targetText, true, out target);
            var group = BuildPipeline.GetBuildTargetGroup(target);
            var outputPath = p.Get("outputPath", string.Empty)?.Trim();
            var includeDisabled = p.GetBool("includeDisabledScenes", false);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 200, 1, 2000);

            var issues = new List<object>();
            if (!targetParsed)
            {
                issues.Add(new
                {
                    severity = "error",
                    code = "invalid_build_target",
                    message = "Build target could not be parsed.",
                    value = targetText ?? string.Empty
                });
            }

            if (targetParsed && !BuildPipeline.IsBuildTargetSupported(group, target))
            {
                issues.Add(new
                {
                    severity = "error",
                    code = "unsupported_build_target",
                    message = "Build target is not supported by this editor installation.",
                    buildTarget = target.ToString(),
                    buildTargetGroup = group.ToString()
                });
            }

            if (EditorApplication.isCompiling)
            {
                issues.Add(new { severity = "error", code = "editor_compiling", message = "Editor is compiling." });
            }

            if (EditorApplication.isUpdating)
            {
                issues.Add(new { severity = "warning", code = "asset_database_updating", message = "AssetDatabase is updating." });
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                issues.Add(new { severity = "warning", code = "play_mode_active", message = "Editor is in or entering play mode." });
            }

            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                var fullOutputPath = Path.GetFullPath(outputPath);
                var outputDirectory = Path.HasExtension(fullOutputPath)
                    ? Path.GetDirectoryName(fullOutputPath)
                    : fullOutputPath;
                if (string.IsNullOrWhiteSpace(outputDirectory) || !Directory.Exists(outputDirectory))
                {
                    issues.Add(new
                    {
                        severity = "warning",
                        code = "output_directory_missing",
                        message = "Build output directory does not exist.",
                        outputPath = fullOutputPath,
                        outputDirectory = outputDirectory ?? string.Empty
                    });
                }
            }

            var scenes = new List<object>();
            var enabledSceneCount = 0;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    enabledSceneCount++;
                }

                if (!includeDisabled && !scene.enabled)
                {
                    continue;
                }

                if (scenes.Count >= limit)
                {
                    continue;
                }

                var path = UnityCliToolShared.TryConvertToAssetPath(scene.path);
                var exists = !string.IsNullOrWhiteSpace(path) && AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null;
                if (scene.enabled && !exists && issues.Count < limit)
                {
                    issues.Add(new
                    {
                        severity = "error",
                        code = "missing_enabled_scene",
                        message = "Enabled Build Settings scene is missing.",
                        path
                    });
                }

                scenes.Add(new
                {
                    enabled = scene.enabled,
                    exists,
                    path,
                    guid = string.IsNullOrWhiteSpace(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path)
                });
            }

            if (enabledSceneCount == 0)
            {
                issues.Add(new { severity = "warning", code = "no_enabled_scenes", message = "No enabled scenes are configured in Build Settings." });
            }

            return ToolResult.Success(
                "Build preflight completed.",
                new
                {
                    buildTarget = target.ToString(),
                    buildTargetGroup = group.ToString(),
                    targetParsed,
                    outputPath = outputPath ?? string.Empty,
                    enabledSceneCount,
                    returnedScenes = scenes.Count,
                    scenes,
                    issueCount = issues.Count,
                    issues
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect build preflight state.");
        }
    }
}
}
