using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityCliTools.Infrastructure.Ui;
using UnityCliTools.Runtime.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.ValidateUitkUi,
    Description = "Validate UI Toolkit UIDocument assets, USS tokens, required elements, and optional tutorial pager bindings.",
    Group = UnityCliToolGroups.Visual)]
public static class ValidateUitkUiTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path.")]
        public string ScenePath { get; set; }

        [ToolParameter("UIDocument GameObject hierarchy path. If omitted, validates all loaded UIDocuments.")]
        public string HierarchyPath { get; set; }

        [ToolParameter("Required VisualElement names as array or comma-separated string.")]
        public string RequiredElementNames { get; set; }

        [ToolParameter("Required USS tokens as array or comma-separated string.")]
        public string RequiredTokens { get; set; }

        [ToolParameter("Validate UitkTutorialPager if present.", DefaultValue = "true")]
        public bool ValidateTutorialPager { get; set; }

        [ToolParameter("Include inactive scene objects.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Max returned issues.", DefaultValue = "500")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var requiredElementNames = ParamUtility.ReadStringList(parameters, "requiredElementNames");
            var requiredTokens = ParamUtility.ReadStringList(parameters, "requiredTokens");
            var includeInactive = p.GetBool("includeInactive", true);
            var validateTutorialPager = p.GetBool("validateTutorialPager", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 500, 1, 5000);
            var documents = ResolveDocuments(scenePath, hierarchyPath, includeInactive);
            var issues = new List<object>();

            foreach (var go in documents)
            {
                ValidateDocument(go, requiredElementNames, requiredTokens, validateTutorialPager, issues, limit);
            }

            if (!string.IsNullOrWhiteSpace(hierarchyPath) && documents.Count == 0)
            {
                AddIssue(issues, limit, "error", "document_not_found", "UIDocument GameObject was not found.", null);
            }

            return ToolResult.Success(
                "UI Toolkit UI validated.",
                new
                {
                    query = new { scenePath, hierarchyPath, includeInactive, requiredElementNames, requiredTokens },
                    documentCount = documents.Count,
                    issueCount = issues.Count,
                    issues
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to validate UI Toolkit UI.");
        }
    }

    private static List<UnityEngine.GameObject> ResolveDocuments(string scenePath, string hierarchyPath, bool includeInactive)
    {
        var documents = new List<UnityEngine.GameObject>();
        if (!string.IsNullOrWhiteSpace(hierarchyPath))
        {
            if (UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var go, out _) && go.GetComponent<UIDocument>() != null)
            {
                documents.Add(go);
            }

            return documents;
        }

        foreach (var go in UiToolUtility.EnumerateLoadedSceneObjects(scenePath, includeInactive))
        {
            if (go.GetComponent<UIDocument>() != null)
            {
                documents.Add(go);
            }
        }

        return documents;
    }

    private static void ValidateDocument(
        UnityEngine.GameObject go,
        IReadOnlyList<string> requiredElementNames,
        IReadOnlyList<string> requiredTokens,
        bool validateTutorialPager,
        List<object> issues,
        int limit)
    {
        var document = go.GetComponent<UIDocument>();
        if (document.visualTreeAsset == null)
        {
            AddIssue(issues, limit, "error", "missing_visual_tree_asset", "UIDocument has no VisualTreeAsset.", go);
        }

        if (document.panelSettings == null)
        {
            AddIssue(issues, limit, "error", "missing_panel_settings", "UIDocument has no PanelSettings.", go);
        }

        var uxmlPath = document.visualTreeAsset != null ? AssetDatabase.GetAssetPath(document.visualTreeAsset) : string.Empty;
        var uxmlText = !string.IsNullOrWhiteSpace(uxmlPath) && File.Exists(uxmlPath) ? File.ReadAllText(uxmlPath) : string.Empty;
        foreach (var elementName in requiredElementNames)
        {
            if (uxmlText.IndexOf("name=\"" + elementName + "\"", StringComparison.Ordinal) < 0)
            {
                AddIssue(issues, limit, "error", "required_element_missing", "Required UXML element name is missing: " + elementName, go);
            }
        }

        var binding = go.GetComponent<UitkStyleSheetBinding>();
        if (binding == null)
        {
            AddIssue(issues, limit, "warning", "missing_stylesheet_binding", "UIDocument has no UitkStyleSheetBinding, so generated USS may not apply at runtime.", go);
        }

        var ussText = ReadBoundUssText(go);
        foreach (var token in requiredTokens)
        {
            var normalizedToken = UitkToolUtility.NormalizeTokenName(token);
            if (ussText.IndexOf(normalizedToken, StringComparison.Ordinal) < 0)
            {
                AddIssue(issues, limit, "warning", "required_token_missing", "Required USS token is missing: " + normalizedToken, go);
            }
        }

        if (validateTutorialPager)
        {
            ValidatePager(go, uxmlText, issues, limit);
        }
    }

    private static string ReadBoundUssText(UnityEngine.GameObject go)
    {
        var binding = go.GetComponent<UitkStyleSheetBinding>();
        if (binding == null)
        {
            return string.Empty;
        }

        var serialized = new SerializedObject(binding);
        var styleSheet = serialized.FindProperty("styleSheet").objectReferenceValue as StyleSheet;
        var ussPath = styleSheet != null ? AssetDatabase.GetAssetPath(styleSheet) : string.Empty;
        return !string.IsNullOrWhiteSpace(ussPath) && File.Exists(ussPath) ? File.ReadAllText(ussPath) : string.Empty;
    }

    private static void ValidatePager(UnityEngine.GameObject go, string uxmlText, List<object> issues, int limit)
    {
        var pager = go.GetComponent<UitkTutorialPager>();
        if (pager == null)
        {
            return;
        }

        var serialized = new SerializedObject(pager);
        ValidateNamedElement(serialized.FindProperty("previousButtonName").stringValue, "pager_previous_button_missing", uxmlText, go, issues, limit);
        ValidateNamedElement(serialized.FindProperty("nextButtonName").stringValue, "pager_next_button_missing", uxmlText, go, issues, limit);

        var pages = serialized.FindProperty("pageElementNames");
        if (pages.arraySize == 0)
        {
            AddIssue(issues, limit, "warning", "pager_pages_empty", "UitkTutorialPager has no page element names.", go);
        }

        for (var i = 0; i < pages.arraySize; i++)
        {
            ValidateNamedElement(pages.GetArrayElementAtIndex(i).stringValue, "pager_page_missing", uxmlText, go, issues, limit);
        }
    }

    private static void ValidateNamedElement(string elementName, string code, string uxmlText, UnityEngine.GameObject go, List<object> issues, int limit)
    {
        if (string.IsNullOrWhiteSpace(elementName))
        {
            AddIssue(issues, limit, "error", code, "Pager element name is empty.", go);
            return;
        }

        if (uxmlText.IndexOf("name=\"" + elementName + "\"", StringComparison.Ordinal) < 0)
        {
            AddIssue(issues, limit, "error", code, "Pager target element is missing in UXML: " + elementName, go);
        }
    }

    private static void AddIssue(List<object> issues, int limit, string severity, string code, string message, UnityEngine.GameObject target)
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
            target = target == null ? null : UitkToolUtility.DocumentSnapshot(target)
        });
    }
}
}
