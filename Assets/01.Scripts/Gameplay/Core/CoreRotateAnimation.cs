using UnityEngine;

public class CoreRotateAnimation : MonoBehaviour
{
    public float ratateSpeed = 5f;

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.forward * ratateSpeed * Time.fixedDeltaTime);
    }
}
