using UnityEngine;

public class RotateProjectile : MonoBehaviour
{
    public float rotateSpeed = 1f;

    private void FixedUpdate()
    {
        // 로컬 Z축 기준으로 회전
        transform.Rotate(0, 0, rotateSpeed * Time.fixedDeltaTime, Space.Self);
    }
}
