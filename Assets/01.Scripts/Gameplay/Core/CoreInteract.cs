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

        if (isShow) // ���� �����ְ� ������
        {
            isShow = false;

            // �÷��̾ ��ó���� �������� ������
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
        if (inPlayer == false) return; // ��ó�� ���ٸ� false

        if (isShow) // ���� �����ִ� ���̶�� ������
        {
            HidePanel();
        }
        else // UI�� ������ �ʴٸ� �Ѿ���
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
