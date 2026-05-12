using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.SetUitkUssTokens,
    Description = "Batch update CSS custom property tokens in a UI Toolkit USS file.",
    Group = UnityCliToolGroups.Visual)]
public static class SetUitkUssTokensTool
{
    public sealed class Parameters
    {
        [ToolParameter("USS asset path under Assets.", Required = true)]
        public string UssPath { get; set; }

        [ToolParameter("Object map of token name to USS value, e.g. {\"uitk-primary\":\"rgb(0, 120, 90)\"}.", Required = true)]
        public string Tokens { get; set; }

        [ToolParameter("Preview only without modifying the USS asset.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var ussPath = UitkToolUtility.NormalizeAssetPath(p.Get("ussPath", string.Empty));
            var dryRun = p.GetBool("dryRun", true);
            UnityCliToolShared.GuardRequired(ussPath, nameof(ussPath));

            if (!UitkToolUtility.IsAssetPath(ussPath, ".uss") || !File.Exists(ussPath))
            {
                return ToolResult.Error("ussPath must be an existing .uss asset under Assets.", new { ussPath });
            }

            var tokens = ReadTokens(parameters["tokens"] as JObject);
            if (tokens.Count == 0)
            {
                return ToolResult.Error("tokens must contain at least one key/value pair.");
            }

            var before = File.ReadAllText(ussPath);
            var after = UitkToolUtility.UpdateUssTokens(before, tokens);
            if (!dryRun)
            {
                File.WriteAllText(Path.GetFullPath(ussPath), after);
                AssetDatabase.ImportAsset(ussPath);
            }

            return ToolResult.Success(
                dryRun ? "UI Toolkit USS token update previewed." : "UI Toolkit USS tokens updated.",
                new
                {
                    dryRun,
                    ussPath,
                    tokenCount = tokens.Count,
                    changed = !string.Equals(before, after, StringComparison.Ordinal)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to update UI Toolkit USS tokens.");
        }
    }

    private static Dictionary<string, string> ReadTokens(JObject source)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (source == null)
        {
            return values;
        }

        foreach (var item in source)
        {
            values[item.Key] = item.Value == null ? string.Empty : item.Value.ToString();
        }

        return values;
    }
}
}
