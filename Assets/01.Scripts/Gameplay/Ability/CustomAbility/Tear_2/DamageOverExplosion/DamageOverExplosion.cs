using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageOverExplosion", menuName = "Ability/Tear2/DamageOverExplosion")]
public class DamageOverExplosion : Ability, IOnHitDamage
{
    public float cooltime;
    public float useLimitValue = 4f;

    public float startRange;
    public float stackRange;

    public float startDamage;
    public float stackDamage;

    public LayerMask layerMask;

    float range;
    float damage;

    bool isCharged = true;
    public override void SetCount(int count)
    {
        base.SetCount(count);
        AbilitySet();
    }
    void AbilitySet()
    {
        isCharged = true;

        range = startRange + stackRange * (count - 1);

        damage = startDamage + stackDamage * (count - 1);
    }

    public void OnHit(AttackData attackData, float damage)
    {
        if (!isCharged) return;

        if (damage > PlayerAttributes.Get(Attribute.AttackDamage) * useLimitValue)
        {
            isCharged = false;

            ExplosionEffect explosionEffect = PoolingManager.Instance.GetObject<ExplosionEffect>("ExplosionEffect");

            explosionEffect.Setting(range, attackData, damage * this.damage, DamageApply);

            GameManager.Instance.StartCoroutine(CooltimeCoroutine());
        }
    }
    void DamageApply(AttackData attackData, float damage)
    {
        float tempDamage;

        Collider[] colliders = Physics.OverlapSphere(attackData.position, range, layerMask);

        foreach (Collider collider in colliders)
        {
            if (LuckManager.Calculate(PlayerAttributes.Get(Attribute.CriticalChance), true))
            {
                tempDamage = PlayerAttributes.Get(Attribute.CriticalDamage) * damage;

                attackData.OnCritical();

                IAttackable next = collider.GetComponent<IAttackable>();

                if (next.IsAlive())
                {
                    next.ReceiveHit(damage, true);

                    attackData.OnHit(damage, next.GetPosition(), (next.GetPosition() - attackData.position).normalized);

                    if (!next.IsAlive()) attackData.OnKill();
                }
            }
            else
            {
                IAttackable next = collider.GetComponent<IAttackable>();

                if (next.IsAlive())
                {
                    next.ReceiveHit(damage);

                    attackData.OnHit(damage, next.GetPosition(), (next.GetPosition() - attackData.position).normalized);

                    if (!next.IsAlive()) attackData.OnKill();
                }
            }

            
        }
    }
    IEnumerator CooltimeCoroutine()
    {
        yield return new WaitForSeconds(cooltime);

        isCharged = true;
    }
    public override ICondition GetCondition() => this;
}
