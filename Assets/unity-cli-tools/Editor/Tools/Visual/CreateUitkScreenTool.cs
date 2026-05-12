using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.CreateUitkScreen,
    Description = "Create a UI Toolkit screen asset set and optional scene UIDocument for menu/tutorial-style UI.",
    Group = UnityCliToolGroups.Visual)]
public static class CreateUitkScreenTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path for the UIDocument GameObject.")]
        public string ScenePath { get; set; }

        [ToolParameter("Screen name used for default asset and GameObject names.", DefaultValue = "TutorialScreen")]
        public string Name { get; set; }

        [ToolParameter("Folder under Assets for generated UXML/USS/PanelSettings.", DefaultValue = "Assets/UI/UITK")]
        public string FolderPath { get; set; }

        [ToolParameter("Optional explicit UXML asset path.")]
        public string UxmlPath { get; set; }

        [ToolParameter("Optional explicit USS asset path.")]
        public string UssPath { get; set; }

        [ToolParameter("Optional explicit PanelSettings asset path.")]
        public string PanelSettingsPath { get; set; }

        [ToolParameter("Title label text.", DefaultValue = "Tutorial")]
        public string Title { get; set; }

        [ToolParameter("Tutorial page texts as array or newline/comma separated string.")]
        public string PageTexts { get; set; }

        [ToolParameter("Create a scene GameObject with UIDocument.", DefaultValue = "true")]
        public bool CreateDocumentObject { get; set; }

        [ToolParameter("Add and configure UitkTutorialPager on the UIDocument GameObject.", DefaultValue = "true")]
        public bool AddTutorialPager { get; set; }

        [ToolParameter("Overwrite existing generated assets.", DefaultValue = "false")]
        public bool Overwrite { get; set; }

        [ToolParameter("Preview only without creating assets or scene objects.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var name = UitkToolUtility.MakeSafeClassOrAssetName(p.Get("name", "TutorialScreen"), "TutorialScreen");
            var folderPath = UitkToolUtility.NormalizeAssetPath(p.Get("folderPath", "Assets/UI/UITK")).TrimEnd('/');
            var uxmlPath = UitkToolUtility.NormalizeAssetPath(p.Get("uxmlPath", string.Empty));
            var ussPath = UitkToolUtility.NormalizeAssetPath(p.Get("ussPath", string.Empty));
            var panelSettingsPath = UitkToolUtility.NormalizeAssetPath(p.Get("panelSettingsPath", string.Empty));
            var title = p.Get("title", "Tutorial");
            var pageTexts = UitkToolUtility.NormalizePageTexts(ParamUtility.ReadStringList(parameters, "pageTexts"));
            var createDocumentObject = p.GetBool("createDocumentObject", true);
            var addTutorialPager = p.GetBool("addTutorialPager", true);
            var overwrite = p.GetBool("overwrite", false);
            var dryRun = p.GetBool("dryRun", true);
            var scene = default(UnityScene);

            if (string.IsNullOrWhiteSpace(uxmlPath)) uxmlPath = folderPath + "/" + name + ".uxml";
            if (string.IsNullOrWhiteSpace(ussPath)) ussPath = folderPath + "/" + name + ".uss";
            if (string.IsNullOrWhiteSpace(panelSettingsPath)) panelSettingsPath = folderPath + "/" + name + "PanelSettings.asset";

            if (!ValidatePaths(uxmlPath, ussPath, panelSettingsPath, out var pathError))
            {
                return ToolResult.Error(pathError, new { uxmlPath, ussPath, panelSettingsPath });
            }

            var canWriteUxml = UitkToolUtility.CanWriteAsset(uxmlPath, overwrite, out var uxmlError);
            var canWriteUss = UitkToolUtility.CanWriteAsset(ussPath, overwrite, out var ussError);
            var canWritePanel = UitkToolUtility.CanWriteAsset(panelSettingsPath, overwrite, out var panelError);
            if (!canWriteUxml || !canWriteUss || !canWritePanel)
            {
                return ToolResult.Error(
                    "Generated asset already exists.",
                    new { uxmlPath, uxmlError, ussPath, ussError, panelSettingsPath, panelError });
            }

            if (createDocumentObject && !UitkToolUtility.TryResolveScene(p.Get("scenePath", string.Empty), out scene, out var sceneError))
            {
                return ToolResult.Error(sceneError, new { scenePath = p.Get("scenePath", string.Empty) });
            }

            var pageNames = UitkToolUtility.BuildPageNames(pageTexts.Count);
            if (dryRun)
            {
                return ToolResult.Success(
                    "UI Toolkit screen creation previewed.",
                    new
                    {
                        dryRun,
                        name,
                        uxmlPath,
                        ussPath,
                        panelSettingsPath,
                        title,
                        pageTexts,
                        pageNames,
                        wouldCreateDocumentObject = createDocumentObject,
                        wouldAddTutorialPager = createDocumentObject && addTutorialPager
                    });
            }

            UitkToolUtility.WriteTextAsset(uxmlPath, UitkToolUtility.BuildTutorialUxml(title, pageTexts));
            UitkToolUtility.WriteTextAsset(ussPath, UitkToolUtility.BuildTutorialUss());

            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
            if (panelSettings == null || overwrite)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                UitkToolUtility.EnsureParentDirectory(panelSettingsPath);
                AssetDatabase.CreateAsset(panelSettings, panelSettingsPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject documentObject = null;
            if (createDocumentObject)
            {
                var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
                documentObject = UitkToolUtility.CreateDocumentObject(scene, name, visualTreeAsset, styleSheet, panelSettings);
                if (addTutorialPager)
                {
                    UitkToolUtility.ConfigureTutorialPager(documentObject, pageNames, "previous_button", "next_button");
                }
            }

            return ToolResult.Success(
                "UI Toolkit screen created.",
                new
                {
                    dryRun = false,
                    name,
                    uxmlPath,
                    ussPath,
                    panelSettingsPath,
                    pageNames,
                    document = UitkToolUtility.DocumentSnapshot(documentObject)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to create UI Toolkit screen.");
        }
    }

    private static bool ValidatePaths(string uxmlPath, string ussPath, string panelSettingsPath, out string error)
    {
        error = string.Empty;
        if (!UitkToolUtility.IsAssetPath(uxmlPath, ".uxml"))
        {
            error = "uxmlPath must be a .uxml asset under Assets.";
            return false;
        }

        if (!UitkToolUtility.IsAssetPath(ussPath, ".uss"))
        {
            error = "ussPath must be a .uss asset under Assets.";
            return false;
        }

        if (!UitkToolUtility.IsAssetPath(panelSettingsPath, ".asset"))
        {
            error = "panelSettingsPath must be a .asset under Assets.";
            return false;
        }

        return true;
    }
}
}
