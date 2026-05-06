using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class TitleCursor : MonoBehaviour
{
    public RectTransform cursorTransform;
    public Image cursorImage;

    public Sprite uiCursor;
    public Sprite uiClickCursor;

    public InputActionReference mousePositionAction;
    public InputActionReference mouseClickAction;

    [SerializeField] private Canvas cursorCanvas;

    Sprite currentCursorSprite;
    Sprite beforeCursorSprite;

    Vector2 mousePos;
    RectTransform rootCanvasRect;
    Camera uiCamera;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        currentCursorSprite = uiCursor;
        beforeCursorSprite = uiCursor;
        cursorImage.sprite = uiCursor;
        CacheCanvasReferences();


        Color color = Color.white;
        color.a = 1f;
        color.r = PlayerPrefs.GetFloat("Cursorr", 1f);
        color.g = PlayerPrefs.GetFloat("Cursorg", 1f);
        color.b = PlayerPrefs.GetFloat("Cursorb", 1f);
        cursorImage.color = color;
    }
    private void LateUpdate()
    {
        CacheCanvasReferences();

        mousePos = mousePositionAction.action.ReadValue<Vector2>();
        if (!TryScreenToCanvasLocal(mousePos, out Vector2 mouseLocalPos))
            return;

        if (mouseClickAction.action.IsPressed())
        {
            currentCursorSprite = uiClickCursor;
        }
        else
        {
            currentCursorSprite = uiCursor;
        }

        if (currentCursorSprite != beforeCursorSprite)
        {
            beforeCursorSprite = currentCursorSprite;
            cursorImage.sprite = beforeCursorSprite;
        }

        cursorTransform.anchoredPosition = mouseLocalPos;
    }
    private void OnEnable()
    {
        if (!mousePositionAction.action.enabled)
            mousePositionAction.action.Enable();

        mouseClickAction.action.Enable();
    }
    private void OnDisable()
    {
        if (mousePositionAction.action.enabled)
            mousePositionAction.action.Disable();

        mouseClickAction.action.Disable();
    }
    public void SetCursorColor(Color cursorColor)
    {
        cursorImage.color = cursorColor;
    }

    private void CacheCanvasReferences()
    {
        if (cursorCanvas == null)
        {
            cursorCanvas = (cursorTransform != null)
                ? cursorTransform.GetComponentInParent<Canvas>()
                : GetComponentInParent<Canvas>();
        }

        if (cursorCanvas == null)
            return;

        rootCanvasRect = cursorCanvas.rootCanvas.transform as RectTransform;
        uiCamera = cursorCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : (cursorCanvas.worldCamera != null ? cursorCanvas.worldCamera : Camera.main);
    }

    private bool TryScreenToCanvasLocal(Vector2 screenPos, out Vector2 localPos)
    {
        if (rootCanvasRect == null)
        {
            localPos = default;
            return false;
        }

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvasRect, screenPos, uiCamera, out localPos);
    }
}
