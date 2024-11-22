using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour
{
    public ScrollRect scrollRect;

    public float scrollSpeed = 0.1f;

    private InputActionReference scrollAction;

    private void OnEnable()
    {
        // TODO 여기부터 시작
        scrollAction.action.Enable();
    }
}
