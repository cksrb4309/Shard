using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageOverExplosion", menuName = "Ability/Tear2/DamageOverExplosion")]
public class DamageOverExplosion : Ability
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

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.DamageEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        isCharged = true;

        range = startRange + stackRange * (count - 1);

        damage = startDamage + stackDamage * (count - 1);
    }
    public override void OnEvent(AttackData attackData)
    {
        if (!attackData.CanApplyAttack(this)) return;

        if (!isCharged) return;

        if (attackData.Value > PlayerAttributes.Get(Attribute.AttackDamage) * useLimitValue)
        {
            attackData.Add(this);

            attackData = AttackData.GetAttackData(attackData);

            isCharged = false;

            ExplosionEffect explosionEffect = PoolingManager.Instance.GetObject<ExplosionEffect>("ExplosionEffect");

            explosionEffect.Setting(range, attackData, attackData.Value * damage, DamageApply);

            GameManager.Instance.StartCoroutine(CooltimeCoroutine());
        }
    }
    void DamageApply(AttackData attackData, float damage)
    {
        attackData.Value = damage;

        List<IAttackable> attackables = new List<IAttackable>();

        int count = AttackableTargetSelector.CollectTargetsInRadiusNonAlloc(attackData.Position, range, attackables);

        for (int i = 0; i < count; i++)
            attackData.OnHit(attackables[i], this, false);

        AttackData.ReleaseAttackData(attackData);
    }
    IEnumerator CooltimeCoroutine()
    {
        yield return new WaitForSeconds(cooltime);

        isCharged = true;
    }
}
