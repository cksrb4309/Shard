using System;
using System.Collections.Generic;
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
using UnityCliTools.Infrastructure.Selection;
using UnityCliTools.Infrastructure.Ui;
using UnityCliTools.Runtime.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.CreateUiControl,
    Description = "Create reusable UGUI/TMP controls: Toggle, Slider, ScrollView, TMP_InputField, or TMP_Dropdown.",
    Group = UnityCliToolGroups.Visual)]
public static class CreateUiControlTool
{
    public sealed class Parameters
    {
        [ToolParameter("Control kind: toggle, slider, scroll_view, input_field, dropdown.", Required = true)]
        public string Kind { get; set; }

        [ToolParameter("New control GameObject name.")]
        public string Name { get; set; }

        [ToolParameter("Loaded scene path.")]
        public string ScenePath { get; set; }

        [ToolParameter("Parent hierarchy path.", Required = true)]
        public string ParentHierarchyPath { get; set; }

        [ToolParameter("Label or placeholder text.")]
        public string Text { get; set; }

        [ToolParameter("Dropdown options as array or comma-separated list.")]
        public string Options { get; set; }

        [ToolParameter("Anchored position as x,y.")]
        public string AnchoredPosition { get; set; }

        [ToolParameter("Size delta as x,y.")]
        public string SizeDelta { get; set; }

        [ToolParameter("Optional theme role to bind after creation, e.g. primary_button.")]
        public string ThemeRole { get; set; }

        [ToolParameter("Add UiThemeBinding when themeRole is provided.", DefaultValue = "true")]
        public bool BindTheme { get; set; }

        [ToolParameter("Preview only without creating the control.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var kind = Normalize(p.Get("kind", string.Empty));
            var name = string.IsNullOrWhiteSpace(p.Get("name", string.Empty)) ? DefaultName(kind) : p.Get("name", string.Empty).Trim();
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var parentPath = p.Get("parentHierarchyPath", string.Empty)?.Trim();
            var text = p.Get("text", string.Empty);
            var bindTheme = p.GetBool("bindTheme", true);
            var dryRun = p.GetBool("dryRun", true);
            if (!UiThemeToolUtility.TryReadRole(p.Get("themeRole", string.Empty), out var themeRole, out var roleError))
            {
                return ToolResult.Error(roleError);
            }

            if (!IsSupported(kind))
            {
                return ToolResult.Error("Unsupported control kind.", new { kind });
            }

            if (!UiToolUtility.TryResolveGameObject(scenePath, parentPath, out var parent, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, parentHierarchyPath = parentPath ?? string.Empty });
            }

            if (dryRun)
            {
                return ToolResult.Success(
                    "UI control creation previewed.",
                    new { dryRun, kind, name, scenePath = parent.scene.path, parentHierarchyPath = parentPath, themeRole = themeRole.ToString(), wouldBindTheme = bindTheme && themeRole != UiThemeRole.None });
            }

            var created = Create(kind, name, text, ParamUtility.ReadStringList(parameters, "options"));
            SceneManager.MoveGameObjectToScene(created, parent.scene);
            created.transform.SetParent(parent.transform, false);
            ApplyRect(parameters, created.transform as RectTransform);
            if (bindTheme && themeRole != UiThemeRole.None)
            {
                UiThemeToolUtility.AddOrUpdateBinding(created, themeRole, true, true, true, true, true, true);
            }

            UiToolUtility.EnsureEventSystem(parent.scene);
            EditorSceneManager.MarkSceneDirty(parent.scene);

            return ToolResult.Success(
                "UI control created.",
                new { dryRun = false, created = UiToolUtility.ElementSnapshot(created) });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to create UI control.");
        }
    }

    private static GameObject Create(string kind, string name, string text, IReadOnlyList<string> options)
    {
        if (kind == "toggle") return CreateToggle(name, text);
        if (kind == "slider") return CreateSlider(name);
        if (kind == "scroll_view") return CreateScrollView(name);
        if (kind == "input_field") return CreateInputField(name, text);
        return CreateDropdown(name, options);
    }

