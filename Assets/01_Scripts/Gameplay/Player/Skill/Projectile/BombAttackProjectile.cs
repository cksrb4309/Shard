using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BombAttackProjectile : MonoBehaviour
{
    public string projectileName;
    public Collider cd;

    float rotateSpeed = 30f;

    AttackData attackData = null;

    float speed;
    float range;

    bool isAttack = false;

    private void OnEnable()
    {
        speed = 0;
        rotateSpeed = 0;
        cd.enabled = false;
    }

    public void SetAttackProjectile(AttackData attackData, float damage, float speed, float destroyDelay, float range, Vector3 pos, Quaternion rotation)
    {
        this.speed = speed;
        this.attackData = attackData;
        this.range = range;

        transform.position = pos;
        transform.rotation = rotation;

        attackData.Value = damage;

        cd.enabled = true;
        isAttack = false;
        StartCoroutine(WaitReturnCoroutine(destroyDelay));
    }
    public void SetRotateSpeed(float speed)
    {
        rotateSpeed = speed;
    }
    IEnumerator WaitReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        cd.enabled = false;

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
    private void FixedUpdate()
    {
        transform.position += transform.forward * Time.fixedDeltaTime * speed;

        // 로컬 Z축 기준으로 회전
        transform.Rotate(0, 0, rotateSpeed * Time.fixedDeltaTime, Space.Self);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isAttack) return;
        isAttack = true;

        // Physics Layer 설정을 통해 공격 할 수 있는 것만 충돌함
        IAttackable attackable_1 = other.GetComponent<IAttackable>();

        attackData.OnHit(attackable_1, null, false);

        List<IAttackable> attackables = new List<IAttackable>();
        int count = AttackableTargetSelector.CollectTargetsInRadiusNonAlloc(transform.position, range, attackables);

        for (int i = 0; i < count; i++)
        
            if (attackables[i].IsAlive())
            
                attackData.OnHit(attackables[i], null, false);

        cd.enabled = false;

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
}
