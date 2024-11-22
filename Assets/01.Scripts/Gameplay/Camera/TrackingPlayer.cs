using UnityEngine;

public class TrackingPlayer : MonoBehaviour
{
    public Transform target;

    Vector2 max = new Vector2(50.1f, 20.3f);
    Vector2 min = new Vector2(-50.8f, -30.4f);

    Vector3 offset;
    Vector3 targetPos;

    private void Start()
    {
        transform.parent = null;

        offset = transform.position - target.position;
    }

    private void FixedUpdate()
    {
        targetPos = target.position + offset;

        targetPos.x = Mathf.Clamp(targetPos.x, min.x, max.x);
        targetPos.z = Mathf.Clamp(targetPos.z, min.y, max.y);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.fixedDeltaTime * 3f);
    }
}
