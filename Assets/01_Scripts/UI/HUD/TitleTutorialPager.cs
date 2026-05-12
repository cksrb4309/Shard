using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class TitleTutorialPager : MonoBehaviour
{
    [SerializeField] private Transform pageRoot;
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text previousLabel;
    [SerializeField] private TMP_Text nextLabel;
    [SerializeField] private string previousText = "<";
    [SerializeField] private string nextText = ">";
    [SerializeField] private string closeText = "X";

    private static readonly Color CommonColor = Color.white;
    private static readonly Color RareColor = new(0.38039216f, 0.38039216f, 0.8862745f, 1f);
    private static readonly Color LegendaryColor = new(0.93333334f, 0.29411766f, 0.29411766f, 1f);

    private int pageIndex;
    private float previousTimeScale = 1f;
    private bool pausedByTutorial;

    private void Awake()
    {
        ResolveReferences();
        BindButtons();
    }

    private void OnEnable()
    {
        ResolveReferences();
        PauseGameTime();
        pageIndex = 0;
        Refresh();
    }

    private void OnDisable()
    {
        ResumeGameTime();
    }

    private void OnValidate()
    {
        ResolveReferences();
    }

    public void Previous()
    {
        if (pageIndex <= 0)
        {
            Refresh();
            return;
        }

        pageIndex--;
        Refresh();
    }

    public void NextOrClose()
    {
        if (pages == null || pages.Length == 0 || pageIndex >= pages.Length - 1)
        {
            gameObject.SetActive(false);
            return;
        }

        pageIndex++;
        Refresh();
    }

    private void PauseGameTime()
    {
        if (!Application.isPlaying || pausedByTutorial)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        pausedByTutorial = true;
    }

    private void ResumeGameTime()
    {
        if (!Application.isPlaying || !pausedByTutorial)
        {
            return;
        }

        Time.timeScale = previousTimeScale;
        pausedByTutorial = false;
    }

    public void Refresh()
    {
        ResolveReferences();
        ApplyStaticText();
        ApplyImageSettings();

        var pageCount = pages == null ? 0 : pages.Length;
        if (pageCount > 0)
        {
            pageIndex = Mathf.Clamp(pageIndex, 0, pageCount - 1);
            for (var i = 0; i < pageCount; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(i == pageIndex);
                }
            }
        }

        if (previousButton != null)
        {
            previousButton.interactable = pageIndex > 0;
        }

        SetButtonLabels(previousButton, previousText);
        SetButtonLabels(nextButton, pageCount > 0 && pageIndex >= pageCount - 1 ? closeText : nextText);
    }

    private void ResolveReferences()
    {
        if (pageRoot == null)
        {
            pageRoot = transform.Find("ExplainPanel/PageRoot");
        }

        if (previousButton == null)
        {
            previousButton = transform.Find("PreviousButton")?.GetComponent<Button>();
        }

        if (nextButton == null)
        {
            nextButton = transform.Find("NextButton")?.GetComponent<Button>();
        }

        if (previousLabel == null)
        {
            previousLabel = previousButton != null
                ? previousButton.GetComponentInChildren<TMP_Text>(true)
                : transform.Find("PreviousButton")?.GetComponentInChildren<TMP_Text>(true);
        }

        if (nextLabel == null)
        {
            nextLabel = nextButton != null
                ? nextButton.GetComponentInChildren<TMP_Text>(true)
                : transform.Find("NextButton")?.GetComponentInChildren<TMP_Text>(true);
        }

        if (pageRoot != null)
        {
            pages = pageRoot.Cast<Transform>()
                .Where(child => child.name.StartsWith("Page_", StringComparison.Ordinal))
                .OrderBy(child => child.GetSiblingIndex())
                .Select(child => child.gameObject)
                .ToArray();
        }
    }

    private void BindButtons()
    {
        if (previousButton != null)
        {
            previousButton.onClick.RemoveListener(Previous);
            if (previousButton.onClick.GetPersistentEventCount() == 0)
            {
                previousButton.onClick.AddListener(Previous);
            }
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextOrClose);
            if (nextButton.onClick.GetPersistentEventCount() == 0)
            {
                nextButton.onClick.AddListener(NextOrClose);
            }
        }
    }

    private static void SetButtonLabels(Button button, string value)
    {
        if (button == null)
        {
            return;
        }

        foreach (var label in button.GetComponentsInChildren<TMP_Text>(true))
        {
            label.text = value;
        }
    }

    private void ApplyStaticText()
    {
        SetText("MainText", "\uD29C\uD1A0\uB9AC\uC5BC");

        SetText("ExplainPanel/PageRoot/Page_01_AbilityChoice/AbilitySelectionPreview/LevelUpText", "LEVEL UP");
        SetText("ExplainPanel/PageRoot/Page_01_AbilityChoice/AbilitySelectionPreview/Grid/SelectPanel_1/AbilityName", "\uACF5\uACA9\uB825 \uC99D\uAC00");
        SetText("ExplainPanel/PageRoot/Page_01_AbilityChoice/AbilitySelectionPreview/Grid/SelectPanel_1/TierPanel/TierText", "\uC77C\uBC18");
        SetText("ExplainPanel/PageRoot/Page_01_AbilityChoice/AbilitySelectionPreview/Grid/SelectPanel_2/AbilityName", "\uC801\uC911 \uC2DC \uC5F0\uC1C4 \uBC88\uAC1C \uBC1C\uC0DD");
        SetText("ExplainPanel/PageRoot/Page_01_AbilityChoice/AbilitySelectionPreview/Grid/SelectPanel_2/TierPanel/TierText", "\uD76C\uADC0");
        SetText("ExplainPanel/PageRoot/Page_01_AbilityChoice/AbilitySelectionPreview/Grid/SelectPanel_3/AbilityName", "\uBAA8\uB4E0 \uACF5\uACA9 \uAD11\uC5ED \uB370\uBBF8\uC9C0 \uCD94\uAC00");
        SetText("ExplainPanel/PageRoot/Page_01_AbilityChoice/AbilitySelectionPreview/Grid/SelectPanel_3/TierPanel/TierText", "\uC804\uC124");
        SetText("ExplainPanel/PageRoot/Page_01_AbilityChoice/BodyText",
            "\uB808\uBCA8\uC5C5\uC744 \uD558\uBA74 3\uAC1C\uC758 \uB79C\uB364 \uB2A5\uB825 \uC911 \uD558\uB098\uB97C \uC120\uD0DD\uD569\uB2C8\uB2E4. \uC120\uD0DD\uD55C \uB2A5\uB825\uC740 \uC989\uC2DC \uC801\uC6A9\uB418\uACE0, \uAC19\uC740 \uB2A5\uB825\uC744 \uB2E4\uC2DC \uBFD1\uC73C\uBA74 \uD6A8\uACFC\uAC00 \uB204\uC801\uB429\uB2C8\uB2E4.");

        SetText("ExplainPanel/PageRoot/Page_02_Resources/ResourceDiagram/Resource_1/Name", "\uD30C\uD3B8");
        SetText("ExplainPanel/PageRoot/Page_02_Resources/ResourceDiagram/Resource_1/Flow", "\uBE14\uB85D\uC744 \uBD80\uC218\uBA74 \uC5BB\uACE0\n\uC911\uC559 \uACB0\uC815\uCCB4\uB97C \uAC15\uD654\uD569\uB2C8\uB2E4.");
        SetText("ExplainPanel/PageRoot/Page_02_Resources/ResourceDiagram/Resource_2/Name", "\uC601\uD63C");
        SetText("ExplainPanel/PageRoot/Page_02_Resources/ResourceDiagram/Resource_2/Flow", "\uC801\uC744 \uCC98\uCE58\uD558\uBA74 \uC5BB\uACE0\n\uC804\uD22C \uC131\uC7A5\uC744 \uB3D5\uC2B5\uB2C8\uB2E4.");
        SetText("ExplainPanel/PageRoot/Page_02_Resources/BodyText",
            "\uBE14\uB85D\uC5D0\uC11C\uB294 \uD30C\uD3B8\uC744, \uC801\uC744 \uC4F0\uB7EC\uB728\uB9AC\uBA74 \uC601\uD63C\uC744 \uC5BB\uC2B5\uB2C8\uB2E4.\n\uBE14\uB85D\uC744 \uBD80\uC218\uAC70\uB098 \uC801\uC744 \uCC98\uCE58\uD558\uBA74 \uACBD\uD5D8\uCE58\uB97C \uC5BB\uACE0, \uACBD\uD5D8\uCE58\uAC00 \uCC28\uBA74 \uB808\uBCA8\uC5C5 \uBCF4\uC0C1\uC744 \uC120\uD0DD\uD569\uB2C8\uB2E4.");

        SetText("ExplainPanel/PageRoot/Page_03_StageLoop/StageDiagram/Step_1/Number", "1");
        SetText("ExplainPanel/PageRoot/Page_03_StageLoop/StageDiagram/Step_1/Label", "\uD30C\uD3B8 \uCC44\uAD74");
        SetText("ExplainPanel/PageRoot/Page_03_StageLoop/StageDiagram/Step_2/Number", "2");
        SetText("ExplainPanel/PageRoot/Page_03_StageLoop/StageDiagram/Step_2/Label", "\uBCF4\uC2A4 \uB4F1\uC7A5");
        SetText("ExplainPanel/PageRoot/Page_03_StageLoop/StageDiagram/Step_3/Number", "3");
        SetText("ExplainPanel/PageRoot/Page_03_StageLoop/StageDiagram/Step_3/Label", "\uACB0\uC815\uCCB4 \uBCF5\uC6D0");
        SetText("ExplainPanel/PageRoot/Page_03_StageLoop/BodyText",
            "\uBE14\uB85D \uD30C\uAD34\uAC00 \uB204\uC801\uB418\uBA74 \uBCF4\uC2A4\uAC00 \uB4F1\uC7A5\uD569\uB2C8\uB2E4. \uBCF4\uC2A4\uB97C \uCC98\uCE58\uD558\uBA74 \uC911\uC559 \uACB0\uC815\uCCB4\uC640 \uD544\uB4DC\uAC00 \uBCF5\uC6D0\uB418\uACE0 \uB2E4\uC74C \uC2A4\uD14C\uC774\uC9C0\uB85C \uB118\uC5B4\uAC11\uB2C8\uB2E4. \uB9C8\uC9C0\uB9C9 \uD398\uC774\uC9C0\uC758 X \uBC84\uD2BC\uC73C\uB85C \uD29C\uD1A0\uB9AC\uC5BC\uC744 \uB2EB\uC744 \uC218 \uC788\uC2B5\uB2C8\uB2E4.");

        SetText("ExplainPanel/PageRoot/Page_04_Difficulty/TitleText", "\uB09C\uC774\uB3C4");
        SetText("ExplainPanel/PageRoot/Page_04_Difficulty/BodyText",
            "\uC2DC\uAC04\uC774 \uD750\uB97C\uC218\uB85D \uB09C\uC774\uB3C4 \uAC8C\uC774\uC9C0\uAC00 \uCC28\uC624\uB985\uB2C8\uB2E4. \uAC8C\uC774\uC9C0\uAC00 \uCC28\uBA74 \uB09C\uC774\uB3C4 \uB2E8\uACC4\uAC00 \uC62C\uB77C\uAC00\uACE0, \uC801\uACFC \uBCF4\uC2A4\uAC00 \uC810\uC810 \uB354 \uAC15\uD574\uC9D1\uB2C8\uB2E4.");

        ApplyPageFont();
    }

    private void ApplyImageSettings()
    {
        ConfigureImagesUnder(transform.Find("ExplainPanel/PageRoot/Page_01_AbilityChoice"));
        ConfigureImagesUnder(transform.Find("ExplainPanel/PageRoot/Page_02_Resources"));
        ConfigureImagesUnder(transform.Find("ExplainPanel/PageRoot/Page_03_StageLoop"));
    }

    private void ConfigureImagesUnder(Transform root)
    {
        if (root == null)
        {
            return;
        }

        foreach (var image in root.GetComponentsInChildren<Image>(true))
        {
            ConfigureImage(image);
        }
    }

    private void ApplyPreviewLayout()
    {
        var preview = transform.Find("ExplainPanel/PageRoot/Page_01_AbilityChoice/AbilitySelectionPreview") as RectTransform;
        if (preview == null)
        {
            return;
        }

        Stretch(preview, new Vector2(0.12f, 0.36f), new Vector2(0.88f, 0.91f), Vector2.zero, Vector2.zero);

        var background = preview.GetComponent<Image>();
        if (background != null)
        {
            background.raycastTarget = false;
        }

        var levelUpText = preview.Find("LevelUpText") as RectTransform;
        Stretch(levelUpText, new Vector2(0f, 0.78f), new Vector2(1f, 0.96f), Vector2.zero, Vector2.zero);
        ConfigureText(levelUpText, 38f, 24f, 42f, TextAlignmentOptions.Center, false);

        var grid = preview.Find("Grid") as RectTransform;
        Stretch(grid, new Vector2(0.07f, 0.17f), new Vector2(0.93f, 0.72f), Vector2.zero, Vector2.zero);
        if (grid != null)
        {
            var layout = grid.GetComponent<GridLayoutGroup>();
            if (layout != null)
            {
                layout.padding = new RectOffset(0, 0, 0, 0);
                layout.cellSize = new Vector2(270f, 155f);
                layout.spacing = new Vector2(24f, 0f);
                layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
                layout.startAxis = GridLayoutGroup.Axis.Horizontal;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layout.constraintCount = 3;
            }
        }

        ConfigureSelectPanel(preview, "Grid/SelectPanel_1", CommonColor);
        ConfigureSelectPanel(preview, "Grid/SelectPanel_2", RareColor);
        ConfigureSelectPanel(preview, "Grid/SelectPanel_3", LegendaryColor);

        var bodyText = transform.Find("ExplainPanel/PageRoot/Page_01_AbilityChoice/BodyText") as RectTransform;
        Stretch(bodyText, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.28f), Vector2.zero, Vector2.zero);
        ConfigureText(bodyText, 25f, 18f, 28f, TextAlignmentOptions.Center, true);
    }

    private void ApplyResourceLayout()
    {
        var resourcePage = transform.Find("ExplainPanel/PageRoot/Page_02_Resources") as RectTransform;
        if (resourcePage == null)
        {
            return;
        }

        var diagram = resourcePage.Find("ResourceDiagram") as RectTransform;
        Stretch(diagram, new Vector2(0.08f, 0.44f), new Vector2(0.92f, 0.78f), Vector2.zero, Vector2.zero);
        ConfigureImage(diagram);

        ConfigureResourceCard(resourcePage, "ResourceDiagram/Resource_1");
        ConfigureResourceCard(resourcePage, "ResourceDiagram/Resource_2");

        var bodyText = resourcePage.Find("BodyText") as RectTransform;
        Stretch(bodyText, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.33f), Vector2.zero, Vector2.zero);
        ConfigureText(bodyText, 25f, 18f, 28f, TextAlignmentOptions.Center, true);
    }

    private void ConfigureResourceCard(Transform resourcePage, string relativePath)
    {
        var card = resourcePage.Find(relativePath) as RectTransform;
        if (card == null)
        {
            return;
        }

        ConfigureImage(card);

        var icon = card.Find("IconImage") as RectTransform;
        if (icon != null)
        {
            icon.anchorMin = new Vector2(0f, 1f);
            icon.anchorMax = new Vector2(0f, 1f);
            icon.pivot = new Vector2(0f, 1f);
            icon.anchoredPosition = new Vector2(28f, -26f);
            icon.sizeDelta = new Vector2(70f, 70f);
            icon.localScale = Vector3.one;
            ConfigureImage(icon.GetComponent<Image>());
        }

        var name = card.Find("Name") as RectTransform;
        Stretch(name, new Vector2(0f, 0.56f), new Vector2(1f, 0.95f), new Vector2(112f, 0f), new Vector2(-24f, 0f));
        ConfigureText(name, 25f, 16f, 29f, TextAlignmentOptions.Center, false);

        var flow = card.Find("Flow") as RectTransform;
        Stretch(flow, new Vector2(0.07f, 0.08f), new Vector2(0.93f, 0.54f), Vector2.zero, Vector2.zero);
        ConfigureText(flow, 20f, 14f, 23f, TextAlignmentOptions.Center, true);
    }

    private void ConfigureSelectPanel(Transform preview, string relativePath, Color tierColor)
    {
        var panel = preview.Find(relativePath) as RectTransform;
        if (panel == null)
        {
            return;
        }

        panel.localScale = Vector3.one;
        panel.sizeDelta = new Vector2(270f, 155f);
        var button = panel.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = false;
        }

        var image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = false;
        }

        var tierPanel = panel.Find("TierPanel") as RectTransform;
        Stretch(tierPanel, new Vector2(0.02f, 0.72f), new Vector2(0.45f, 0.98f), Vector2.zero, Vector2.zero);
        var tierImage = tierPanel != null ? tierPanel.GetComponent<Image>() : null;
        if (tierImage != null)
        {
            tierImage.raycastTarget = false;
        }

        var tierText = tierPanel != null ? tierPanel.Find("TierText") as RectTransform : null;
        Stretch(tierText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        ConfigureText(tierText, 21f, 13f, 24f, TextAlignmentOptions.Center, false);
        SetTextColor(tierText, tierColor);

        var abilityName = panel.Find("AbilityName") as RectTransform;
        Stretch(abilityName, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.68f), Vector2.zero, Vector2.zero);
        ConfigureText(abilityName, 34f, 18f, 42f, TextAlignmentOptions.Center, true);
        SetTextColor(abilityName, Color.white);
    }

    private void SetText(string relativePath, string value)
    {
        var target = transform.Find(relativePath);
        if (target == null)
        {
            return;
        }

        var text = target.GetComponent<TMP_Text>();
        if (text != null && text.text != value)
        {
            text.text = value;
        }
    }

    private void ApplyPageFont()
    {
        var sourceFont = transform.Find("MainText")?.GetComponent<TMP_Text>()?.font;
        if (sourceFont == null || pageRoot == null)
        {
            return;
        }

        foreach (var text in pageRoot.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.font != sourceFont)
            {
                text.font = sourceFont;
            }
        }
    }

    private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
    }

    private static void ConfigureText(RectTransform rect, float fontSize, float minSize, float maxSize, TextAlignmentOptions alignment, bool wrap)
    {
        if (rect == null)
        {
            return;
        }

        var text = rect.GetComponent<TMP_Text>();
        if (text == null)
        {
            return;
        }

        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.alignment = alignment;
        text.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
    }

    private static void ConfigureImage(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        ConfigureImage(rect.GetComponent<Image>());
    }

    private static void ConfigureImage(Image image)
    {
        if (image == null)
        {
            return;
        }

        image.raycastTarget = false;
        image.preserveAspect = false;
        if (image.sprite != null && image.sprite.border.sqrMagnitude > 0f)
        {
            image.type = Image.Type.Sliced;
        }
    }

    private static void SetTextColor(RectTransform rect, Color color)
    {
        if (rect == null)
        {
            return;
        }

        var text = rect.GetComponent<TMP_Text>();
        if (text != null)
        {
            text.color = color;
        }
    }
}
