using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FollowCursor : MonoBehaviour
{
    public RectTransform cursorTransform;

    public Image ui_CursorImage;
    public Image attack_CursorImage;
    public Image attack_Background_CursorImage;

    public RectTransform playerAttack_Cursor;

    public Sprite ui_CursorSprite;
    public Sprite ui_ClickCursorSprite;

    public InputActionReference mousePositionAction;
    public InputActionReference mouseClickAction;

    public Camera cam;

    public GameObject attack_Cursor;
    public GameObject ui_Cursor;

    Transform playerTransform;

    Vector2 mousePos;
    Vector2 screenSize;

    Sprite beforeSprite;
    Sprite currentSprite;
    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        ui_CursorImage.sprite = ui_CursorSprite;

        screenSize = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        Color color = Color.white;
        color.a = 1f;
        color.r = PlayerPrefs.GetFloat("Cursorr", 1f);
        color.g = PlayerPrefs.GetFloat("Cursorg", 1f);
        color.b = PlayerPrefs.GetFloat("Cursorb", 1f);
        ui_CursorImage.color = color;

        color.r = PlayerPrefs.GetFloat("AttackCursorr", 1f);
        color.g = PlayerPrefs.GetFloat("AttackCursorg", 1f);
        color.b = PlayerPrefs.GetFloat("AttackCursorb", 1f);
        attack_CursorImage.color = color;

        color.a = 0.6f;
        color.r = PlayerPrefs.GetFloat("AttackBackgroundCursorr", 1f);
        color.g = PlayerPrefs.GetFloat("AttackBackgroundCursorg", 1f);
        color.b = PlayerPrefs.GetFloat("AttackBackgroundCursorb", 1f);

        attack_Background_CursorImage.color = color;
    }
    private void LateUpdate()
    {
        mousePos = mousePositionAction.action.ReadValue<Vector2>();

        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (!ui_Cursor.activeSelf) ui_Cursor.SetActive(true);
            if (attack_Cursor.activeSelf) attack_Cursor.SetActive(false);
            if (playerAttack_Cursor.gameObject.activeSelf) playerAttack_Cursor.gameObject.SetActive(false);

            if (mouseClickAction.action.IsPressed()) currentSprite = ui_ClickCursorSprite;

            else currentSprite = ui_CursorSprite;

            if (currentSprite != beforeSprite)
            {
                beforeSprite = currentSprite;

                ui_CursorImage.sprite = currentSprite;
            }
        }
        else
        {
            if (ui_Cursor.activeSelf) ui_Cursor.SetActive(false);
            if (!attack_Cursor.activeSelf) attack_Cursor.SetActive(true);
            if (!playerAttack_Cursor.gameObject.activeSelf) playerAttack_Cursor.gameObject.SetActive(true);

            Vector2 playerForwardPosition = cam.WorldToScreenPoint(playerTransform.position + playerTransform.forward);

            Vector2 playerPosition = cam.WorldToScreenPoint(playerTransform.position);

            Vector2 playerAttackCursorDir = (playerForwardPosition - playerPosition).normalized;

            float cursorDistance = (mousePos - playerPosition).magnitude;

            playerAttack_Cursor.localPosition = (playerPosition + (playerAttackCursorDir * cursorDistance)) - screenSize;
        }

        mousePos -= screenSize;

        cursorTransform.localPosition = mousePos;
    }
    private void OnEnable()
    {
        if (!mousePositionAction.action.enabled)
            mousePositionAction.action.Enable();

        if (!mouseClickAction.action.enabled)
            mouseClickAction.action.Enable();
    }
    private void OnDisable()
    {
        if (mousePositionAction.action.enabled)
            mousePositionAction.action.Disable();
        if (mouseClickAction.action.enabled)
            mouseClickAction.action.Disable();
    }
    public void Connect(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }
}
