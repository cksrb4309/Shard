using UnityEngine;

public class TrackingPlayer : MonoBehaviour
{
    Transform target = null;

    Vector2 max = new Vector2(50.1f, 13.73f);
    Vector2 min = new Vector2(-50.8f, -37.1f);

    Vector3 offset = new Vector3(0, 19.11f, -11.59f);
    Vector3 targetPos;

    private void FixedUpdate()
    {
        if (target != null)
        {
            targetPos = target.position + offset;

            targetPos.x = Mathf.Clamp(targetPos.x, min.x, max.x);
            targetPos.z = Mathf.Clamp(targetPos.z, min.y, max.y);

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.fixedDeltaTime * 3f);
        }
    }
    public void Connect(Transform playerTransform)
    {
        target = playerTransform;
    }
}
