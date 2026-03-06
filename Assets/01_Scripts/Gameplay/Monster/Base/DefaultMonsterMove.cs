using System.Collections;
using UnityEngine;

public class DefaultMonsterMove : MonsterMove
{
    public float moveSpeed = 3f;        // 이동 속도
    public float rotationSpeed = 360f;  // 회전 속도

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

            // 현재 바라보는 방향을 점진적으로 플레이어 방향으로 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);

            myTransform.rotation = Quaternion.RotateTowards(myTransform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            yield return null;
        }
    }
}
