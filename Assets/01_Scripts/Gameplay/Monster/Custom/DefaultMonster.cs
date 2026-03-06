using System.Collections;
using UnityEngine;

public class DefaultMonster : Monster
{
    public float moveSpeed = 3f;
    public float updateInterval; // 목표 위치 갱신 주기
    public float rotationSpeed = 360f; // 회전 속도

    public Transform myTransform;

    public SearchTarget searchTarget;

    Vector3 moveDir = Vector3.zero;

    Transform target = null;

    Animator animator = null;

    public override void Setting(float monsterHpMultiplier, float damageMultiplier)
    {
        base.Setting(monsterHpMultiplier, damageMultiplier);

        if (animator == null) animator = GetComponentInChildren<Animator>();

        animator.SetTrigger("Idle");

        target = GameManager.GetCoreTransform();

        moveDir = (target.position - myTransform.position).normalized;

        transform.rotation = Quaternion.LookRotation(moveDir);

        StartCoroutine(TrackingCoroutine());
        StartCoroutine(MoveCoroutine());
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
    IEnumerator MoveCoroutine()
    {
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
    public void SetTarget(Transform target) => this.target = target;
    public override void ReceiveHit(float damage)
    {
        if (!IsAlive()) return;

        base.ReceiveHit(damage);

        searchTarget.Search(15f);
    }
    public override void Dead()
    {
        base.Dead();
        
        StopAllCoroutines();

        animator.SetTrigger("Die");
    }

    public void ReturnMonster()
    {
        PoolingManager.Instance.ReturnObject(mobName, gameObject);
    }
}