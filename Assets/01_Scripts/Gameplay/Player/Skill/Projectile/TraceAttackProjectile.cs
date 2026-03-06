using UnityEngine;
using System.Collections;

public class TraceAttackProjectile : MonoBehaviour
{
    public string projectileName;

    public bool isNearest = true;

    AttackData attackData = null;

    float criticalChance;
    float criticalDamage;
    float damage;
    float speed;

    Transform myTransform = null;

    IAttackable target;

    bool hasNearbyTarget = true;

    bool isAttack = true;

    Coroutine coroutine = null;

    public void SetAttackProjectile(AttackData data, float damage, float speed, float destroyDelay, Vector3 pos)
    {
        myTransform = transform;

        if (isNearest) target = AttackableTargetSelector.GetRandomAliveTargetNearSelf();

        else target = AttackableTargetSelector.GetFirstAliveTargetInExpandingRings(data.Position);

        if (target == null) 
        {
            PoolingManager.Instance.ReturnObject(projectileName, gameObject);
        }
        else
        {
            Vector3 dir = (target.GetPosition() - myTransform.position).normalized;

            myTransform.forward = dir;

            hasNearbyTarget = true;

            this.attackData = data;
            this.speed = speed;
            this.damage = damage;

            attackData.Value = damage;

            myTransform.position = pos;

            isAttack = true;

            coroutine = StartCoroutine(WaitReturnCoroutine(destroyDelay));
        }
    }
    IEnumerator WaitReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        Return();
    }
    private void FixedUpdate()
    {
        if (hasNearbyTarget && !target.IsAlive())
        {
            if (isNearest)
                target = AttackableTargetSelector.GetRandomAliveTargetNearSelf();
            else
                target = AttackableTargetSelector.GetFirstAliveTargetInExpandingRings(attackData.Position);

            if (target == null)
                hasNearbyTarget = false;
        }

        Vector3 dir = myTransform.forward;

        if (target != null)
        {
            dir = (target.GetPosition() - myTransform.position).normalized;

            myTransform.forward = dir;
        }

        myTransform.position += dir * speed * Time.fixedDeltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isAttack) return;

        isAttack = false;

        // Physics Layer 설정을 통해 공격 할 수 있는 것만 충돌함
        IAttackable attackable = other.GetComponent<IAttackable>();

        if (!attackable.IsAlive()) return;

        attackData.OnHit(attackable, null, false);

        Return();
    }

    private void Return()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
        AttackData.ReleaseAttackData(attackData);
    }
}
