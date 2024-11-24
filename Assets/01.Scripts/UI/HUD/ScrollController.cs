using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour
{
    public ScrollRect scrollRect;

    public float scrollSpeed = 0.1f;

    public InputActionReference scrollAction;

    public RectTransform viewPortTransform;
    public RectTransform contentTransform;

    private void OnEnable()
    {
        scrollAction.action.Enable();
    }

    private void OnDisable()
    {
        scrollAction.action.Disable();
    }

    private void Start()
    {
        Debug.Log("---------------------------");

        Debug.Log("Content Height : " + contentTransform.rect.height.ToString());


        Debug.Log("ViewPort Height : " + viewPortTransform.rect.height.ToString());
        Debug.Log("---------------------------");

    }
    private void Update()
    {

    }
}