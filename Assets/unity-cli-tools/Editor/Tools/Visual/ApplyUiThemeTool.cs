using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;
using UnityCliTools.Runtime.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.ApplyUiTheme,
    Description = "Assign a UiTheme to a UI root and apply all child UiThemeBinding components.",
    Group = UnityCliToolGroups.Visual)]
public static class ApplyUiThemeTool
{
    public sealed class Parameters
    {
        [ToolParameter("UiTheme asset path.")]
        public string ThemePath { get; set; }

        [ToolParameter("UiTheme asset GUID.")]
        public string ThemeGuid { get; set; }

        [ToolParameter("Loaded scene path containing the UI root.")]
        public string ScenePath { get; set; }

        [ToolParameter("UI root hierarchy path.", Required = true)]
        public string RootHierarchyPath { get; set; }

        [ToolParameter("Include inactive child bindings.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Add UiThemeApplier to root if missing.", DefaultValue = "true")]
        public bool AddApplierIfMissing { get; set; }

        [ToolParameter("Preview only without assigning or applying the theme.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var rootPath = p.Get("rootHierarchyPath", string.Empty)?.Trim();
            var includeInactive = p.GetBool("includeInactive", true);
            var addApplier = p.GetBool("addApplierIfMissing", true);
            var dryRun = p.GetBool("dryRun", true);

            if (!UiThemeToolUtility.TryResolveTheme(parameters, out var theme, out var themePath, out var error))
            {
                return ToolResult.Error(error);
            }

            if (!UiToolUtility.TryResolveGameObject(scenePath, rootPath, out var root, out error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, rootHierarchyPath = rootPath ?? string.Empty });
            }

            var bindings = new List<UiThemeBinding>(root.GetComponentsInChildren<UiThemeBinding>(includeInactive));
            var applier = root.GetComponent<UiThemeApplier>();
            if (applier == null && !addApplier)
            {
                return ToolResult.Error("UiThemeApplier is missing and addApplierIfMissing is false.", UiToolUtility.ElementSnapshot(root));
            }

            if (!dryRun)
            {
                if (applier == null)
                {
                    applier = Undo.AddComponent<UiThemeApplier>(root);
                }
                else
                {
                    Undo.RecordObject(applier, "Apply UI Theme");
                }

                applier.Theme = theme;
                applier.IncludeInactive = includeInactive;
                var appliedCount = applier.ApplyTheme();
                EditorUtility.SetDirty(applier);
                UiToolUtility.MarkUiObjectDirty(root, "Apply UI Theme");
                bindings = new List<UiThemeBinding>(root.GetComponentsInChildren<UiThemeBinding>(includeInactive));

                return ToolResult.Success(
                    "UI theme applied.",
                    new { dryRun = false, themePath, root = UiToolUtility.ElementSnapshot(root), appliedCount, bindingCount = bindings.Count });
            }

            return ToolResult.Success(
                "UI theme application previewed.",
                new { dryRun, themePath, root = UiToolUtility.ElementSnapshot(root), bindingCount = bindings.Count, wouldAddApplier = applier == null && addApplier });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to apply UI theme.");
        }
    }
}
}
