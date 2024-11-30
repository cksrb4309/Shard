using UnityEngine;

public class AnchorObject : MonoBehaviour
{
    Vector3 initialPosition;
    private void Awake()
    {
        initialPosition = transform.position;
    }
    private void LateUpdate()
    {
        transform.position = initialPosition;
    }
}
