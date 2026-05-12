using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Selection;
using UnityCliTools.Runtime.Ui;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace UnityCliTools.Infrastructure.Ui
{

public static class UitkToolUtility
{
    public static bool TryResolveScene(string scenePath, out UnityScene scene, out string error)
    {
        scenePath = UnityCliToolShared.TryConvertToAssetPath(scenePath);
        error = string.Empty;

        if (!string.IsNullOrWhiteSpace(scenePath))
        {
            foreach (var loadedScene in HierarchyUtility.EnumerateLoadedScenes())
            {
                if (string.Equals(loadedScene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    scene = loadedScene;
                    return true;
                }
            }

            scene = default;
            error = "Requested scene is not loaded.";
            return false;
        }

        scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            error = "No active loaded scene is available.";
            return false;
        }

        return true;
    }

    public static bool IsAssetPath(string path, string extension)
    {
        path = UnityCliToolShared.TryConvertToAssetPath(path);
        return path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeAssetPath(string path)
    {
        return UnityCliToolShared.TryConvertToAssetPath(path);
    }

    public static void EnsureParentDirectory(string assetPath)
    {
        var absolutePath = Path.GetFullPath(assetPath);
        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public static bool CanWriteAsset(string path, bool overwrite, out string error)
    {
        error = string.Empty;
        if (File.Exists(path) && !overwrite)
        {
            error = "Asset already exists and overwrite is false.";
            return false;
        }

        return true;
    }

    public static void WriteTextAsset(string assetPath, string text)
    {
        EnsureParentDirectory(assetPath);
        File.WriteAllText(Path.GetFullPath(assetPath), text, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(assetPath);
    }

    public static string MakeSafeClassOrAssetName(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value.Trim())
        {
            builder.Append(char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' ? ch : '_');
        }

        var result = builder.ToString().Trim('_', '-');
        return string.IsNullOrWhiteSpace(result) ? fallback : result;
    }

    public static IReadOnlyList<string> NormalizePageTexts(IReadOnlyList<string> values)
    {
        if (values != null && values.Count > 0)
        {
            return values;
        }

        return new[]
        {
            "Move with the controls shown on screen.",
            "Avoid hazards and follow objectives.",
            "Press X to close this tutorial."
        };
    }

    public static string BuildTutorialUxml(string title, IReadOnlyList<string> pageTexts)
    {
        var pages = new StringBuilder();
        for (var i = 0; i < pageTexts.Count; i++)
        {
            var displayClass = i == 0 ? "tutorial-page" : "tutorial-page hidden";
            pages.Append("            <ui:VisualElement name=\"page_").Append(i).Append("\" class=\"").Append(displayClass).Append("\">\n");
            pages.Append("                <ui:Label name=\"page_").Append(i).Append("_text\" class=\"body-text\" text=\"").Append(EscapeXml(pageTexts[i])).Append("\" />\n");
            pages.Append("            </ui:VisualElement>\n");
        }

        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"
            + "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" xmlns:uie=\"UnityEditor.UIElements\" editor-extension-mode=\"False\">\n"
            + "    <ui:VisualElement name=\"tutorial_root\" class=\"tutorial-root\">\n"
            + "        <ui:VisualElement name=\"tutorial_backdrop\" class=\"tutorial-backdrop\">\n"
            + "            <ui:VisualElement name=\"tutorial_card\" class=\"tutorial-card\">\n"
            + "                <ui:Label name=\"title_label\" class=\"title-text\" text=\"" + EscapeXml(title) + "\" />\n"
            + "                <ui:VisualElement name=\"page_container\" class=\"page-container\">\n"
            + pages
            + "                </ui:VisualElement>\n"
            + "                <ui:VisualElement name=\"button_row\" class=\"button-row\">\n"
            + "                    <ui:Button name=\"previous_button\" class=\"nav-button secondary-button\" text=\"&lt;\" />\n"
            + "                    <ui:Button name=\"next_button\" class=\"nav-button primary-button\" text=\"&gt;\" />\n"
            + "                </ui:VisualElement>\n"
            + "            </ui:VisualElement>\n"
            + "        </ui:VisualElement>\n"
            + "    </ui:VisualElement>\n"
            + "</ui:UXML>\n";
    }

    public static string BuildTutorialUss()
    {
        return ":root {\n"
            + "    --uitk-bg: rgba(8, 14, 22, 0.72);\n"
            + "    --uitk-panel: rgb(245, 241, 226);\n"
            + "    --uitk-text: rgb(18, 24, 32);\n"
            + "    --uitk-muted: rgb(84, 92, 102);\n"
            + "    --uitk-primary: rgb(35, 112, 92);\n"
            + "    --uitk-secondary: rgb(222, 214, 190);\n"
            + "    --uitk-radius: 24px;\n"
            + "    --uitk-title-size: 52px;\n"
            + "    --uitk-body-size: 30px;\n"
            + "}\n\n"
            + ".tutorial-root {\n"
            + "    position: absolute;\n"
            + "    left: 0;\n"
            + "    right: 0;\n"
            + "    top: 0;\n"
            + "    bottom: 0;\n"
            + "}\n\n"
            + ".tutorial-backdrop {\n"
            + "    flex-grow: 1;\n"
            + "    align-items: center;\n"
            + "    justify-content: center;\n"
            + "    padding-left: 5%;\n"
            + "    padding-right: 5%;\n"
            + "    padding-top: 5%;\n"
            + "    padding-bottom: 5%;\n"
            + "    background-color: var(--uitk-bg);\n"
            + "}\n\n"
            + ".tutorial-card {\n"
            + "    width: 78%;\n"
            + "    max-width: 1120px;\n"
            + "    min-width: 280px;\n"
            + "    max-height: 86%;\n"
            + "    padding-left: 52px;\n"
            + "    padding-right: 52px;\n"
            + "    padding-top: 42px;\n"
            + "    padding-bottom: 38px;\n"
            + "    border-top-left-radius: var(--uitk-radius);\n"
            + "    border-top-right-radius: var(--uitk-radius);\n"
            + "    border-bottom-left-radius: var(--uitk-radius);\n"
            + "    border-bottom-right-radius: var(--uitk-radius);\n"
            + "    background-color: var(--uitk-panel);\n"
            + "}\n\n"
            + ".title-text {\n"
            + "    font-size: var(--uitk-title-size);\n"
            + "    color: var(--uitk-text);\n"
            + "    -unity-font-style: bold;\n"
            + "    white-space: normal;\n"
            + "    margin-bottom: 28px;\n"
            + "}\n\n"
            + ".page-container {\n"
            + "    min-height: 180px;\n"
            + "    flex-grow: 1;\n"
            + "}\n\n"
            + ".tutorial-page {\n"
            + "    flex-grow: 1;\n"
            + "}\n\n"
            + ".body-text {\n"
            + "    font-size: var(--uitk-body-size);\n"
            + "    color: var(--uitk-muted);\n"
            + "    white-space: normal;\n"
            + "}\n\n"
            + ".button-row {\n"
            + "    flex-direction: row;\n"
            + "    justify-content: flex-end;\n"
            + "    margin-top: 34px;\n"
            + "}\n\n"
            + ".nav-button {\n"
            + "    min-width: 96px;\n"
            + "    min-height: 64px;\n"
            + "    margin-left: 16px;\n"
            + "    font-size: 34px;\n"
            + "    border-top-left-radius: 18px;\n"
            + "    border-top-right-radius: 18px;\n"
            + "    border-bottom-left-radius: 18px;\n"
            + "    border-bottom-right-radius: 18px;\n"
            + "}\n\n"
            + ".primary-button {\n"
            + "    background-color: var(--uitk-primary);\n"
            + "    color: white;\n"
            + "}\n\n"
            + ".secondary-button {\n"
            + "    background-color: var(--uitk-secondary);\n"
            + "    color: var(--uitk-text);\n"
            + "}\n\n"
            + ".hidden {\n"
            + "    display: none;\n"
            + "}\n\n"
            + "@media (max-width: 640px) {\n"
            + "    .tutorial-card {\n"
            + "        width: 94%;\n"
            + "        padding-left: 24px;\n"
            + "        padding-right: 24px;\n"
            + "        padding-top: 24px;\n"
            + "        padding-bottom: 22px;\n"
            + "    }\n\n"
            + "    .title-text {\n"
            + "        font-size: 34px;\n"
            + "    }\n\n"
            + "    .body-text {\n"
            + "        font-size: 22px;\n"
            + "    }\n"
            + "}\n";
    }

    public static GameObject CreateDocumentObject(
        UnityScene scene,
        string name,
        VisualTreeAsset visualTreeAsset,
        StyleSheet styleSheet,
        PanelSettings panelSettings)
    {
        var go = new GameObject(name, typeof(UIDocument), typeof(UitkStyleSheetBinding));
        Undo.RegisterCreatedObjectUndo(go, "Create UI Toolkit Document");
        SceneManager.MoveGameObjectToScene(go, scene);

        var document = go.GetComponent<UIDocument>();
        document.visualTreeAsset = visualTreeAsset;
        document.panelSettings = panelSettings;

        var binding = go.GetComponent<UitkStyleSheetBinding>();
        var serialized = new SerializedObject(binding);
        serialized.FindProperty("styleSheet").objectReferenceValue = styleSheet;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        return go;
    }

    public static void ConfigureTutorialPager(GameObject go, IReadOnlyList<string> pageNames, string previousButtonName, string nextButtonName)
    {
        var pager = go.GetComponent<UitkTutorialPager>();
        if (pager == null)
        {
            pager = Undo.AddComponent<UitkTutorialPager>(go);
        }

        var serialized = new SerializedObject(pager);
        serialized.FindProperty("previousButtonName").stringValue = string.IsNullOrWhiteSpace(previousButtonName) ? "previous_button" : previousButtonName;
        serialized.FindProperty("nextButtonName").stringValue = string.IsNullOrWhiteSpace(nextButtonName) ? "next_button" : nextButtonName;
        var pages = serialized.FindProperty("pageElementNames");
        pages.arraySize = pageNames.Count;
        for (var i = 0; i < pageNames.Count; i++)
        {
            pages.GetArrayElementAtIndex(i).stringValue = pageNames[i];
        }

        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(pager);
        EditorSceneManager.MarkSceneDirty(go.scene);
    }

    public static List<string> BuildPageNames(int count)
    {
        var names = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            names.Add("page_" + i);
        }

        return names;
    }

    public static object DocumentSnapshot(GameObject go)
    {
        if (go == null)
        {
            return null;
        }

        var document = go.GetComponent<UIDocument>();
        return new
        {
            name = go.name,
            hierarchyPath = HierarchyUtility.GetHierarchyPath(go.transform),
            scenePath = go.scene.path,
            hasUIDocument = document != null,
            visualTreeAsset = document != null && document.visualTreeAsset != null ? AssetDatabase.GetAssetPath(document.visualTreeAsset) : string.Empty,
            panelSettings = document != null && document.panelSettings != null ? AssetDatabase.GetAssetPath(document.panelSettings) : string.Empty,
            hasStyleSheetBinding = go.GetComponent<UitkStyleSheetBinding>() != null,
            hasTutorialPager = go.GetComponent<UitkTutorialPager>() != null
        };
    }

    public static string UpdateUssTokens(string uss, IDictionary<string, string> tokens)
    {
        foreach (var item in tokens)
        {
            var key = NormalizeTokenName(item.Key);
            var value = item.Value == null ? string.Empty : item.Value.Trim();
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var pattern = @"(?m)(^\s*" + Regex.Escape(key) + @"\s*:\s*)([^;]+)(;)";
            if (Regex.IsMatch(uss, pattern))
            {
                uss = Regex.Replace(uss, pattern, match => match.Groups[1].Value + value + match.Groups[3].Value);
                continue;
            }

            var rootMatch = Regex.Match(uss, @":root\s*\{");
            if (rootMatch.Success)
            {
                var insertAt = rootMatch.Index + rootMatch.Length;
                uss = uss.Insert(insertAt, "\n    " + key + ": " + value + ";");
            }
            else
            {
                uss = ":root {\n    " + key + ": " + value + ";\n}\n\n" + uss;
            }
        }

        return uss;
    }

    public static string NormalizeTokenName(string token)
    {
        token = token == null ? string.Empty : token.Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return string.Empty;
        }

        return token.StartsWith("--", StringComparison.Ordinal) ? token : "--" + token;
    }

    private static string EscapeXml(string value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return value
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}
}
