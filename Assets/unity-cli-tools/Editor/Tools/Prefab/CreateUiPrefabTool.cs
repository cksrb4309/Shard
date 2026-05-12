using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Prefab
{

[UnityCliTool(
    Name = UnityCliToolNames.CreateUiPrefab,
    Description = "Save a scene UI hierarchy as a prefab asset.",
    Group = UnityCliToolGroups.Prefab)]
public static class CreateUiPrefabTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the source UI object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Source UI GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Prefab asset path under Assets.", Required = true)]
        public string PrefabPath { get; set; }

        [ToolParameter("Connect saved prefab to the scene instance.", DefaultValue = "false")]
        public bool Connect { get; set; }

        [ToolParameter("Overwrite an existing prefab asset.", DefaultValue = "false")]
        public bool Overwrite { get; set; }

        [ToolParameter("Preview only without saving the prefab.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = p.Get("scenePath", string.Empty);
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var prefabPath = UnityCliToolShared.TryConvertToAssetPath(p.Get("prefabPath", string.Empty));
            var connect = p.GetBool("connect", false);
            var overwrite = p.GetBool("overwrite", false);
            var dryRun = p.GetBool("dryRun", true);

            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var source, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, hierarchyPath = hierarchyPath ?? string.Empty });
            }

            if (!IsValidPrefabPath(prefabPath))
            {
                return ToolResult.Error("prefabPath must be a .prefab file under Assets.", new { prefabPath });
            }

            var exists = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
            if (exists && !overwrite)
            {
                return ToolResult.Error("Prefab already exists and overwrite is false.", new { prefabPath });
            }

            if (dryRun)
            {
                return ToolResult.Success(
                    "UI prefab creation previewed.",
                    new { dryRun, source = UiToolUtility.ElementSnapshot(source), prefabPath, exists, connect, overwrite });
            }

            EnsureFolder(prefabPath);
            GameObject saved;
            if (connect)
            {
                saved = PrefabUtility.SaveAsPrefabAssetAndConnect(source, prefabPath, InteractionMode.AutomatedAction);
            }
            else
            {
                saved = PrefabUtility.SaveAsPrefabAsset(source, prefabPath);
            }

            AssetDatabase.SaveAssets();
            return ToolResult.Success(
                "UI prefab saved.",
                new
                {
                    dryRun = false,
                    prefab = new
                    {
                        path = prefabPath,
                        guid = AssetDatabase.AssetPathToGUID(prefabPath),
                        name = saved != null ? saved.name : string.Empty
                    },
                    source = UiToolUtility.ElementSnapshot(source)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to create UI prefab.");
        }
    }

    private static bool IsValidPrefabPath(string path)
    {
        return !string.IsNullOrWhiteSpace(path)
            && path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase);
    }

    private static void EnsureFolder(string path)
    {
        var directory = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
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
