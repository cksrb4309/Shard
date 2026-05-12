using System;
using Newtonsoft.Json.Linq;
using UnityEditor.Build;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Project
{

[UnityCliTool(
    Name = UnityCliToolNames.InspectProjectSettings,
    Description = "Inspect common PlayerSettings, editor version, and active build target.",
    Group = UnityCliToolGroups.Project)]
public static class InspectProjectSettingsTool
{
    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(group);
            return ToolResult.Success(
                "Project settings inspected.",
                new
                {
                    projectPath = UnityCliToolShared.GetProjectPath(),
                    unityVersion = Application.unityVersion,
                    productName = PlayerSettings.productName,
                    companyName = PlayerSettings.companyName,
                    applicationIdentifier = SafeRead(() => PlayerSettings.GetApplicationIdentifier(namedBuildTarget)),
                    bundleVersion = PlayerSettings.bundleVersion,
                    activeBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString(),
                    selectedBuildTargetGroup = group.ToString(),
                    namedBuildTarget = namedBuildTarget.TargetName,
                    scriptingBackend = SafeRead(() => PlayerSettings.GetScriptingBackend(namedBuildTarget).ToString()),
                    apiCompatibilityLevel = SafeRead(() => PlayerSettings.GetApiCompatibilityLevel(namedBuildTarget).ToString()),
                    colorSpace = PlayerSettings.colorSpace.ToString(),
                    runInBackground = PlayerSettings.runInBackground,
                    defaultScreenWidth = PlayerSettings.defaultScreenWidth,
                    defaultScreenHeight = PlayerSettings.defaultScreenHeight,
                    resizableWindow = PlayerSettings.resizableWindow,
                    visibleInBackground = PlayerSettings.visibleInBackground
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect project settings.");
        }
    }

    private static string SafeRead(Func<string> read)
    {
        try
        {
            return read() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
}
