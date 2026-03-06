using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterstellarRunner_Disk : MonoBehaviour
{
    public string projectileName;

    public HitEffectName hitEffectName;

    AttackData attackData = null;

    float speed;

    int attackCount;

    IAttackable target = null; // 현재 쫓아가는 타겟
    public void SetAttackProjectile(AttackData attackData, float damage, float speed, float destroyDelay, int attackCount, Vector3 searchPosition, Vector3 pos, Quaternion rotation)
    {
        this.speed = speed;
        this.attackData = attackData;
        this.attackCount = attackCount;

        transform.position = pos;
        transform.rotation = rotation;

        attackData.Value = damage;

        StartCoroutine(TrackingCoroutine(searchPosition));

        StartCoroutine(WaitReturnCoroutine(destroyDelay));
    }
    IEnumerator WaitReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        AttackData.ReleaseAttackData(attackData);

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
    IEnumerator TrackingCoroutine(Vector3 position)
    {
        // 현재 마우스 커서와 가장 가까운 놈을 고름
        target = AttackableTargetSelector.GetFirstAliveTargetInExpandingRings(position);

        if (target == null) yield break;

        while (true)
        {
            yield return MoveCoroutine();

            if (target == null) // 타겟이 죽어서 쫓아가다 말았으면 다시 타겟을 찾아야함
                target = AttackableTargetSelector.GetFirstAliveTargetInExpandingRings(transform.position);

            else // 만약 타겟이 살아있다면 해당 타겟을 제외하고 찾아야함
            {
                HashSet<IAttackable> ignoredTargets = new HashSet<IAttackable> { target };

                target = AttackableTargetSelector.GetRandomAliveTargetInRadiusExcludingSet(transform.position, 50f, ignoredTargets);
            }
        }
    }
    IEnumerator MoveCoroutine()
    {
        Vector3 distance = Vector3.zero;

        while (true)
        {
            distance = (target.GetPosition() - transform.position);

            if (distance.magnitude < 0.3f) // 닿았다고 판정
            {
                attackCount--; // 공격 횟수 1 차감

                if (attackCount == 0)
                {
                    PoolingManager.Instance.ReturnObject(projectileName, gameObject);
                }

                yield break;
            }
            transform.position += distance.normalized * Time.deltaTime * speed; // 이동

            yield return null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // Physics Layer 설정을 통해 공격 할 수 있는 것만 충돌함
        IAttackable attackable = other.GetComponent<IAttackable>();

        if (!attackable.IsAlive()) return;

        attackData.OnHit(attackable, null, false);

        ParticleManager.Play(transform.position, hitEffectName);
    }
}
