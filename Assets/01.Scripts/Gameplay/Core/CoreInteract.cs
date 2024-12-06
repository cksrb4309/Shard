using UnityEngine;
using UnityEngine.InputSystem;

public class CoreInteract : MonoBehaviour
{
    public InputActionReference interactAction;
    public Outline outline;
    public CoreInteractUI panel;
    bool inPlayer = false;
    bool isShow = false;
    private void OnTriggerEnter(Collider other)
    {
        inPlayer = true;

        outline.enabled = true;
    }
    private void OnTriggerExit(Collider other)
    {
        inPlayer = false;

        outline.enabled = false;

        if (isShow) // 만약 보여주고 있으면
        {
            isShow = false;

            // 플레이어가 근처에서 나갔으니 꺼야함
            panel.HidePanel();
        }
    }
    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.started += SetPanel;
    }
    private void OnDisable()
    {
        interactAction.action.started -= SetPanel;
        interactAction.action.Disable();
    }
    private void SetPanel(InputAction.CallbackContext context)
    {
        if (inPlayer == false) return; // 근처에 없다면 false

        if (isShow) // 현재 보여주는 중이라면 꺼야함
        {
            HidePanel();
        }
        else // UI가 떠있지 않다면 켜야함
        {
            ShowPanel();
        }
    }
    private void ShowPanel()
    {
        isShow = true;

        panel.ShowPanel();
    }
    private void HidePanel()
    {
        isShow = false;

        panel.HidePanel();
    }
}
