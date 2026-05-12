using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Runtime.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.CreateUiTheme,
    Description = "Create a UiTheme ScriptableObject asset for theme-driven UI styling.",
    Group = UnityCliToolGroups.Visual)]
public static class CreateUiThemeTool
{
    public sealed class Parameters
    {
        [ToolParameter("Theme asset path under Assets.", Required = true)]
        public string Path { get; set; }

        [ToolParameter("Overwrite existing theme asset.", DefaultValue = "false")]
        public bool Overwrite { get; set; }

        [ToolParameter("Preview only without creating the asset.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var path = UnityCliToolShared.TryConvertToAssetPath(p.Get("path", string.Empty));
            var overwrite = p.GetBool("overwrite", false);
            var dryRun = p.GetBool("dryRun", true);

            if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) || !path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
            {
                return ToolResult.Error("path must be a .asset file under Assets.", new { path });
            }

            var existing = AssetDatabase.LoadAssetAtPath<UiTheme>(path);
            if (existing != null && !overwrite)
            {
                return ToolResult.Error("Theme already exists and overwrite is false.", new { path });
            }

            if (dryRun)
            {
                return ToolResult.Success("UI theme creation previewed.", new { dryRun, path, exists = existing != null, overwrite });
            }

            EnsureFolder(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            var theme = ScriptableObject.CreateInstance<UiTheme>();
            AssetDatabase.CreateAsset(theme, path);
            AssetDatabase.SaveAssets();

            return ToolResult.Success(
                "UI theme created.",
                new
                {
                    dryRun = false,
                    path,
                    guid = AssetDatabase.AssetPathToGUID(path),
                    name = theme.name
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to create UI theme.");
        }
    }

    private static void EnsureFolder(string path)
    {
        var directory = Path.GetDirectoryName(path)?.Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(directory) || AssetDatabase.IsValidFolder(directory))
        {
            return;
        }

        var parts = directory.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
}
