using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DefaultMonster : Monster
{
    public float moveSpeed = 3f;
    public float updateInterval; // 목표 위치 갱신 주기
    public float rotationSpeed = 360f; // 회전 속도

    public Transform myTransform;

    Vector3 moveDir = Vector3.zero;
    Transform target = null;
    public override void Setting(float monsterHpMultiplier, float damageMultiplier)
    {
        base.Setting(monsterHpMultiplier, damageMultiplier);

        target = GameManager.GetMonsterTargetTransform(transform.position);

        StartCoroutine(TrackingCoroutine());
    }
    public void FixedUpdate()
    {
        transform.position += moveDir * moveSpeed * Time.fixedDeltaTime;
        //rb.MovePosition(myTransform.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        // 현재 바라보는 방향을 점진적으로 플레이어 방향으로 회전
        Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
        myTransform.rotation = Quaternion.RotateTowards(myTransform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }
    IEnumerator TrackingCoroutine()
    {
        yield return new WaitForSeconds(Random.value);

        while (true)
        {
            moveDir = (target.position - myTransform.position).normalized;

            yield return new WaitForSeconds(updateInterval);
        }
    }
}