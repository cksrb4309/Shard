using System.Collections;
using UnityEngine;

public class InterstellarRunner_Disk : MonoBehaviour
{
    public string projectileName;

    public HitEffectName hitEffectName;

    AttackData attackData = null;

    float criticalChance;
    float criticalDamage;
    float damage;
    float speed;

    int attackCount;

    IAttackable target = null; // 현재 쫓아가는 타겟
    public void SetAttackProjectile(AttackData attackData, float damage, float speed, float criticalChance, float criticalDamage, float destroyDelay, int attackCount, Vector3 searchPosition, Vector3 pos, Quaternion rotation)
    {
        this.speed = speed;
        this.attackData = attackData;
        this.damage = damage;
        this.criticalChance = criticalChance;
        this.criticalDamage = criticalDamage;
        this.attackCount = attackCount;

        transform.position = pos;
        transform.rotation = rotation;

        StartCoroutine(TrackingCoroutine(searchPosition));

        StartCoroutine(WaitReturnCoroutine(destroyDelay));
    }
    IEnumerator WaitReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
    IEnumerator TrackingCoroutine(Vector3 position)
    {
        // 현재 마우스 커서와 가장 가까운 놈을 고름
        target = NearestAttackableSelector.GetAttackable(position);

        if (target == null) yield break;

        while (true)
        {
            yield return MoveCoroutine();

            if (target == null) // 타겟이 죽어서 쫓아가다 말았으면 다시 타겟을 찾아야함
                target = NearestAttackableSelector.GetAttackable(transform.position);
            else // 만약 타겟이 살아있다면 해당 타겟을 제외하고 찾아야함
                target = NearestAttackableSelector.GetAttackable(transform.position, 50f, target);
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

        float damage = this.damage;

        if (LuckManager.Calculate(criticalChance, true))
        {
            damage *= criticalDamage;

            attackData.OnCritical();

            attackable.ReceiveHit(damage, true);

            attackData.OnHit(damage, transform.position, transform.rotation.eulerAngles);

            ParticleManager.Play(transform.position, hitEffectName);

            if (!attackable.IsAlive())
            {
                attackData.OnKill();
            }
        }
        else
        {
            attackable.ReceiveHit(damage);

            attackData.OnHit(damage, transform.position, transform.rotation.eulerAngles);

            ParticleManager.Play(transform.position, hitEffectName);

            if (!attackable.IsAlive())
            {
                attackData.OnKill();
            }
        }
    }
}
