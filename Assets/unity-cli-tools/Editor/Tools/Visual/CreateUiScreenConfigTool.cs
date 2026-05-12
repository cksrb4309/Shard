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
    Name = UnityCliToolNames.CreateUiScreenConfig,
    Description = "Create a UiScreenConfig ScriptableObject for screen-specific UI text and layout data.",
    Group = UnityCliToolGroups.Visual)]
public static class CreateUiScreenConfigTool
{
    public sealed class Parameters
    {
        [ToolParameter("Config asset path under Assets.", Required = true)]
        public string Path { get; set; }

        [ToolParameter("Initial title.")]
        public string Title { get; set; }

        [ToolParameter("Initial page texts as array or newline/comma separated string.")]
        public string PageTexts { get; set; }

        [ToolParameter("Overwrite existing config asset.", DefaultValue = "false")]
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

            var existing = AssetDatabase.LoadAssetAtPath<UiScreenConfig>(path);
            if (existing != null && !overwrite)
            {
                return ToolResult.Error("Screen config already exists and overwrite is false.", new { path });
            }

            var pageTexts = ParamUtility.ReadStringList(parameters, "pageTexts");
            if (dryRun)
            {
                return ToolResult.Success("UI screen config creation previewed.", new { dryRun, path, exists = existing != null, pageTextCount = pageTexts.Count });
            }

            EnsureFolder(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            var config = ScriptableObject.CreateInstance<UiScreenConfig>();
            var title = p.Get("title", string.Empty);
            if (!string.IsNullOrWhiteSpace(title))
            {
                config.title = title;
            }

            if (pageTexts.Count > 0)
            {
                config.pageTexts = new string[pageTexts.Count];
                for (var i = 0; i < pageTexts.Count; i++)
                {
                    config.pageTexts[i] = pageTexts[i];
                }
            }

            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            return ToolResult.Success(
                "UI screen config created.",
                new { dryRun = false, path, guid = AssetDatabase.AssetPathToGUID(path), title = config.title, pageTextCount = config.pageTexts.Length });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to create UI screen config.");
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