    private static GameObject CreateToggle(string name, string text)
    {
        var root = UiToolUtility.CreateUiGameObject(name, null, typeof(RectTransform), typeof(LayoutElement), typeof(Toggle));
        Size(root, 220f, 32f);
        var background = Child("Background", root.transform, typeof(Image));
        Size(background, 24f, 24f);
        var checkmark = Child("Checkmark", background.transform, typeof(Image));
        Stretch(checkmark.transform as RectTransform);
        var label = Child("Label", root.transform, typeof(TextMeshProUGUI));
        var labelRect = label.transform as RectTransform;
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(32f, 0f);
        labelRect.offsetMax = Vector2.zero;
        var tmp = label.GetComponent<TextMeshProUGUI>();
        tmp.text = string.IsNullOrEmpty(text) ? "Toggle" : text;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
        var toggle = root.GetComponent<Toggle>();
        toggle.targetGraphic = background.GetComponent<Image>();
        toggle.graphic = checkmark.GetComponent<Image>();
        return root;
    }

    private static GameObject CreateSlider(string name)
    {
        var root = UiToolUtility.CreateUiGameObject(name, null, typeof(RectTransform), typeof(LayoutElement), typeof(Slider));
        Size(root, 240f, 24f);
        var background = Child("Background", root.transform, typeof(Image));
        Stretch(background.transform as RectTransform);
        background.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);
        var fillArea = Child("Fill Area", root.transform, typeof(RectTransform));
        Stretch(fillArea.transform as RectTransform);
        var fill = Child("Fill", fillArea.transform, typeof(Image));
        Stretch(fill.transform as RectTransform);
        var handleArea = Child("Handle Slide Area", root.transform, typeof(RectTransform));
        Stretch(handleArea.transform as RectTransform);
        var handle = Child("Handle", handleArea.transform, typeof(Image));
        Size(handle, 20f, 28f);
        var slider = root.GetComponent<Slider>();
        slider.fillRect = fill.transform as RectTransform;
        slider.handleRect = handle.transform as RectTransform;
        slider.targetGraphic = handle.GetComponent<Image>();
        return root;
    }

    private static GameObject CreateScrollView(string name)
    {
        var root = UiToolUtility.CreateUiGameObject(name, null, typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(ScrollRect));
        Size(root, 400f, 300f);
        var viewport = Child("Viewport", root.transform, typeof(Image), typeof(Mask));
        Stretch(viewport.transform as RectTransform);
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        var content = Child("Content", viewport.transform, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var contentRect = content.transform as RectTransform;
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 300f);
        var fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scroll = root.GetComponent<ScrollRect>();
        scroll.viewport = viewport.transform as RectTransform;
        scroll.content = contentRect;
        scroll.horizontal = false;
        return root;
    }

    private static GameObject CreateInputField(string name, string placeholder)
    {
        var root = UiToolUtility.CreateUiGameObject(name, null, typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(TMP_InputField));
        Size(root, 260f, 44f);
        var textArea = Child("Text Area", root.transform, typeof(RectTransform));
        Stretch(textArea.transform as RectTransform);
        var placeholderGo = Child("Placeholder", textArea.transform, typeof(TextMeshProUGUI));
        Stretch(placeholderGo.transform as RectTransform);
        var placeholderText = placeholderGo.GetComponent<TextMeshProUGUI>();
        placeholderText.text = string.IsNullOrEmpty(placeholder) ? "Enter text..." : placeholder;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.75f);
        placeholderText.raycastTarget = false;
        var textGo = Child("Text", textArea.transform, typeof(TextMeshProUGUI));
        Stretch(textGo.transform as RectTransform);
        var input = root.GetComponent<TMP_InputField>();
        input.textViewport = textArea.transform as RectTransform;
        input.textComponent = textGo.GetComponent<TextMeshProUGUI>();
        input.placeholder = placeholderText;
        return root;
    }

    private static GameObject CreateDropdown(string name, IReadOnlyList<string> options)
    {
        var root = UiToolUtility.CreateUiGameObject(name, null, typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(TMP_Dropdown));
        Size(root, 260f, 44f);
        var label = Child("Label", root.transform, typeof(TextMeshProUGUI));
        Stretch(label.transform as RectTransform);
        var arrow = Child("Arrow", root.transform, typeof(Image));
        Size(arrow, 24f, 24f);
        var dropdown = root.GetComponent<TMP_Dropdown>();
        dropdown.captionText = label.GetComponent<TextMeshProUGUI>();
        dropdown.captionText.raycastTarget = false;
        dropdown.template = CreateDropdownTemplate(root.transform);
        dropdown.options.Clear();
        if (options.Count == 0)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData("Option A"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("Option B"));
        }
        else
        {
            foreach (var option in options)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(option));
            }
        }

        dropdown.RefreshShownValue();
        return root;
    }

    private static RectTransform CreateDropdownTemplate(Transform parent)
    {
        var template = Child("Template", parent, typeof(Image), typeof(ScrollRect));
        var templateRect = template.transform as RectTransform;
        templateRect.anchorMin = new Vector2(0f, 0f);
        templateRect.anchorMax = new Vector2(1f, 0f);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.anchoredPosition = new Vector2(0f, 2f);
        templateRect.sizeDelta = new Vector2(0f, 150f);
        template.SetActive(false);

        var viewport = Child("Viewport", template.transform, typeof(Image), typeof(Mask));
        Stretch(viewport.transform as RectTransform);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        var content = Child("Content", viewport.transform, typeof(RectTransform));
        var contentRect = content.transform as RectTransform;
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 28f);

        var item = Child("Item", content.transform, typeof(Toggle));
        Size(item, 260f, 28f);
        var itemBackground = Child("Item Background", item.transform, typeof(Image));
        Stretch(itemBackground.transform as RectTransform);
        var itemCheckmark = Child("Item Checkmark", item.transform, typeof(Image));
        Size(itemCheckmark, 20f, 20f);
        var itemLabel = Child("Item Label", item.transform, typeof(TextMeshProUGUI));
        Stretch(itemLabel.transform as RectTransform);
        var itemText = itemLabel.GetComponent<TextMeshProUGUI>();
        itemText.text = "Option";
        itemText.alignment = TextAlignmentOptions.MidlineLeft;
        itemText.raycastTarget = false;

        var toggle = item.GetComponent<Toggle>();
        toggle.targetGraphic = itemBackground.GetComponent<Image>();
        toggle.graphic = itemCheckmark.GetComponent<Image>();

        var scrollRect = template.GetComponent<ScrollRect>();
        scrollRect.viewport = viewport.transform as RectTransform;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;

        var dropdown = parent.GetComponent<TMP_Dropdown>();
        dropdown.itemText = itemText;
        dropdown.itemImage = null;

        return templateRect;
    }

    private static GameObject Child(string name, Transform parent, params Type[] components)
    {
        return UiToolUtility.CreateUiGameObject(name, parent, CombineRect(components));
    }

    private static Type[] CombineRect(Type[] components)
    {
        var types = new List<Type> { typeof(RectTransform), typeof(CanvasRenderer) };
        foreach (var component in components)
        {
            if (!types.Contains(component)) types.Add(component);
        }

        return types.ToArray();
    }

    private static void ApplyRect(JObject parameters, RectTransform rect)
    {
        if (rect == null) return;
        if (ParamUtility.TryReadVector2(parameters, "anchoredPosition", out var position)) rect.anchoredPosition = position;
        if (ParamUtility.TryReadVector2(parameters, "sizeDelta", out var size)) rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Size(GameObject go, float width, float height)
    {
        var rect = go.transform as RectTransform;
        if (rect != null) rect.sizeDelta = new Vector2(width, height);
        var layout = go.GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.preferredWidth = width;
            layout.preferredHeight = height;
        }
    }

    private static string Normalize(string value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant().Replace("-", "_");
    }

    private static bool IsSupported(string value)
    {
        return value == "toggle" || value == "slider" || value == "scroll_view" || value == "input_field" || value == "dropdown";
    }

    private static string DefaultName(string kind)
    {
        if (kind == "scroll_view") return "Scroll View";
        if (kind == "input_field") return "Input Field";
        return string.IsNullOrWhiteSpace(kind) ? "UI Control" : char.ToUpperInvariant(kind[0]) + kind.Substring(1);
    }
}
}
