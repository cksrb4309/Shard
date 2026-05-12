using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using PackageManagerInfo = UnityEditor.PackageManager.PackageInfo;

namespace UnityCliTools.Asset
{

[UnityCliTool(
    Name = UnityCliToolNames.AddressablesState,
    Description = "Inspect whether Addressables is installed and discover likely settings assets.",
    Group = UnityCliToolGroups.Asset)]
public static class AddressablesStateTool
{
    public sealed class Parameters
    {
        [ToolParameter("Max number of returned settings assets.", DefaultValue = "20")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 20, 1, 200);
            var package = PackageManagerInfo.GetAllRegisteredPackages()
                .FirstOrDefault(x => string.Equals(x.name, "com.unity.addressables", StringComparison.OrdinalIgnoreCase));
            var editorAssemblyLoaded = AppDomain.CurrentDomain.GetAssemblies()
                .Any(x => x.GetName().Name.IndexOf("Addressables", StringComparison.OrdinalIgnoreCase) >= 0);
            var settings = FindSettingsAssets(limit);

            return ToolResult.Success(
                "Addressables state inspected.",
                new
                {
                    packageInstalled = package != null,
                    packageVersion = package != null ? package.version : string.Empty,
                    packageSource = package != null ? package.source.ToString() : string.Empty,
                    editorAssemblyLoaded,
                    settingsAssetCount = settings.Count,
                    settingsAssets = settings
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect Addressables state.");
        }
    }

    private static List<object> FindSettingsAssets(int limit)
    {
        var rows = new List<object>();
        var guids = AssetDatabase.FindAssets("AddressableAssetSettings");
        foreach (var guid in guids)
        {
            if (rows.Count >= limit)
            {
                break;
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (type == null || type.Name.IndexOf("Addressable", StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            rows.Add(new
            {
                path,
                guid,
                type = type.FullName ?? type.Name
            });
        }

        return rows;
    }
}
}
