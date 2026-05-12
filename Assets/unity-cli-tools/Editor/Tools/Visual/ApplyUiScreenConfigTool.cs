using System;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;
using UnityCliTools.Runtime.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.ApplyUiScreenConfig,
    Description = "Apply UiScreenConfig text and responsive panel values to a scene UI.",
    Group = UnityCliToolGroups.Visual)]
public static class ApplyUiScreenConfigTool
{
    public sealed class Parameters
    {
        [ToolParameter("UiScreenConfig asset path.")]
        public string ConfigPath { get; set; }

        [ToolParameter("UiScreenConfig asset GUID.")]
        public string ConfigGuid { get; set; }

        [ToolParameter("Loaded scene path containing the UI.")]
        public string ScenePath { get; set; }

        [ToolParameter("Title TMP hierarchy path.")]
        public string TitleHierarchyPath { get; set; }

        [ToolParameter("Page body TMP hierarchy paths as array or comma-separated string.")]
        public string PageTextHierarchyPaths { get; set; }

        [ToolParameter("Previous button label TMP hierarchy path.")]
        public string PreviousLabelHierarchyPath { get; set; }

        [ToolParameter("Next button label TMP hierarchy path.")]
        public string NextLabelHierarchyPath { get; set; }

        [ToolParameter("Panel hierarchy path with ResponsiveRectTransform.")]
        public string PanelHierarchyPath { get; set; }

        [ToolParameter("Controller hierarchy path for previousText/nextText/closeText fields.")]
        public string ControllerHierarchyPath { get; set; }

        [ToolParameter("Controller component type name.")]
        public string ControllerComponentType { get; set; }

        [ToolParameter("Preview only without modifying scene UI.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var dryRun = p.GetBool("dryRun", true);
            if (!TryResolveConfig(p, out var config, out var configPath, out var error))
            {
                return ToolResult.Error(error);
            }

            var planned = 0;
            var applied = 0;
            ApplyText(scenePath, p.Get("titleHierarchyPath", string.Empty), config.title, dryRun, ref planned, ref applied);
            var pagePaths = ParamUtility.ReadStringList(parameters, "pageTextHierarchyPaths");
            for (var i = 0; i < pagePaths.Count && i < config.pageTexts.Length; i++)
            {
                ApplyText(scenePath, pagePaths[i], config.pageTexts[i], dryRun, ref planned, ref applied);
            }

            ApplyText(scenePath, p.Get("previousLabelHierarchyPath", string.Empty), config.previousText, dryRun, ref planned, ref applied);
            ApplyText(scenePath, p.Get("nextLabelHierarchyPath", string.Empty), config.nextText, dryRun, ref planned, ref applied);
            ApplyPanel(scenePath, p.Get("panelHierarchyPath", string.Empty), config, dryRun, ref planned, ref applied);
            ApplyController(parameters, config, dryRun, ref planned, ref applied);

            return ToolResult.Success(
                dryRun ? "UI screen config application previewed." : "UI screen config applied.",
                new { dryRun, configPath, plannedChangeCount = planned, appliedChangeCount = applied });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to apply UI screen config.");
        }
    }

    private static void ApplyText(string scenePath, string hierarchyPath, string value, bool dryRun, ref int planned, ref int applied)
    {
        if (string.IsNullOrWhiteSpace(hierarchyPath))
        {
            return;
        }

        planned++;
        if (dryRun)
        {
            return;
        }

        if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath.Trim(), out var go, out _))
        {
            return;
        }

        var tmp = go.GetComponent<TMP_Text>();
        if (tmp == null)
        {
            return;
        }

        Undo.RecordObject(tmp, "Apply UI Screen Config");
        tmp.text = value ?? string.Empty;
        EditorUtility.SetDirty(tmp);
        UiToolUtility.MarkUiObjectDirty(go, "Apply UI Screen Config");
        applied++;
    }

    private static void ApplyPanel(string scenePath, string hierarchyPath, UiScreenConfig config, bool dryRun, ref int planned, ref int applied)
    {
        if (string.IsNullOrWhiteSpace(hierarchyPath))
        {
            return;
        }

        planned++;
        if (dryRun)
        {
            return;
        }

        if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath.Trim(), out var go, out _))
        {
            return;
        }

        var responsive = go.GetComponent<ResponsiveRectTransform>();
        if (responsive == null)
        {
            responsive = Undo.AddComponent<ResponsiveRectTransform>(go);
        }
        else
        {
            Undo.RecordObject(responsive, "Apply UI Screen Config");
        }

        responsive.Margins = config.panelMargins;
        responsive.MinSize = config.panelMinSize;
        responsive.MaxSize = config.panelMaxSize;
        responsive.Apply();
        EditorUtility.SetDirty(responsive);
        UiToolUtility.MarkUiObjectDirty(go, "Apply UI Screen Config");
        applied++;
    }

    private static void ApplyController(JObject parameters, UiScreenConfig config, bool dryRun, ref int planned, ref int applied)
    {
        var p = new ToolParams(parameters);
        var controllerPath = p.Get("controllerHierarchyPath", string.Empty)?.Trim();
        var componentType = p.Get("controllerComponentType", string.Empty)?.Trim();
        if (string.IsNullOrWhiteSpace(controllerPath) || string.IsNullOrWhiteSpace(componentType))
        {
            return;
        }

        planned++;
        if (dryRun)
        {
            return;
        }

        if (!UiSerializedUtility.TryResolveComponent(p.Get("scenePath", string.Empty), controllerPath, componentType, 0, out var component, out var go, out _))
        {
            return;
        }

        Undo.RecordObject(component, "Apply UI Screen Config");
        var serialized = new SerializedObject(component);
        SetString(serialized, "previousText", config.previousText);
        SetString(serialized, "nextText", config.nextText);
        SetString(serialized, "closeText", config.closeText);
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(component);
        UiToolUtility.MarkUiObjectDirty(go, "Apply UI Screen Config");
        applied++;
    }

    private static void SetString(SerializedObject serialized, string propertyPath, string value)
    {
        var property = serialized.FindProperty(propertyPath);
        if (property != null && property.propertyType == SerializedPropertyType.String)
        {
            property.stringValue = value ?? string.Empty;
        }
    }

    private static bool TryResolveConfig(ToolParams p, out UiScreenConfig config, out string path, out string error)
    {
        config = null;
        path = UnityCliToolShared.TryConvertToAssetPath(p.Get("configPath", string.Empty));
        error = string.Empty;
        var guid = p.Get("configGuid", string.Empty)?.Trim();
        if (!string.IsNullOrWhiteSpace(guid))
        {
            path = AssetDatabase.GUIDToAssetPath(guid);
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            error = "configPath or configGuid is required.";
            return false;
        }

        config = AssetDatabase.LoadAssetAtPath<UiScreenConfig>(path);
        if (config == null)
        {
            error = "UiScreenConfig asset could not be resolved.";
            return false;
        }

        return true;
    }
}
}
