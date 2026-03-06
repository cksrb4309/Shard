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

    Sprite currentCursorSprite;
    Sprite beforeCursorSprite;

    Vector2 mousePos;
    Vector2 screenSize;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        currentCursorSprite = uiCursor;
        beforeCursorSprite = uiCursor;
        cursorImage.sprite = uiCursor;

        screenSize = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);


        Color color = Color.white;
        color.a = 1f;
        color.r = PlayerPrefs.GetFloat("Cursorr", 1f);
        color.g = PlayerPrefs.GetFloat("Cursorg", 1f);
        color.b = PlayerPrefs.GetFloat("Cursorb", 1f);
        cursorImage.color = color;
    }
    private void LateUpdate()
    {
        mousePos = mousePositionAction.action.ReadValue<Vector2>();

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

        mousePos -= screenSize;

        cursorTransform.localPosition = mousePos;
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
}
