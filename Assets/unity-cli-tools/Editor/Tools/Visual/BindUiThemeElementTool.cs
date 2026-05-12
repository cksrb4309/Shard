using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.BindUiThemeElement,
    Description = "Add or configure a UiThemeBinding on a UI element.",
    Group = UnityCliToolGroups.Visual)]
public static class BindUiThemeElementTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target UI GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Theme role, e.g. panel, title_text, primary_button.", Required = true)]
        public string Role { get; set; }

        [ToolParameter("Apply Graphic color.", DefaultValue = "true")]
        public bool ApplyGraphic { get; set; }

        [ToolParameter("Apply TMP text color.", DefaultValue = "true")]
        public bool ApplyText { get; set; }

        [ToolParameter("Apply Button ColorBlock.", DefaultValue = "true")]
        public bool ApplyButton { get; set; }

        [ToolParameter("Apply Image sprite token.", DefaultValue = "true")]
        public bool ApplySprite { get; set; }

        [ToolParameter("Apply TMP font token.", DefaultValue = "true")]
        public bool ApplyFont { get; set; }

        [ToolParameter("Apply TMP font size token.", DefaultValue = "true")]
        public bool ApplyFontSize { get; set; }

        [ToolParameter("Preview only without modifying the target.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var dryRun = p.GetBool("dryRun", true);
            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, hierarchyPath = hierarchyPath ?? string.Empty });
            }

            if (!UiThemeToolUtility.TryReadRole(p.Get("role", string.Empty), out var role, out error) || role == Runtime.Ui.UiThemeRole.None)
            {
                return ToolResult.Error(string.IsNullOrWhiteSpace(error) ? "role is required." : error);
            }

            var before = UiThemeToolUtility.BindingSnapshot(target.GetComponent<Runtime.Ui.UiThemeBinding>());
            if (!dryRun)
            {
                UiThemeToolUtility.AddOrUpdateBinding(
                    target,
                    role,
                    p.GetBool("applyGraphic", true),
                    p.GetBool("applyText", true),
                    p.GetBool("applyButton", true),
                    p.GetBool("applySprite", true),
                    p.GetBool("applyFont", true),
                    p.GetBool("applyFontSize", true));
                UiToolUtility.MarkUiObjectDirty(target, "Bind UI Theme Element");
            }

            return ToolResult.Success(
                dryRun ? "UI theme binding previewed." : "UI theme binding updated.",
                new
                {
                    dryRun,
                    target = UiToolUtility.ElementSnapshot(target),
                    role = role.ToString(),
                    before,
                    after = dryRun ? before : UiThemeToolUtility.BindingSnapshot(target.GetComponent<Runtime.Ui.UiThemeBinding>())
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to bind UI theme element.");
        }
    }
}
}
