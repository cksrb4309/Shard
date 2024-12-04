using System.Collections;
using UnityEngine;

public class DefaultMonsterMove : MonsterMove
{
    public float moveSpeed = 3f;        // �̵� �ӵ�
    public float rotationSpeed = 360f;  // ȸ�� �ӵ�

    Transform myTransform;

    Vector3 moveDir = Vector3.zero;
    private void Awake()
    {
        myTransform = transform;
    }
    public override void SetDir(Vector3 dir)
    {
        moveDir = dir;
    }
    public override void StartMove()
    {
        StartCoroutine(MoveCoroutine());
    }

    public override void StopMove()
    {
        StopAllCoroutines();
    }
    IEnumerator MoveCoroutine()
    {
        while (moveDir == Vector3.zero) yield return null;

        while (true)
        {
            // rb.MovePosition(myTransform.position + moveDir * moveSpeed * Time.fixedDeltaTime);

            transform.position += moveDir * moveSpeed * Time.deltaTime;

            // ���� �ٶ󺸴� ������ ���������� �÷��̾� �������� ȸ��
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);

            myTransform.rotation = Quaternion.RotateTowards(myTransform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            yield return null;
        }
    }
}
