using UnityEngine;

public class CoreInteract : MonoBehaviour
{
    public CoreInteractUI panel;
    private void OnTriggerEnter(Collider other)
    {
        panel.ShowPanel();
    }
    private void OnTriggerExit(Collider other)
    {
        panel.HidePanel();
    }
}
