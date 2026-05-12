using System;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.ConfigureUitkTutorialPager,
    Description = "Add or update UitkTutorialPager page/button bindings on a UIDocument GameObject.",
    Group = UnityCliToolGroups.Visual)]
public static class ConfigureUitkTutorialPagerTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path.")]
        public string ScenePath { get; set; }

        [ToolParameter("UIDocument GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Page element names as array or comma-separated string.", Required = true)]
        public string PageElementNames { get; set; }

        [ToolParameter("Previous Button element name.", DefaultValue = "previous_button")]
        public string PreviousButtonName { get; set; }

        [ToolParameter("Next Button element name.", DefaultValue = "next_button")]
        public string NextButtonName { get; set; }

        [ToolParameter("Preview only without modifying the scene.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var pageNames = ParamUtility.ReadStringList(parameters, "pageElementNames");
            var previousButtonName = p.Get("previousButtonName", "previous_button");
            var nextButtonName = p.Get("nextButtonName", "next_button");
            var dryRun = p.GetBool("dryRun", true);

            UnityCliToolShared.GuardRequired(hierarchyPath, nameof(hierarchyPath));
            if (pageNames.Count == 0)
            {
                return ToolResult.Error("pageElementNames must contain at least one page element name.");
            }

            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var go, out var error))
            {
                return ToolResult.Error(error, new { scenePath, hierarchyPath });
            }

            if (dryRun)
            {
                return ToolResult.Success(
                    "UI Toolkit tutorial pager configuration previewed.",
                    new
                    {
                        dryRun,
                        target = HierarchyUtility.GetHierarchyPath(go.transform),
                        pageNames,
                        previousButtonName,
                        nextButtonName
                    });
            }

            UitkToolUtility.ConfigureTutorialPager(go, pageNames, previousButtonName, nextButtonName);
            return ToolResult.Success(
                "UI Toolkit tutorial pager configured.",
                new { dryRun = false, document = UitkToolUtility.DocumentSnapshot(go), pageNames });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to configure UI Toolkit tutorial pager.");
        }
    }
}
}
