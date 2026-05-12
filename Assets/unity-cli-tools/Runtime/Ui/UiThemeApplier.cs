using UnityEngine;

namespace UnityCliTools.Runtime.Ui
{

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class UiThemeApplier : MonoBehaviour
{
    [SerializeField] private UiTheme theme;
    [SerializeField] private bool includeInactive = true;
    [SerializeField] private bool applyOnEnable = true;

    public UiTheme Theme
    {
        get { return theme; }
        set { theme = value; }
    }

    public bool IncludeInactive
    {
        get { return includeInactive; }
        set { includeInactive = value; }
    }

    private void OnEnable()
    {
        if (applyOnEnable)
        {
            ApplyTheme();
        }
    }

    private void OnValidate()
    {
        if (applyOnEnable)
        {
            ApplyTheme();
        }
    }

    public int ApplyTheme()
    {
        if (theme == null)
        {
            return 0;
        }

        var bindings = GetComponentsInChildren<UiThemeBinding>(includeInactive);
        for (var i = 0; i < bindings.Length; i++)
        {
            if (bindings[i] != null)
            {
                bindings[i].Apply(theme);
            }
        }

        return bindings.Length;
    }
}
}
