using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.CreateUitkDocument,
    Description = "Create a scene GameObject with UIDocument, PanelSettings, and optional runtime USS binding.",
    Group = UnityCliToolGroups.Visual)]
public static class CreateUitkDocumentTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path.", Required = true)]
        public string ScenePath { get; set; }

        [ToolParameter("New GameObject name.", DefaultValue = "UIDocument")]
        public string Name { get; set; }

        [ToolParameter("VisualTreeAsset .uxml path.", Required = true)]
        public string UxmlPath { get; set; }

        [ToolParameter("StyleSheet .uss path.")]
        public string UssPath { get; set; }

        [ToolParameter("PanelSettings asset path.", Required = true)]
        public string PanelSettingsPath { get; set; }

        [ToolParameter("Preview only without creating the GameObject.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = p.Get("scenePath", string.Empty);
            var name = UitkToolUtility.MakeSafeClassOrAssetName(p.Get("name", "UIDocument"), "UIDocument");
            var uxmlPath = UitkToolUtility.NormalizeAssetPath(p.Get("uxmlPath", string.Empty));
            var ussPath = UitkToolUtility.NormalizeAssetPath(p.Get("ussPath", string.Empty));
            var panelSettingsPath = UitkToolUtility.NormalizeAssetPath(p.Get("panelSettingsPath", string.Empty));
            var dryRun = p.GetBool("dryRun", true);

            UnityCliToolShared.GuardRequired(uxmlPath, nameof(uxmlPath));
            UnityCliToolShared.GuardRequired(panelSettingsPath, nameof(panelSettingsPath));

            if (!UitkToolUtility.TryResolveScene(scenePath, out var scene, out var sceneError))
            {
                return ToolResult.Error(sceneError, new { scenePath });
            }

            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            var styleSheet = string.IsNullOrWhiteSpace(ussPath) ? null : AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
            if (visualTreeAsset == null || panelSettings == null || (!string.IsNullOrWhiteSpace(ussPath) && styleSheet == null))
            {
                return ToolResult.Error(
                    "One or more UI Toolkit assets could not be loaded.",
                    new
                    {
                        uxmlPath,
                        uxmlLoaded = visualTreeAsset != null,
                        ussPath,
                        ussLoaded = string.IsNullOrWhiteSpace(ussPath) || styleSheet != null,
                        panelSettingsPath,
                        panelSettingsLoaded = panelSettings != null
                    });
            }

            if (dryRun)
            {
                return ToolResult.Success(
                    "UI Toolkit document creation previewed.",
                    new { dryRun, scenePath = scene.path, name, uxmlPath, ussPath, panelSettingsPath });
            }

            var go = UitkToolUtility.CreateDocumentObject(scene, name, visualTreeAsset, styleSheet, panelSettings);
            return ToolResult.Success("UI Toolkit document created.", new { dryRun = false, document = UitkToolUtility.DocumentSnapshot(go) });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to create UI Toolkit document.");
        }
    }
}
}
