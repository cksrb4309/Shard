using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
public class TraceAttackProjectile : MonoBehaviour
{
    public string projectileName;

    AttackData attackData = null;

    float criticalChance;
    float criticalDamage;
    float damage;
    float speed;

    IAttackable target;

    bool hasNearbyTarget = true;

    bool isAttack = false;

    public void SetAttackProjectile(AttackData attackData, float damage, float speed, float criticalChance, float criticalDamage, float destroyDelay, Vector3 pos)
    {
        target = NearestAttackableSelector.GetAttackable();

        if (target == null) 
        {
            PoolingManager.Instance.ReturnObject(projectileName, gameObject);
        }
        else
        {
            Vector3 dir = (target.GetPosition() - transform.position).normalized;

            transform.forward = dir;

            hasNearbyTarget = true;

            this.attackData = attackData;
            this.speed = speed;
            this.damage = damage;
            this.criticalChance = criticalChance;
            this.criticalDamage = criticalDamage;

            transform.position = pos;

            isAttack = false;

            StartCoroutine(WaitReturnCoroutine(destroyDelay));
        }
    }
    IEnumerator WaitReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
    private void FixedUpdate()
    {
        if (!hasNearbyTarget) return;
        if (!target.IsAlive()) target = NearestAttackableSelector.GetAttackable();
        if (target == null) { hasNearbyTarget = false; return; }
        Vector3 dir = (target.GetPosition() - transform.position).normalized;

        transform.position += dir * speed * Time.fixedDeltaTime;
        transform.forward = dir;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isAttack) return;
        isAttack = true;

        float damage = this.damage;

        // Physics Layer 설정을 통해 공격 할 수 있는 것만 충돌함
        IAttackable attackable = other.GetComponent<IAttackable>();

        if (!attackable.IsAlive()) return;

        if (criticalChance >= Random.value)
        {
            damage *= criticalDamage;

            attackData.OnCritical();
        }

        attackable.ReceiveHit(damage);

        attackData.OnHit(damage, transform.position, transform.rotation.eulerAngles);

        if (!attackable.IsAlive())
        {
            attackData.OnKill();
        }
        
        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
}
