using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform lookAtTransform;

    public void LookAt()
    {
        transform.LookAt(lookAtTransform);
    }
}
