using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;
using UnityCliTools.Runtime.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.ValidateUiThemeBindings,
    Description = "Validate UiThemeApplier and UiThemeBinding coverage under a UI root.",
    Group = UnityCliToolGroups.Visual)]
public static class ValidateUiThemeBindingsTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the UI root.")]
        public string ScenePath { get; set; }

        [ToolParameter("UI root hierarchy path.", Required = true)]
        public string RootHierarchyPath { get; set; }

        [ToolParameter("Include inactive children.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Report themeable UI elements that have no UiThemeBinding.", DefaultValue = "true")]
        public bool RequireBindings { get; set; }

        [ToolParameter("Max number of returned issues.", DefaultValue = "500")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var rootPath = p.Get("rootHierarchyPath", string.Empty)?.Trim();
            var includeInactive = p.GetBool("includeInactive", true);
            var requireBindings = p.GetBool("requireBindings", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 500, 1, 5000);
            if (!UiToolUtility.TryResolveGameObject(scenePath, rootPath, out var root, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, rootHierarchyPath = rootPath ?? string.Empty });
            }

            var issues = new List<object>();
            var themeableCount = 0;
            var bindingCount = 0;
            var applier = root.GetComponent<UiThemeApplier>();
            if (applier == null)
            {
                AddIssue(issues, limit, "warning", "missing_theme_applier", "UI root has no UiThemeApplier.", root);
            }
            else if (applier.Theme == null)
            {
                AddIssue(issues, limit, "warning", "missing_theme_reference", "UiThemeApplier has no theme reference.", root);
            }

            foreach (var go in UiToolUtility.EnumerateChildren(root, includeInactive, 128))
            {
                var isThemeable = UiThemeToolUtility.HasApplicableTarget(go);
                if (isThemeable)
                {
                    themeableCount++;
                }

                var binding = go.GetComponent<UiThemeBinding>();
                if (binding != null)
                {
                    bindingCount++;
                    if (binding.Role == UiThemeRole.None)
                    {
                        AddIssue(issues, limit, "warning", "binding_role_none", "UiThemeBinding role is None.", go);
                    }

                    if (!isThemeable)
                    {
                        AddIssue(issues, limit, "info", "binding_without_themeable_target", "UiThemeBinding has no TMP_Text, Graphic, Image, or Button on the same GameObject.", go);
                    }
                }
                else if (requireBindings && isThemeable && !HasThemedAncestorSelectable(go))
                {
                    AddIssue(issues, limit, "info", "missing_theme_binding", "Themeable UI element has no UiThemeBinding.", go);
                }

                if (issues.Count >= limit)
                {
                    break;
                }
            }

            return ToolResult.Success(
                "UI theme bindings validated.",
                new
                {
                    query = new { scenePath = scenePath ?? string.Empty, rootHierarchyPath = rootPath ?? string.Empty, includeInactive, requireBindings, limit },
                    themeableCount,
                    bindingCount,
                    issueCount = issues.Count,
                    issues
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to validate UI theme bindings.");
        }
    }

    private static bool HasThemedAncestorSelectable(UnityEngine.GameObject go)
    {
        var cursor = go.transform.parent;
        while (cursor != null)
        {
            if (cursor.GetComponent<Selectable>() != null && cursor.GetComponent<UiThemeBinding>() != null)
            {
                return true;
            }

            cursor = cursor.parent;
        }

        return false;
    }

    private static void AddIssue(List<object> issues, int limit, string severity, string code, string message, UnityEngine.GameObject go)
    {
        if (issues.Count >= limit)
        {
            return;
        }

        issues.Add(new
        {
            severity,
            code,
            message,
            target = go != null ? UiToolUtility.ElementSnapshot(go) : null
        });
    }
}
}
