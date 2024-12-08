using UnityEngine;

public class RotateProjectile : MonoBehaviour
{
    public float rotateSpeed = 1f;

    private void FixedUpdate()
    {
        // ���� Z�� �������� ȸ��
        transform.Rotate(0, 0, rotateSpeed * Time.fixedDeltaTime, Space.Self);
    }
}
