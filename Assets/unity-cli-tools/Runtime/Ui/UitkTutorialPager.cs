using UnityEngine;
using UnityEngine.UIElements;

namespace UnityCliTools.Runtime.Ui
{

[DisallowMultipleComponent]
public sealed class UitkTutorialPager : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private string rootElementName = "tutorial_root";
    [SerializeField] private string previousButtonName = "previous_button";
    [SerializeField] private string nextButtonName = "next_button";
    [SerializeField] private string[] pageElementNames;
    [SerializeField] private string previousText = "<";
    [SerializeField] private string nextText = ">";
    [SerializeField] private string closeText = "X";
    [SerializeField] private bool hideRootOnClose = true;

    private Button previousButton;
    private Button nextButton;
    private int pageIndex;

    private void OnEnable()
    {
        Bind();
        Refresh();
    }

    private void OnDisable()
    {
        if (previousButton != null)
        {
            previousButton.clicked -= Previous;
        }

        if (nextButton != null)
        {
            nextButton.clicked -= NextOrClose;
        }
    }

    public void Bind()
    {
        var resolvedDocument = document != null ? document : GetComponent<UIDocument>();
        if (resolvedDocument == null || resolvedDocument.rootVisualElement == null)
        {
            return;
        }

        previousButton = resolvedDocument.rootVisualElement.Q<Button>(previousButtonName);
        nextButton = resolvedDocument.rootVisualElement.Q<Button>(nextButtonName);

        if (previousButton != null)
        {
            previousButton.clicked -= Previous;
            previousButton.clicked += Previous;
        }

        if (nextButton != null)
        {
            nextButton.clicked -= NextOrClose;
            nextButton.clicked += NextOrClose;
        }
    }

    public void Previous()
    {
        pageIndex = Mathf.Max(0, pageIndex - 1);
        Refresh();
    }

    public void NextOrClose()
    {
        var count = PageCount;
        if (count == 0 || pageIndex >= count - 1)
        {
            Close();
            return;
        }

        pageIndex++;
        Refresh();
    }

    public void Close()
    {
        if (!hideRootOnClose)
        {
            gameObject.SetActive(false);
            return;
        }

        var resolvedDocument = document != null ? document : GetComponent<UIDocument>();
        var root = resolvedDocument != null && resolvedDocument.rootVisualElement != null
            ? resolvedDocument.rootVisualElement.Q<VisualElement>(rootElementName)
            : null;
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void Refresh()
    {
        var resolvedDocument = document != null ? document : GetComponent<UIDocument>();
        if (resolvedDocument == null || resolvedDocument.rootVisualElement == null)
        {
            return;
        }

        var count = PageCount;
        pageIndex = Mathf.Clamp(pageIndex, 0, Mathf.Max(0, count - 1));

        for (var i = 0; i < count; i++)
        {
            var page = resolvedDocument.rootVisualElement.Q<VisualElement>(pageElementNames[i]);
            if (page != null)
            {
                page.style.display = i == pageIndex ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        if (previousButton != null)
        {
            previousButton.text = previousText;
            previousButton.SetEnabled(pageIndex > 0);
        }

        if (nextButton != null)
        {
            nextButton.text = count > 0 && pageIndex >= count - 1 ? closeText : nextText;
        }
    }

    private int PageCount
    {
        get { return pageElementNames == null ? 0 : pageElementNames.Length; }
    }
}
}
