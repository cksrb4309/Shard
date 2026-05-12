using UnityEngine;
using UnityEngine.UIElements;

namespace UnityCliTools.Runtime.Ui
{

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class UitkStyleSheetBinding : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private StyleSheet styleSheet;
    [SerializeField] private bool applyOnEnable = true;

    private void OnEnable()
    {
        if (applyOnEnable)
        {
            Apply();
        }
    }

    public void Apply()
    {
        var resolvedDocument = document != null ? document : GetComponent<UIDocument>();
        if (resolvedDocument == null || styleSheet == null)
        {
            return;
        }

        var root = resolvedDocument.rootVisualElement;
        if (root == null || root.styleSheets.Contains(styleSheet))
        {
            return;
        }

        root.styleSheets.Add(styleSheet);
    }
}
}
