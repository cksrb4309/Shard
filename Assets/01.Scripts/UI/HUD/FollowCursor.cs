using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FollowCursor : MonoBehaviour
{

    public RectTransform cursorTransform;
    public Image cursorImage;

    public Sprite attackCursor;

    public RectTransform playerAttackCursor;

    public Sprite uiCursor;
    public Sprite uiClickCursor;

    public InputActionReference mousePositionAction;
    public InputActionReference mouseClickAction;

    public Camera cam;

    Transform playerTransform;

    Sprite currentCursorSprite;
    Sprite beforeCursorSprite;

    Vector2 mousePos;
    Vector2 screenSize;
    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        currentCursorSprite = attackCursor;
        beforeCursorSprite = attackCursor;

        cursorImage.sprite = attackCursor;

        screenSize = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }
    private void LateUpdate()
    {
        mousePos = mousePositionAction.action.ReadValue<Vector2>();

        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (mouseClickAction.action.IsPressed())
            {
                currentCursorSprite = uiClickCursor;
            }
            else
            {
                currentCursorSprite = uiCursor;
            }
            if (playerAttackCursor.gameObject.activeSelf) playerAttackCursor.gameObject.SetActive(false);
        }
        else
        {
            currentCursorSprite = attackCursor;
            
            if (!playerAttackCursor.gameObject.activeSelf) playerAttackCursor.gameObject.SetActive(true);


            Vector2 playerForwardPosition = cam.WorldToScreenPoint(playerTransform.position + playerTransform.forward);

            Vector2 playerPosition = cam.WorldToScreenPoint(playerTransform.position);

            Vector2 playerAttackCursorDir = (playerForwardPosition - playerPosition).normalized;

            float cursorDistance = (mousePos - playerPosition).magnitude;

            playerAttackCursor.localPosition = (playerPosition + (playerAttackCursorDir * cursorDistance)) - screenSize;
        }


        mousePos -= screenSize;

        cursorTransform.localPosition = mousePos;

        if (currentCursorSprite != beforeCursorSprite)
        {
            beforeCursorSprite = currentCursorSprite;

            cursorImage.sprite = currentCursorSprite;
        }
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
    public void Connect(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }
}
