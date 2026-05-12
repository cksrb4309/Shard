using System;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    Name = UnityCliToolNames.CreateUiElement,
    Description = "Create common UGUI/TMP UI elements under a Canvas or RectTransform.",
    Group = UnityCliToolGroups.Visual)]
public static class CreateUiElementTool
{
    public sealed class Parameters
    {
        [ToolParameter("Element kind: canvas, panel, image, button, tmp_text, empty.")]
        public string Kind { get; set; }

        [ToolParameter("New GameObject name.", DefaultValue = "UI Element")]
        public string Name { get; set; }

        [ToolParameter("Loaded scene path. If omitted, uses active scene or parent scene.")]
        public string ScenePath { get; set; }

        [ToolParameter("Parent hierarchy path. Required except for root canvas.")]
        public string ParentHierarchyPath { get; set; }

        [ToolParameter("TMP text value for text or button label.")]
        public string Text { get; set; }

        [ToolParameter("Anchored position as x,y.")]
        public string AnchoredPosition { get; set; }

        [ToolParameter("Size delta as x,y.")]
        public string SizeDelta { get; set; }

        [ToolParameter("Anchor min as x,y.")]
        public string AnchorMin { get; set; }

        [ToolParameter("Anchor max as x,y.")]
        public string AnchorMax { get; set; }

        [ToolParameter("Pivot as x,y.")]
        public string Pivot { get; set; }

        [ToolParameter("Color as r,g,b,a.")]
        public string Color { get; set; }

        [ToolParameter("Create EventSystem if missing.", DefaultValue = "true")]
        public bool EnsureEventSystem { get; set; }

        [ToolParameter("Optional theme role to bind after creation, e.g. panel, title_text, primary_button.")]
        public string ThemeRole { get; set; }

        [ToolParameter("Add UiThemeBinding when themeRole is provided.", DefaultValue = "true")]
        public bool BindTheme { get; set; }

