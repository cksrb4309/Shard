using UnityEngine;
using UnityEngine.UI;

namespace UnityCliTools.Runtime.Ui
{

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class ResponsiveRectTransform : MonoBehaviour
{
    [SerializeField] private ResponsiveRectMode mode = ResponsiveRectMode.CenteredMaxSize;
    [SerializeField] private Vector4 margins;
    [SerializeField] private Vector2 minSize;
    [SerializeField] private Vector2 maxSize = new Vector2(1200f, 900f);
    [SerializeField] private float aspectRatio = 1.7777778f;

    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private Vector2 lastParentSize;
    private Vector2 lastScreenSize;

    public ResponsiveRectMode Mode
    {
        get { return mode; }
        set { mode = value; Apply(); }
    }

    public Vector4 Margins
    {
        get { return margins; }
        set { margins = value; Apply(); }
    }

    public Vector2 MinSize
    {
        get { return minSize; }
        set { minSize = value; Apply(); }
    }

    public Vector2 MaxSize
    {
        get { return maxSize; }
        set { maxSize = value; Apply(); }
    }

    public float AspectRatio
    {
        get { return aspectRatio; }
        set { aspectRatio = Mathf.Max(0.01f, value); Apply(); }
    }

    private void Awake()
    {
        CacheRect();
    }

    private void OnEnable()
    {
        CacheRect();
        Apply();
    }

    private void Update()
    {
        if (NeedsUpdate())
        {
            Apply();
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        Apply();
    }

    private void OnValidate()
    {
        aspectRatio = Mathf.Max(0.01f, aspectRatio);
        Apply();
    }

    public void Apply()
    {
        CacheRect();
        if (rectTransform == null)
        {
            return;
        }

        if (mode == ResponsiveRectMode.StretchToSafeArea)
        {
            ApplySafeArea();
        }
        else if (mode == ResponsiveRectMode.StretchToParent)
        {
            ApplyStretch(Vector2.zero, Vector2.one);
        }
        else if (mode == ResponsiveRectMode.FitAspectInsideParent)
        {
            ApplyCentered(true);
        }
        else
        {
            ApplyCentered(false);
        }

        TrackState();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    private void ApplySafeArea()
    {
        var safeArea = Screen.safeArea;
        var screenWidth = Mathf.Max(1f, Screen.width);
        var screenHeight = Mathf.Max(1f, Screen.height);
        var anchorMin = new Vector2(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight);
        var anchorMax = new Vector2(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight);
        ApplyStretch(anchorMin, anchorMax);
    }

    private void ApplyStretch(Vector2 anchorMin, Vector2 anchorMax)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = new Vector2(margins.x, margins.w);
        rectTransform.offsetMax = new Vector2(-margins.z, -margins.y);
        rectTransform.localScale = Vector3.one;
    }

    private void ApplyCentered(bool fitAspect)
    {
        var parentSize = GetParentSize();
        var availableWidth = Mathf.Max(0f, parentSize.x - margins.x - margins.z);
        var availableHeight = Mathf.Max(0f, parentSize.y - margins.y - margins.w);
        var size = new Vector2(availableWidth, availableHeight);

        if (fitAspect)
        {
            size = FitAspect(size);
        }

        size.x = ClampSize(size.x, minSize.x, maxSize.x);
        size.y = ClampSize(size.y, minSize.y, maxSize.y);
        size.x = Mathf.Min(size.x, availableWidth);
        size.y = Mathf.Min(size.y, availableHeight);

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2((margins.x - margins.z) * 0.5f, (margins.w - margins.y) * 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.localScale = Vector3.one;
    }

    private Vector2 FitAspect(Vector2 available)
    {
        if (available.x <= 0f || available.y <= 0f)
        {
            return Vector2.zero;
        }

        var width = available.x;
        var height = width / aspectRatio;
        if (height > available.y)
        {
            height = available.y;
            width = height * aspectRatio;
        }

        return new Vector2(width, height);
    }

    private static float ClampSize(float value, float min, float max)
    {
        var resolvedMin = Mathf.Max(0f, min);
        if (max <= 0f)
        {
            return Mathf.Max(value, resolvedMin);
        }

        var resolvedMax = Mathf.Max(resolvedMin, max);
        return Mathf.Clamp(value, resolvedMin, resolvedMax);
    }

    private Vector2 GetParentSize()
    {
        var parent = rectTransform.parent as RectTransform;
        if (parent != null)
        {
            return parent.rect.size;
        }

        return new Vector2(Screen.width, Screen.height);
    }

    private bool NeedsUpdate()
    {
        CacheRect();
        var parentSize = GetParentSize();
        var screenSize = new Vector2(Screen.width, Screen.height);
        return lastSafeArea != Screen.safeArea
            || lastParentSize != parentSize
            || lastScreenSize != screenSize;
    }

    private void TrackState()
    {
        lastSafeArea = Screen.safeArea;
        lastParentSize = GetParentSize();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    private void CacheRect()
    {
        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }
    }
}
}
