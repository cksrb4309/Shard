using UnityEngine;
using UnityEngine.Events;
public class EventExcute : MonoBehaviour
{
    public UnityEvent unityEvent;
    public void Excute()
    {
        unityEvent.Invoke();
    }
}