        [ToolParameter("Preview only without creating the element.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var kind = NormalizeKind(p.Get("kind", "empty"));
            var name = string.IsNullOrWhiteSpace(p.Get("name", string.Empty)) ? DefaultName(kind) : p.Get("name", string.Empty).Trim();
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var parentPath = p.Get("parentHierarchyPath", string.Empty)?.Trim();
            var text = p.Get("text", string.Empty);
            var ensureEventSystem = p.GetBool("ensureEventSystem", true);
            var bindTheme = p.GetBool("bindTheme", true);
            var dryRun = p.GetBool("dryRun", true);
            if (!UiThemeToolUtility.TryReadRole(p.Get("themeRole", string.Empty), out var themeRole, out var roleError))
            {
                return ToolResult.Error(roleError);
            }

            if (!TryResolveParent(scenePath, parentPath, kind, out var scene, out var parent, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, parentHierarchyPath = parentPath ?? string.Empty, kind });
            }

            if (!IsSupportedKind(kind))
            {
                return ToolResult.Error("Unsupported UI element kind.", new { kind });
            }

            if (dryRun)
            {
                return ToolResult.Success(
                    "UI element creation previewed.",
                    new
                    {
                        dryRun,
                        kind,
                        name,
                        scenePath = scene.path,
                        parentHierarchyPath = parentPath ?? string.Empty,
                        wouldEnsureEventSystem = ensureEventSystem && (kind == "canvas" || kind == "button"),
                        themeRole = themeRole.ToString(),
                        wouldBindTheme = bindTheme && themeRole != UiThemeRole.None
                    });
            }

            var created = Create(kind, name, null, text);
            SceneManager.MoveGameObjectToScene(created, scene);
            if (parent != null)
            {
                created.transform.SetParent(parent.transform, false);
            }

            ApplyRect(parameters, created.transform as RectTransform);
            ApplyColor(parameters, created);

            if (ensureEventSystem && (kind == "canvas" || kind == "button"))
            {
                UiToolUtility.EnsureEventSystem(scene);
            }

            if (bindTheme && themeRole != UiThemeRole.None)
            {
                UiThemeToolUtility.AddOrUpdateBinding(created, themeRole, true, true, true, true, true, true);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            return ToolResult.Success(
                "UI element created.",
                new
                {
                    dryRun = false,
                    created = UiToolUtility.ElementSnapshot(created)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to create UI element.");
        }
    }

    private static GameObject Create(string kind, string name, Transform parent, string text)
    {
        if (kind == "canvas")
        {
            var go = UiToolUtility.CreateUiGameObject(name, parent, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            Stretch(go.transform as RectTransform);
            return go;
        }

        if (kind == "panel")
        {
            var go = UiToolUtility.CreateUiGameObject(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
            Stretch(go.transform as RectTransform);
            return go;
        }

        if (kind == "image")
        {
            return UiToolUtility.CreateUiGameObject(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        }

        if (kind == "button")
        {
            var go = UiToolUtility.CreateUiGameObject(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.GetComponent<Image>().color = Color.white;
            var label = UiToolUtility.CreateUiGameObject("Text", go.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            Stretch(label.transform as RectTransform);
            var tmp = label.GetComponent<TextMeshProUGUI>();
            tmp.text = string.IsNullOrEmpty(text) ? "Button" : text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;
            tmp.fontSize = 24f;
            return go;
        }

        if (kind == "tmp_text")
        {
            var go = UiToolUtility.CreateUiGameObject(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text ?? string.Empty;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontSize = 32f;
            return go;
        }

        return UiToolUtility.CreateUiGameObject(name, parent, typeof(RectTransform));
    }

    private static void ApplyRect(JObject parameters, RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        if (ParamUtility.TryReadVector2(parameters, "anchorMin", out var anchorMin)) rect.anchorMin = anchorMin;
        if (ParamUtility.TryReadVector2(parameters, "anchorMax", out var anchorMax)) rect.anchorMax = anchorMax;
        if (ParamUtility.TryReadVector2(parameters, "pivot", out var pivot)) rect.pivot = pivot;
        if (ParamUtility.TryReadVector2(parameters, "anchoredPosition", out var anchoredPosition)) rect.anchoredPosition = anchoredPosition;
        if (ParamUtility.TryReadVector2(parameters, "sizeDelta", out var sizeDelta)) rect.sizeDelta = sizeDelta;
    }

    private static void ApplyColor(JObject parameters, GameObject go)
    {
        if (!ParamUtility.TryReadColor(parameters, "color", out var color))
        {
            return;
        }

        var graphic = go.GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.color = color;
            EditorUtility.SetDirty(graphic);
        }
    }

    private static void Stretch(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static bool TryResolveParent(string scenePath, string parentPath, string kind, out UnityEngine.SceneManagement.Scene scene, out GameObject parent, out string error)
    {
        parent = null;
        error = string.Empty;

        if (!string.IsNullOrWhiteSpace(parentPath))
        {
            if (!UiToolUtility.TryResolveGameObject(scenePath, parentPath, out parent, out error))
            {
                scene = default;
                return false;
            }

            scene = parent.scene;
            return true;
        }

        if (kind != "canvas")
        {
            scene = default;
            error = "parentHierarchyPath is required for non-canvas UI elements.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(scenePath))
        {
            foreach (var loadedScene in UnityCliTools.Infrastructure.Selection.HierarchyUtility.EnumerateLoadedScenes())
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

    private static string NormalizeKind(string kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            return "empty";
        }

        return kind.Trim().ToLowerInvariant().Replace("-", "_");
    }

    private static bool IsSupportedKind(string kind)
    {
        return kind == "canvas"
            || kind == "panel"
            || kind == "image"
            || kind == "button"
            || kind == "tmp_text"
            || kind == "empty";
    }

    private static string DefaultName(string kind)
    {
        return kind == "tmp_text" ? "Text" : char.ToUpperInvariant(kind[0]) + kind.Substring(1);
    }
}
}
