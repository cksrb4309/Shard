using UnityEngine;
using UnityEngine.Events;
public class EventExcute : MonoBehaviour
{
    public UnityEvent unityEvent;
    public UnityEvent[] unityEvents;
    public void Excute()
    {
        unityEvent.Invoke();
    }
    public void CustomExcute(int excuteIndex)
    {
        unityEvents[excuteIndex].Invoke();
    }
}
